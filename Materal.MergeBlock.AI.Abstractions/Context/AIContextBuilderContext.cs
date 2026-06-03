namespace Materal.MergeBlock.AI.Abstractions.Context;

/// <summary>
/// AI上下文构建上下文
/// </summary>
/// <param name="serviceProvider">服务提供器</param>
public sealed class AIContextBuilderContext(IServiceProvider serviceProvider)
{
    /// <summary>
    /// 服务提供器
    /// </summary>
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    /// <summary>
    /// 可写上下文项目
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();
    /// <summary>
    /// 冻结为只读上下文
    /// </summary>
    /// <returns>只读AI上下文</returns>
    public IReadOnlyAIContext Freeze() => new AIContextSnapshot(Items);
}
