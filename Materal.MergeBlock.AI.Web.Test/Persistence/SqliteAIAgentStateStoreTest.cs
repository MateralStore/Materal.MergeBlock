using Materal.MergeBlock.AI.Web.Persistence;
using Materal.MergeBlock.AI.Web.RemoteTools;

namespace Materal.MergeBlock.AI.Web.Test.Persistence;

[TestClass]
public class SqliteAIAgentStateStoreTest
{
    [TestMethod]
    public async Task GetRunTraceAsync_ShouldReturnRunEventsAndToolCalls()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sqlite3");
        SqliteAIAgentStateStore store = new(databasePath);
        await store.InitializeAsync();

        await store.UpsertSessionAsync("thread_001");
        await store.StartRunAsync("run_001", "thread_001");
        await store.RecordStreamEventAsync(new AgentStreamEvent
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Seq = 1,
            Event = "run.started",
            Payload = new Dictionary<string, object?> { ["message"] = "started" }
        });
        await store.RecordToolCallAsync(new RemoteToolPendingCall
        {
            ToolCallId = "call_001",
            ThreadId = "thread_001",
            RunId = "run_001",
            ToolName = "runClientAction",
            Status = "requested",
            Arguments = new Dictionary<string, object?> { ["script"] = "return 1;" }
        });
        await store.CompleteRunAsync("run_001", "waiting_tool_result");

        AgentRunTrace trace = await store.GetRunTraceAsync("run_001");

        Assert.AreEqual("run_001", trace.Run.RunId);
        Assert.AreEqual("thread_001", trace.Run.ThreadId);
        Assert.AreEqual("waiting_tool_result", trace.Run.Status);
        Assert.AreEqual(1, trace.Events.Count);
        Assert.AreEqual("run.started", trace.Events[0].Event);
        Assert.AreEqual(1, trace.ToolCalls.Count);
        Assert.AreEqual("call_001", trace.ToolCalls[0].ToolCallId);
    }

    [TestMethod]
    public async Task RecordToolResultAsync_ShouldUpdateToolCallResult()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sqlite3");
        SqliteAIAgentStateStore store = new(databasePath);
        await store.InitializeAsync();
        await store.UpsertSessionAsync("thread_001");
        await store.StartRunAsync("run_001", "thread_001");
        await store.RecordToolCallAsync(new RemoteToolPendingCall
        {
            ToolCallId = "call_001",
            ThreadId = "thread_001",
            RunId = "run_001",
            ToolName = "runClientAction",
            Status = AIToolCallStatus.Requested,
            Arguments = new Dictionary<string, object?> { ["kind"] = "demo" }
        });

        await store.RecordToolResultAsync(new RemoteToolResultItem
        {
            ToolCallId = "call_001",
            Status = AIToolCallStatus.Completed,
            Result = new Dictionary<string, object?> { ["ok"] = true }
        });

        AgentRunTrace trace = await store.GetRunTraceAsync("run_001");

        Assert.AreEqual(AIToolCallStatus.Completed, trace.ToolCalls[0].Status);
        Assert.IsNotNull(trace.ToolCalls[0].Result);
        Assert.AreEqual("True", trace.ToolCalls[0].Result!["ok"]?.ToString());
    }

    [TestMethod]
    public async Task GetRunTraceAsync_ShouldReturnMessagesScriptReviewAndCheckpoint()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sqlite3");
        SqliteAIAgentStateStore store = new(databasePath);
        await store.InitializeAsync();
        await store.UpsertSessionAsync("thread_001");
        await store.StartRunAsync("run_001", "thread_001");

        await store.RecordMessageAsync(new AgentMessageRecord
        {
            Id = "message_user_001",
            ThreadId = "thread_001",
            RunId = "run_001",
            Role = "user",
            Content = new Dictionary<string, object?> { ["text"] = "hello" }
        });
        await store.RecordMessageAsync(new AgentMessageRecord
        {
            Id = "message_assistant_001",
            ThreadId = "thread_001",
            RunId = "run_001",
            Role = "assistant",
            Content = new Dictionary<string, object?> { ["text"] = "hi" }
        });
        await store.RecordScriptReviewAsync(new ScriptReviewResult
        {
            Id = "review_001",
            ThreadId = "thread_001",
            RunId = "run_001",
            ToolCallId = "call_001",
            Approved = false,
            Reason = "脚本需要重写",
            RiskLevel = "medium"
        });
        await store.RecordCheckpointAsync(
            "run_001",
            new Dictionary<string, object?> { ["resume_token"] = "token_001" },
            new Dictionary<string, object?> { ["model"] = "glm-5.1", ["api_key"] = "***" });

        AgentRunTrace trace = await store.GetRunTraceAsync("run_001");

        Assert.AreEqual(2, trace.Messages.Count);
        Assert.AreEqual("user", trace.Messages[0].Role);
        Assert.AreEqual("hello", trace.Messages[0].Content["text"]?.ToString());
        Assert.AreEqual(1, trace.ScriptReviews.Count);
        Assert.AreEqual("call_001", trace.ScriptReviews[0].ToolCallId);
        Assert.IsFalse(trace.ScriptReviews[0].Approved);
        Assert.IsNotNull(trace.Checkpoint);
        Assert.AreEqual("token_001", trace.Checkpoint!.Metadata["resume_token"]?.ToString());
        Assert.AreEqual("glm-5.1", trace.Checkpoint.ModelConfigSummary!["model"]?.ToString());
        Assert.IsFalse(trace.Checkpoint.ModelConfigSummary.ContainsKey("api_key"));
    }

    [TestMethod]
    public async Task GetRunTraceAsync_ShouldReturnTimelineAndRedactedModelConfig()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sqlite3");
        SqliteAIAgentStateStore store = new(databasePath);
        await store.InitializeAsync();
        await store.UpsertSessionAsync("thread_001");
        await store.StartRunAsync("run_001", "thread_001");
        await store.RecordCheckpointAsync(
            "run_001",
            new Dictionary<string, object?> { ["phase"] = "started" },
            new Dictionary<string, object?>
            {
                ["provider"] = "openai",
                ["model"] = "gpt-test",
                ["api_key"] = "secret"
            });
        await store.RecordMessageAsync(new AgentMessageRecord
        {
            Id = "message_001",
            ThreadId = "thread_001",
            RunId = "run_001",
            Role = "user",
            Content = new Dictionary<string, object?> { ["text"] = "hello" }
        });
        await store.RecordStreamEventAsync(new AgentStreamEvent
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Seq = 1,
            Event = "run.started",
            Payload = new Dictionary<string, object?>()
        });

        AgentRunTrace trace = await store.GetRunTraceAsync("run_001");

        Assert.IsNotNull(trace.Checkpoint);
        Assert.IsNotNull(trace.Checkpoint.ModelConfigSummary);
        Assert.IsFalse(trace.Checkpoint.ModelConfigSummary.ContainsKey("api_key"));
        Assert.IsTrue(trace.Timeline.Count >= 2);
    }
}
