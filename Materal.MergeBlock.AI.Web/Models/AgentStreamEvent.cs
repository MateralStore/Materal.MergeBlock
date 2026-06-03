namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent流式事件
/// </summary>
public class AgentStreamEvent
{
    /// <summary>
    /// 契约版本
    /// </summary>
    public string SchemaVersion { get; init; } = "agent-stream-v1";
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 序号
    /// </summary>
    public int Seq { get; init; }
    /// <summary>
    /// 事件名称
    /// </summary>
    public string Event { get; init; } = string.Empty;
    /// <summary>
    /// 载荷
    /// </summary>
    public IReadOnlyDictionary<string, object?> Payload { get; init; } = new Dictionary<string, object?>();
}
