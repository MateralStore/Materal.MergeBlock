namespace Materal.MergeBlock.AI.Abstractions.Context;

/// <summary>
/// AI上下文提供器
/// </summary>
public interface IAIContextProvider
{
    /// <summary>
    /// 提供上下文
    /// </summary>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    Task ProvideAsync(AIContextBuilderContext context);
}
