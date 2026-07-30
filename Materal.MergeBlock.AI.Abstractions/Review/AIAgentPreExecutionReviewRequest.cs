using Materal.MergeBlock.AI.Abstractions.Runtime;

namespace Materal.MergeBlock.AI.Abstractions.Review;

/// <summary>
/// AI Agent执行前审查请求
/// </summary>
public class AIAgentPreExecutionReviewRequest
{
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
    /// 工具名称
    /// </summary>
    public string ToolName { get; init; } = string.Empty;
    /// <summary>
    /// 用户消息
    /// </summary>
    public string UserMessage { get; init; } = string.Empty;
    /// <summary>
    /// 工具参数
    /// </summary>
    public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();
    /// <summary>
    /// 模型配置
    /// </summary>
    public AIAgentModelConfig ModelConfig { get; init; } = new();
}
