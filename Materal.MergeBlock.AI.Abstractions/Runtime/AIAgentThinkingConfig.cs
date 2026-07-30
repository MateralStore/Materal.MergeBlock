using System.Text.Json.Serialization;

namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent思考配置
/// </summary>
public class AIAgentThinkingConfig
{
    /// <summary>
    /// 是否启用
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }
    /// <summary>
    /// 预算Token数
    /// </summary>
    [JsonPropertyName("budget_tokens")]
    public int BudgetTokens { get; init; } = 1024;
}
