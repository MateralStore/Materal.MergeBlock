namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// Agent运行追踪
/// </summary>
public class AgentRunTrace
{
    /// <summary>
    /// 运行记录
    /// </summary>
    public AgentRunRecord Run { get; init; } = new();
    /// <summary>
    /// 流式事件
    /// </summary>
    public IReadOnlyList<AgentStreamEvent> Events { get; init; } = [];
    /// <summary>
    /// 工具调用
    /// </summary>
    public IReadOnlyList<RemoteToolPendingCall> ToolCalls { get; init; } = [];
    /// <summary>
    /// 消息记录
    /// </summary>
    public IReadOnlyList<AgentMessageRecord> Messages { get; init; } = [];
    /// <summary>
    /// 脚本审查结果
    /// </summary>
    public IReadOnlyList<ScriptReviewResult> ScriptReviews { get; init; } = [];
    /// <summary>
    /// 时间线
    /// </summary>
    public IReadOnlyList<AgentTimelineItem> Timeline { get; init; } = [];
    /// <summary>
    /// 检查点
    /// </summary>
    public AgentCheckpointRecord? Checkpoint { get; init; }
}
