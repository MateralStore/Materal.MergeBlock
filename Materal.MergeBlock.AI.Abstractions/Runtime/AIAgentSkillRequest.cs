using System.Text.Json.Serialization;

namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent能力请求
/// </summary>
public class AIAgentSkillRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
}
