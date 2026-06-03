namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent远程工具结果
/// </summary>
public class AIAgentRemoteToolResult
{
    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string ToolCallId { get; init; } = string.Empty;
    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// 结果
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Result { get; init; }
    /// <summary>
    /// 错误
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Error { get; init; }
}
