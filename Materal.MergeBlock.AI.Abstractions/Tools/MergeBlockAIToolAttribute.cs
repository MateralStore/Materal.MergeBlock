namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// MergeBlock AI工具标记
/// </summary>
/// <param name="description">工具描述</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class MergeBlockAIToolAttribute(string description) : Attribute
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string? Name { get; init; }
    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description { get; } = description;
    /// <summary>
    /// 执行模式
    /// </summary>
    public AIToolExecutionMode ExecutionMode { get; init; } = AIToolExecutionMode.Local;
    /// <summary>
    /// 所需权限
    /// </summary>
    public string? RequiredPermission { get; init; }
}
