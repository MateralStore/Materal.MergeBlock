using System.Text.Json.Serialization;

namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent执行前审查配置
/// </summary>
public class AIAgentPreExecutionReviewConfig
{
    /// <summary>
    /// 是否启用
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }
}
