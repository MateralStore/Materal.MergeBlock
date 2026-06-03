namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent对话请求
/// </summary>
public class AgentChatRequest
{
    /// <summary>
    /// 契约版本
    /// </summary>
    public string SchemaVersion { get; init; } = "agent-chat-request-v1";
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string? RunId { get; init; }
    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
