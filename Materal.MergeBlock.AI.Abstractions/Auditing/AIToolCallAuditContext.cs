namespace Materal.MergeBlock.AI.Abstractions.Auditing;

/// <summary>
/// AI工具调用审计上下文
/// </summary>
public class AIToolCallAuditContext
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string ToolName { get; init; } = string.Empty;
    /// <summary>
    /// 执行模式
    /// </summary>
    public AIToolExecutionMode ExecutionMode { get; init; }
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// 元数据
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();
}
