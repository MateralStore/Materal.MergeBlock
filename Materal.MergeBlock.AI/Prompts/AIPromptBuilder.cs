namespace Materal.MergeBlock.AI.Prompts;

/// <summary>
/// AI提示词构建器
/// </summary>
/// <param name="contributors">提示词贡献器</param>
public class AIPromptBuilder(IEnumerable<IAIPromptContributor> contributors)
{
    /// <summary>
    /// 构建系统提示词
    /// </summary>
    /// <param name="context">只读AI上下文</param>
    /// <returns>系统提示词列表</returns>
    public async Task<IReadOnlyList<string>> BuildSystemMessagesAsync(IReadOnlyAIContext context)
    {
        AIPromptContributionContext promptContext = new(context);
        foreach (IAIPromptContributor contributor in contributors)
        {
            await contributor.ContributeAsync(promptContext);
        }
        return promptContext.SystemMessages;
    }
}
