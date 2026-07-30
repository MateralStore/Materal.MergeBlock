using System.Text.Json.Serialization;

namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent推理配置
/// </summary>
public class AIAgentReasoningConfig
{
    /// <summary>
    /// 是否启用
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }
    /// <summary>
    /// 推理强度
    /// </summary>
    [JsonPropertyName("effort")]
    public string Effort { get; init; } = "medium";
    /// <summary>
    /// 推理预算Token数
    /// </summary>
    [JsonPropertyName("budget_tokens")]
    public int? BudgetTokens { get; init; }
    /// <summary>
    /// 摘要策略
    /// </summary>
    [JsonPropertyName("summary")]
    public string Summary { get; init; } = "auto";
}
