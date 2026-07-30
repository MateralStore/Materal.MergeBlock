namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI工具契约Schema
/// </summary>
public class AIToolContractSchema
{
    /// <summary>
    /// Schema内容
    /// </summary>
    public IReadOnlyDictionary<string, object?> Schema { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// 创建对象Schema
    /// </summary>
    public static AIToolContractSchema Object(
        IReadOnlyDictionary<string, object?> properties,
        IReadOnlyList<string>? required = null)
    {
        return new AIToolContractSchema
        {
            Schema = new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = required ?? [],
                ["additionalProperties"] = false
            }
        };
    }

    /// <summary>
    /// 创建通用对象Schema
    /// </summary>
    public static AIToolContractSchema GenericObject()
    {
        return new AIToolContractSchema
        {
            Schema = new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["additionalProperties"] = true
            }
        };
    }
}
