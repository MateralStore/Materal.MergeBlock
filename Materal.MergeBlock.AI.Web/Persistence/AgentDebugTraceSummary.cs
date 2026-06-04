namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// Agent调试追踪摘要
/// </summary>
public class AgentDebugTraceSummary
{
    /// <summary>
    /// 追踪ID
    /// </summary>
    public string TraceId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; init; }
}
