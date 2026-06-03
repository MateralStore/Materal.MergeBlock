namespace Materal.MergeBlock.AI.Context;

/// <summary>
/// AI上下文构建器
/// </summary>
/// <param name="serviceProvider">服务提供器</param>
/// <param name="providers">上下文提供器</param>
public class AIContextBuilder(IServiceProvider serviceProvider, IEnumerable<IAIContextProvider> providers)
{
    /// <summary>
    /// 构建上下文
    /// </summary>
    /// <returns>只读AI上下文</returns>
    public async Task<IReadOnlyAIContext> BuildAsync()
    {
        AIContextBuilderContext context = new(serviceProvider);
        foreach (IAIContextProvider provider in providers)
        {
            await provider.ProvideAsync(context);
        }
        return context.Freeze();
    }
}
