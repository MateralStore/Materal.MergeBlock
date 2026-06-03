namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// SQLite AI Agent状态存储
/// </summary>
public class SqliteAIAgentStateStore(string databasePath) : IAIAgentStateStore
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly string _connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        string? directory = Path.GetDirectoryName(Path.GetFullPath(databasePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();
        foreach (string sql in GetInitializeSql())
        {
            await using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }
    }
    /// <inheritdoc />
    public async Task UpsertSessionAsync(string threadId)
    {
        string now = GetNow();
        await ExecuteNonQueryAsync("""
            insert into ai_agent_sessions(thread_id, created_at, updated_at)
            values($thread_id, $now, $now)
            on conflict(thread_id) do update set updated_at = excluded.updated_at;
            """,
            command =>
            {
                command.Parameters.AddWithValue("$thread_id", threadId);
                command.Parameters.AddWithValue("$now", now);
            });
    }
    /// <inheritdoc />
    public async Task StartRunAsync(string runId, string threadId)
    {
        await ExecuteNonQueryAsync("""
            insert into ai_agent_runs(run_id, thread_id, status, started_at)
            values($run_id, $thread_id, $status, $started_at);
            """,
            command =>
            {
                command.Parameters.AddWithValue("$run_id", runId);
                command.Parameters.AddWithValue("$thread_id", threadId);
                command.Parameters.AddWithValue("$status", "running");
                command.Parameters.AddWithValue("$started_at", GetNow());
            });
    }
    /// <inheritdoc />
    public async Task CompleteRunAsync(string runId, string status, string? errorMessage = null)
    {
        await ExecuteNonQueryAsync("""
            update ai_agent_runs
            set status = $status,
                completed_at = $completed_at,
                error_message = $error_message
            where run_id = $run_id;
            """,
            command =>
            {
                command.Parameters.AddWithValue("$run_id", runId);
                command.Parameters.AddWithValue("$status", status);
                command.Parameters.AddWithValue("$completed_at", GetNow());
                command.Parameters.AddWithValue("$error_message", (object?)errorMessage ?? DBNull.Value);
            });
    }
    /// <inheritdoc />
    public async Task RecordStreamEventAsync(AgentStreamEvent streamEvent)
    {
        await ExecuteNonQueryAsync("""
            insert into ai_agent_stream_events(thread_id, run_id, seq, event_type, payload_json, created_at)
            values($thread_id, $run_id, $seq, $event_type, $payload_json, $created_at);
            """,
            command =>
            {
                command.Parameters.AddWithValue("$thread_id", streamEvent.ThreadId);
                command.Parameters.AddWithValue("$run_id", streamEvent.RunId);
                command.Parameters.AddWithValue("$seq", streamEvent.Seq);
                command.Parameters.AddWithValue("$event_type", streamEvent.Event);
                command.Parameters.AddWithValue("$payload_json", Serialize(streamEvent.Payload));
                command.Parameters.AddWithValue("$created_at", GetNow());
            });
    }
    /// <inheritdoc />
    public async Task RecordToolCallAsync(RemoteToolPendingCall toolCall)
    {
        await ExecuteNonQueryAsync("""
            insert into ai_agent_tool_calls(id, thread_id, run_id, tool_name, status, arguments_json, result_json, error_json, created_at, completed_at)
            values($id, $thread_id, $run_id, $tool_name, $status, $arguments_json, $result_json, $error_json, $created_at, $completed_at)
            on conflict(id) do update set
                status = excluded.status,
                result_json = excluded.result_json,
                error_json = excluded.error_json,
                completed_at = excluded.completed_at;
            """,
            command =>
            {
                command.Parameters.AddWithValue("$id", toolCall.ToolCallId);
                command.Parameters.AddWithValue("$thread_id", toolCall.ThreadId);
                command.Parameters.AddWithValue("$run_id", toolCall.RunId);
                command.Parameters.AddWithValue("$tool_name", toolCall.ToolName);
                command.Parameters.AddWithValue("$status", toolCall.Status);
                command.Parameters.AddWithValue("$arguments_json", (object?)Serialize(toolCall.Arguments) ?? DBNull.Value);
                command.Parameters.AddWithValue("$result_json", (object?)Serialize(toolCall.Result) ?? DBNull.Value);
                command.Parameters.AddWithValue("$error_json", (object?)Serialize(toolCall.Error) ?? DBNull.Value);
                command.Parameters.AddWithValue("$created_at", GetNow());
                command.Parameters.AddWithValue("$completed_at", IsPending(toolCall.Status) ? DBNull.Value : GetNow());
            });
    }
    /// <inheritdoc />
    public async Task RecordToolResultAsync(RemoteToolResultItem toolResult)
    {
        await ExecuteNonQueryAsync("""
            update ai_agent_tool_calls
            set status = $status,
                result_json = $result_json,
                error_json = $error_json,
                completed_at = $completed_at
            where id = $id;
            """,
            command =>
            {
                command.Parameters.AddWithValue("$id", toolResult.ToolCallId);
                command.Parameters.AddWithValue("$status", toolResult.Status);
                command.Parameters.AddWithValue("$result_json", (object?)Serialize(toolResult.Result) ?? DBNull.Value);
                command.Parameters.AddWithValue("$error_json", (object?)Serialize(toolResult.Error) ?? DBNull.Value);
                command.Parameters.AddWithValue("$completed_at", GetNow());
            });
    }
    /// <inheritdoc />
    public async Task<AgentRunTrace> GetRunTraceAsync(string runId)
    {
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();
        AgentRunRecord run = await GetRunAsync(connection, runId);
        List<AgentStreamEvent> events = await GetEventsAsync(connection, runId);
        List<RemoteToolPendingCall> toolCalls = await GetToolCallsAsync(connection, runId);
        return new AgentRunTrace
        {
            Run = run,
            Events = events,
            ToolCalls = toolCalls
        };
    }
    private async Task<AgentRunRecord> GetRunAsync(SqliteConnection connection, string runId)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "select run_id, thread_id, status, error_message from ai_agent_runs where run_id = $run_id;";
        command.Parameters.AddWithValue("$run_id", runId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new KeyNotFoundException($"未找到AI Agent运行记录: {runId}");
        }
        return new AgentRunRecord
        {
            RunId = reader.GetString(0),
            ThreadId = reader.GetString(1),
            Status = reader.GetString(2),
            ErrorMessage = reader.IsDBNull(3) ? null : reader.GetString(3)
        };
    }
    private async Task<List<AgentStreamEvent>> GetEventsAsync(SqliteConnection connection, string runId)
    {
        List<AgentStreamEvent> result = [];
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            select thread_id, run_id, seq, event_type, payload_json
            from ai_agent_stream_events
            where run_id = $run_id
            order by seq asc, id asc;
            """;
        command.Parameters.AddWithValue("$run_id", runId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new AgentStreamEvent
            {
                ThreadId = reader.GetString(0),
                RunId = reader.GetString(1),
                Seq = reader.GetInt32(2),
                Event = reader.GetString(3),
                Payload = DeserializeDictionary(reader.GetString(4))
            });
        }
        return result;
    }
    private async Task<List<RemoteToolPendingCall>> GetToolCallsAsync(SqliteConnection connection, string runId)
    {
        List<RemoteToolPendingCall> result = [];
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            select id, thread_id, run_id, tool_name, status, arguments_json, result_json, error_json
            from ai_agent_tool_calls
            where run_id = $run_id
            order by created_at asc, id asc;
            """;
        command.Parameters.AddWithValue("$run_id", runId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new RemoteToolPendingCall
            {
                ToolCallId = reader.GetString(0),
                ThreadId = reader.GetString(1),
                RunId = reader.GetString(2),
                ToolName = reader.GetString(3),
                Status = reader.GetString(4),
                Arguments = reader.IsDBNull(5) ? null : DeserializeDictionary(reader.GetString(5)),
                Result = reader.IsDBNull(6) ? null : DeserializeDictionary(reader.GetString(6)),
                Error = reader.IsDBNull(7) ? null : DeserializeDictionary(reader.GetString(7))
            });
        }
        return result;
    }
    private async Task ExecuteNonQueryAsync(string sql, Action<SqliteCommand> configure)
    {
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        configure(command);
        await command.ExecuteNonQueryAsync();
    }
    private static IEnumerable<string> GetInitializeSql()
    {
        yield return """
            create table if not exists ai_agent_sessions (
              thread_id text primary key,
              created_at text not null,
              updated_at text not null
            );
            """;
        yield return """
            create table if not exists ai_agent_runs (
              run_id text primary key,
              thread_id text not null,
              status text not null,
              started_at text not null,
              completed_at text,
              error_message text
            );
            """;
        yield return """
            create table if not exists ai_agent_stream_events (
              id integer primary key autoincrement,
              thread_id text not null,
              run_id text not null,
              seq integer not null,
              event_type text not null,
              payload_json text not null,
              created_at text not null
            );
            """;
        yield return """
            create table if not exists ai_agent_tool_calls (
              id text primary key,
              thread_id text not null,
              run_id text not null,
              tool_name text not null,
              status text not null,
              arguments_json text,
              result_json text,
              error_json text,
              created_at text not null,
              completed_at text
            );
            """;
    }
    private static string? Serialize(IReadOnlyDictionary<string, object?>? value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, _jsonSerializerOptions);
    }
    private static IReadOnlyDictionary<string, object?> DeserializeDictionary(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, _jsonSerializerOptions) ?? new Dictionary<string, object?>();
    }
    private static bool IsPending(string status) => string.Equals(status, AIToolCallStatus.Requested, StringComparison.OrdinalIgnoreCase);
    private static string GetNow() => DateTimeOffset.UtcNow.ToString("O");
}
