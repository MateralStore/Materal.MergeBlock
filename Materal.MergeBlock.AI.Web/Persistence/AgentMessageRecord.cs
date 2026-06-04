namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// Agent消息记录
/// </summary>
public class AgentMessageRecord
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 角色
    /// </summary>
    public string Role { get; init; } = string.Empty;
    /// <summary>
    /// 内容
    /// </summary>
    public IReadOnlyDictionary<string, object?> Content { get; init; } = new Dictionary<string, object?>();
}
