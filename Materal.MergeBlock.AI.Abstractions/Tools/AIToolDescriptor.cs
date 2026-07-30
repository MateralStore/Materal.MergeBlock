namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI工具描述
/// </summary>
public class AIToolDescriptor
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>
    /// 执行模式
    /// </summary>
    public AIToolExecutionMode ExecutionMode { get; init; }
    /// <summary>
    /// 所需权限
    /// </summary>
    public string? RequiredPermission { get; init; }
    /// <summary>
    /// 权限级别
    /// </summary>
    public AIToolPermissionLevel PermissionLevel { get; init; } = AIToolPermissionLevel.Read;
    /// <summary>
    /// 输入Schema
    /// </summary>
    public AIToolContractSchema? InputSchema { get; init; }
    /// <summary>
    /// 结果Schema
    /// </summary>
    public AIToolContractSchema? ResultSchema { get; init; }
    /// <summary>
    /// 是否需要执行前审查
    /// </summary>
    public bool RequirePreExecutionReview { get; init; }
    /// <summary>
    /// 输入类型
    /// </summary>
    public Type? InputType { get; init; }
    /// <summary>
    /// 结果类型
    /// </summary>
    public Type? ResultType { get; init; }
    /// <summary>
    /// 工具实现类型
    /// </summary>
    public Type? ImplementationType { get; init; }
}
