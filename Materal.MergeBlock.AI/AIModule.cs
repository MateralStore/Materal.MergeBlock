namespace Materal.MergeBlock.AI;

/// <summary>
/// AI模块
/// </summary>
public class AIModule() : MergeBlockModule("AI模块")
{
    /// <inheritdoc />
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        if (context.Configuration is not null)
        {
            context.Services.Configure<AIOptions>(context.Configuration.GetSection(AIOptions.ConfigKey));
        }
        context.Services.AddMergeBlockAI();
        base.OnConfigureServices(context);
    }

    /// <inheritdoc />
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        AIOptions options = context.ServiceProvider.GetService<Microsoft.Extensions.Options.IOptions<AIOptions>>()?.Value ?? new AIOptions();
        if (!options.Enable)
        {
            base.OnApplicationInitialization(context);
            return;
        }
        AIToolRegistry registry = context.ServiceProvider.GetRequiredService<AIToolRegistry>();
        if (options.ScanTools)
        {
            AIToolScanner scanner = context.ServiceProvider.GetRequiredService<AIToolScanner>();
            MergeBlockContext? mergeBlockContext = context.ServiceProvider.GetService<MergeBlockContext>();
            Assembly[] assemblies = [.. mergeBlockContext?.MergeBlockAssemblies ?? []];
            foreach (AIToolDescriptor descriptor in scanner.Scan(assemblies))
            {
                registry.Register(descriptor);
            }
        }
        foreach (IAIToolMetadataProvider provider in context.ServiceProvider.GetServices<IAIToolMetadataProvider>())
        {
            foreach (AIToolDescriptor descriptor in provider.GetToolDescriptors())
            {
                registry.Register(descriptor);
            }
        }
        base.OnApplicationInitialization(context);
    }
}
