using System.Text.Json.Serialization;

namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// 取消Agent运行请求
/// </summary>
public class CancelAgentRunRequest
{
    /// <summary>
    /// 会话ID
    /// </summary>
    [JsonPropertyName("thread_id")]
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 取消原因
    /// </summary>
    [JsonPropertyName("reason")]
    public string Reason { get; init; } = "user_requested";
    /// <summary>
    /// 来源
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; init; } = "agent_chat_ui";
}
