namespace Materal.MergeBlock.AI.Web.RemoteTools;

/// <summary>
/// 远程工具待处理调用
/// </summary>
public class RemoteToolPendingCall
{
    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string ToolCallId { get; init; } = string.Empty;
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 工具名称
    /// </summary>
    public string ToolName { get; init; } = string.Empty;
    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// 参数
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Arguments { get; init; }
    /// <summary>
    /// 结果
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Result { get; init; }
    /// <summary>
    /// 错误
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Error { get; init; }
}
