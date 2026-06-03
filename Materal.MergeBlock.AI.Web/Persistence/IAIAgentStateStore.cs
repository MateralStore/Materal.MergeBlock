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
    /// 获取运行追踪
    /// </summary>
    Task<AgentRunTrace> GetRunTraceAsync(string runId);
}
