namespace Materal.MergeBlock.AI.Abstractions.Review;

/// <summary>
/// AI Agent执行前审查器
/// </summary>
public interface IAIAgentPreExecutionReviewer
{
    /// <summary>
    /// 审查工具调用
    /// </summary>
    Task<AIAgentPreExecutionReviewResult> ReviewAsync(AIAgentPreExecutionReviewRequest request);
}
