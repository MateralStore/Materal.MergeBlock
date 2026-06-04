namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// Agent检查点记录
/// </summary>
public class AgentCheckpointRecord
{
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 恢复元数据
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();
    /// <summary>
    /// 已脱敏模型配置摘要
    /// </summary>
    public IReadOnlyDictionary<string, object?>? ModelConfigSummary { get; init; }
}
