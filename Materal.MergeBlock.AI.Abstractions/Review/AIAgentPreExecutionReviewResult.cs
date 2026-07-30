namespace Materal.MergeBlock.AI.Abstractions.Review;

/// <summary>
/// AI Agent执行前审查结果
/// </summary>
public class AIAgentPreExecutionReviewResult
{
    /// <summary>
    /// 是否通过
    /// </summary>
    public bool Approved { get; init; }
    /// <summary>
    /// 决策
    /// </summary>
    public string Decision { get; init; } = "reject";
    /// <summary>
    /// 原因
    /// </summary>
    public string Reason { get; init; } = string.Empty;
    /// <summary>
    /// 给Agent的错误消息
    /// </summary>
    public string AgentErrorMessage { get; init; } = string.Empty;
    /// <summary>
    /// 违规项
    /// </summary>
    public IReadOnlyList<string> Violations { get; init; } = [];

    /// <summary>
    /// 创建通过结果
    /// </summary>
    public static AIAgentPreExecutionReviewResult ApprovedResult(string reason) => Approve(reason);

    /// <summary>
    /// 创建通过结果
    /// </summary>
    public static AIAgentPreExecutionReviewResult Approve(string reason) => new()
    {
        Approved = true,
        Decision = "approve",
        Reason = reason
    };

    /// <summary>
    /// 创建拒绝结果
    /// </summary>
    public static AIAgentPreExecutionReviewResult Rejected(string reason, string agentErrorMessage, IReadOnlyList<string>? violations = null) => new()
    {
        Approved = false,
        Decision = "reject",
        Reason = reason,
        AgentErrorMessage = agentErrorMessage,
        Violations = violations ?? []
    };
}
