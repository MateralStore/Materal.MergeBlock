namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// Agent会话追踪
/// </summary>
public class AgentSessionTrace
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行记录
    /// </summary>
    public IReadOnlyList<AgentRunRecord> Runs { get; init; } = [];
}
