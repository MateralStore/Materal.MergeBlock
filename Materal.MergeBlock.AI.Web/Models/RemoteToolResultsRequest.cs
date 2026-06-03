namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// 远程工具结果请求
/// </summary>
public class RemoteToolResultsRequest
{
    /// <summary>
    /// 契约版本
    /// </summary>
    public string SchemaVersion { get; init; } = "remote-tool-results-v1";
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 工具结果
    /// </summary>
    public IReadOnlyList<RemoteToolResultItem> ToolResults { get; init; } = [];
}

/// <summary>
/// 远程工具结果项
/// </summary>
public class RemoteToolResultItem
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
