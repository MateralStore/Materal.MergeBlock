namespace Materal.MergeBlock.AI.Extensions;

/// <summary>
/// 依赖注入扩展
/// </summary>
public static class DIExtensions
{
    /// <summary>
    /// 添加MergeBlock AI
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMergeBlockAI(this IServiceCollection services)
    {
        services.AddSingleton<AIToolRegistry>();
        services.AddSingleton<AIToolScanner>();
        services.AddSingleton<AIContextBuilder>();
        services.AddSingleton<AIPromptBuilder>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAIToolCallAuditor, LoggingAIToolCallAuditor>());
        return services;
    }
}
