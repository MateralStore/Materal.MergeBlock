namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// 脚本审查结果
/// </summary>
public class ScriptReviewResult
{
    /// <summary>
    /// 审查ID
    /// </summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string ToolCallId { get; init; } = string.Empty;
    /// <summary>
    /// 是否通过
    /// </summary>
    public bool Approved { get; init; }
    /// <summary>
    /// 原因
    /// </summary>
    public string? Reason { get; init; }
    /// <summary>
    /// 风险等级
    /// </summary>
    public string? RiskLevel { get; init; }
}
