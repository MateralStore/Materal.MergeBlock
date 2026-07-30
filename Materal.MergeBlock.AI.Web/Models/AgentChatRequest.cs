using System.Text.Json.Serialization;

namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent对话请求
/// </summary>
public class AgentChatRequest
{
    /// <summary>
    /// 契约版本
    /// </summary>
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; init; } = "agent-chat-request-v1";
    /// <summary>
    /// 会话ID
    /// </summary>
    [JsonPropertyName("thread_id")]
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    [JsonPropertyName("run_id")]
    public string? RunId { get; init; }
    /// <summary>
    /// 消息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
    /// <summary>
    /// 模型配置
    /// </summary>
    [JsonPropertyName("model_config")]
    public AIAgentModelConfig ModelConfig { get; init; } = new();
    /// <summary>
    /// 顶层推理配置
    /// </summary>
    [JsonPropertyName("reasoning")]
    public AIAgentReasoningConfig? Reasoning { get; init; }
    /// <summary>
    /// 顶层思考配置
    /// </summary>
    [JsonPropertyName("thinking")]
    public AIAgentThinkingConfig? Thinking { get; init; }
    /// <summary>
    /// 能力请求
    /// </summary>
    [JsonPropertyName("skill_request")]
    public AIAgentSkillRequest? SkillRequest { get; init; }
    /// <summary>
    /// 执行前审查配置
    /// </summary>
    [JsonPropertyName("pre_execution_review")]
    public AIAgentPreExecutionReviewConfig PreExecutionReview { get; init; } = new();
    /// <summary>
    /// 上下文
    /// </summary>
    [JsonPropertyName("context")]
    public IReadOnlyDictionary<string, object?> Context { get; init; } = new Dictionary<string, object?>();
    /// <summary>
    /// 扩展元数据
    /// </summary>
    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();
}
