namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// Agent时间线项目
/// </summary>
public class AgentTimelineItem
{
    /// <summary>
    /// 类型
    /// </summary>
    public string Kind { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 序号
    /// </summary>
    public int? Seq { get; init; }
    /// <summary>
    /// 角色
    /// </summary>
    public string? Role { get; init; }
    /// <summary>
    /// 事件名称
    /// </summary>
    public string? Event { get; init; }
    /// <summary>
    /// 载荷
    /// </summary>
    public IReadOnlyDictionary<string, object?> Payload { get; init; } = new Dictionary<string, object?>();
}
