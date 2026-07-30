using System.Text.Json.Serialization;

namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent流式事件
/// </summary>
public class AgentStreamEvent
{
    /// <summary>
    /// 契约版本
    /// </summary>
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; init; } = "agent-stream-v1";
    /// <summary>
    /// 会话ID
    /// </summary>
    [JsonPropertyName("thread_id")]
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    [JsonPropertyName("run_id")]
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 序号
    /// </summary>
    [JsonPropertyName("seq")]
    public int Seq { get; init; }
    /// <summary>
    /// 事件名称
    /// </summary>
    [JsonPropertyName("event")]
    public string Event { get; init; } = string.Empty;
    /// <summary>
    /// 载荷
    /// </summary>
    [JsonPropertyName("payload")]
    public IReadOnlyDictionary<string, object?> Payload { get; init; } = new Dictionary<string, object?>();
}
