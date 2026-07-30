namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI服务端工具上下文
/// </summary>
public class AIServerToolContext
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
    /// 工具参数
    /// </summary>
    public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();
    /// <summary>
    /// AI上下文
    /// </summary>
    public IReadOnlyAIContext? AIContext { get; init; }
    /// <summary>
    /// 取消令牌
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}
