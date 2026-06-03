namespace Materal.MergeBlock.AI.Abstractions.Prompts;

/// <summary>
/// AI提示词贡献上下文
/// </summary>
/// <param name="aiContext">只读AI上下文</param>
public sealed class AIPromptContributionContext(IReadOnlyAIContext aiContext)
{
    private readonly List<string> _systemMessages = [];
    /// <summary>
    /// 只读AI上下文
    /// </summary>
    public IReadOnlyAIContext AIContext { get; } = aiContext;
    /// <summary>
    /// 系统提示词
    /// </summary>
    public IReadOnlyList<string> SystemMessages => _systemMessages;
    /// <summary>
    /// 添加系统提示词
    /// </summary>
    /// <param name="message">提示词</param>
    public void AddSystemMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        _systemMessages.Add(message);
    }
}
