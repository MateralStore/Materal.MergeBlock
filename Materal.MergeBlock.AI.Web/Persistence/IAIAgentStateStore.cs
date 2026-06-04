namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// AI Agent状态存储
/// </summary>
public interface IAIAgentStateStore
{
    /// <summary>
    /// 初始化
    /// </summary>
    Task InitializeAsync();
    /// <summary>
    /// 新增或更新会话
    /// </summary>
    Task UpsertSessionAsync(string threadId);
    /// <summary>
    /// 开始运行
    /// </summary>
    Task StartRunAsync(string runId, string threadId);
    /// <summary>
    /// 完成运行
    /// </summary>
    Task CompleteRunAsync(string runId, string status, string? errorMessage = null);
    /// <summary>
    /// 记录流式事件
    /// </summary>
    Task RecordStreamEventAsync(AgentStreamEvent streamEvent);
    /// <summary>
    /// 记录工具调用
    /// </summary>
    Task RecordToolCallAsync(RemoteToolPendingCall toolCall);
    /// <summary>
    /// 记录工具结果
    /// </summary>
    Task RecordToolResultAsync(RemoteToolResultItem toolResult);
    /// <summary>
    /// 记录消息
    /// </summary>
    Task RecordMessageAsync(AgentMessageRecord message);
    /// <summary>
    /// 记录脚本审查
    /// </summary>
    Task RecordScriptReviewAsync(ScriptReviewResult scriptReviewResult);
    /// <summary>
    /// 记录检查点
    /// </summary>
    Task RecordCheckpointAsync(string runId, IReadOnlyDictionary<string, object?> metadata, IReadOnlyDictionary<string, object?>? modelConfigSummary = null);
    /// <summary>
    /// 获取会话追踪
    /// </summary>
    Task<AgentSessionTrace> GetSessionTraceAsync(string threadId);
    /// <summary>
    /// 获取运行记录
    /// </summary>
    Task<AgentRunRecord> GetRunAsync(string runId);
    /// <summary>
    /// 查询调试追踪摘要
    /// </summary>
    Task<IReadOnlyList<AgentDebugTraceSummary>> ListDebugTracesAsync();
    /// <summary>
    /// 获取运行追踪
    /// </summary>
    Task<AgentRunTrace> GetRunTraceAsync(string runId);
}
