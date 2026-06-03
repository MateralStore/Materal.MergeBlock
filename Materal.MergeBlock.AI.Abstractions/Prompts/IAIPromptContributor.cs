namespace Materal.MergeBlock.AI.Abstractions.Prompts;

/// <summary>
/// AI提示词贡献器
/// </summary>
public interface IAIPromptContributor
{
    /// <summary>
    /// 贡献提示词
    /// </summary>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    Task ContributeAsync(AIPromptContributionContext context);
}
