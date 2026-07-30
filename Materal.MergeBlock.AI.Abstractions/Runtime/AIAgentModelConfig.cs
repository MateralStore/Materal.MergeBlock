using System.Text.Json.Serialization;

namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent模型配置
/// </summary>
public class AIAgentModelConfig
{
    /// <summary>
    /// Provider名称
    /// </summary>
    [JsonPropertyName("provider")]
    public string Provider { get; init; } = string.Empty;
    /// <summary>
    /// Provider适配器
    /// </summary>
    [JsonPropertyName("adapter")]
    public string? Adapter { get; init; }
    /// <summary>
    /// 模型名称
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;
    /// <summary>
    /// API地址
    /// </summary>
    [JsonPropertyName("base_url")]
    public string? BaseUrl { get; init; }
    /// <summary>
    /// API密钥
    /// </summary>
    [JsonPropertyName("api_key")]
    public string ApiKey { get; init; } = string.Empty;
    /// <summary>
    /// 温度
    /// </summary>
    [JsonPropertyName("temperature")]
    public float Temperature { get; init; } = 0.2f;
    /// <summary>
    /// 最大输出Token数
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; init; } = 1200;
    /// <summary>
    /// 推理配置
    /// </summary>
    [JsonPropertyName("reasoning")]
    public AIAgentReasoningConfig? Reasoning { get; init; }
    /// <summary>
    /// 思考配置
    /// </summary>
    [JsonPropertyName("thinking")]
    public AIAgentThinkingConfig? Thinking { get; init; }
}
