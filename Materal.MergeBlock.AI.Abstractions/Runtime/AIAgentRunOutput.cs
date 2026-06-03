namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent运行输出
/// </summary>
public class AIAgentRunOutput
{
    /// <summary>
    /// 输出类型
    /// </summary>
    public AIAgentRunOutputType Type { get; init; }
    /// <summary>
    /// 文本内容
    /// </summary>
    public string? Text { get; init; }
    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string? ToolCallId { get; init; }
    /// <summary>
    /// 工具名称
    /// </summary>
    public string? ToolName { get; init; }
    /// <summary>
    /// 工具参数
    /// </summary>
    public IReadOnlyDictionary<string, object?>? ToolArguments { get; init; }
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; init; }
    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; init; }
    /// <summary>
    /// 扩展元数据
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
    /// <summary>
    /// 创建文本增量
    /// </summary>
    public static AIAgentRunOutput MessageDelta(string text) => new()
    {
        Type = AIAgentRunOutputType.MessageDelta,
        Text = text
    };
    /// <summary>
    /// 创建工具调用请求
    /// </summary>
    public static AIAgentRunOutput ToolCallRequested(string toolCallId, string toolName, IReadOnlyDictionary<string, object?>? arguments = null) => new()
    {
        Type = AIAgentRunOutputType.ToolCallRequested,
        ToolCallId = toolCallId,
        ToolName = toolName,
        ToolArguments = arguments
    };
    /// <summary>
    /// 创建运行暂停
    /// </summary>
    public static AIAgentRunOutput RunPaused() => new()
    {
        Type = AIAgentRunOutputType.RunPaused
    };
    /// <summary>
    /// 创建运行完成
    /// </summary>
    public static AIAgentRunOutput RunCompleted() => new()
    {
        Type = AIAgentRunOutputType.RunCompleted
    };
    /// <summary>
    /// 创建错误
    /// </summary>
    public static AIAgentRunOutput Error(string message, string code = "runtime_error") => new()
    {
        Type = AIAgentRunOutputType.Error,
        ErrorMessage = message,
        ErrorCode = code
    };
}
