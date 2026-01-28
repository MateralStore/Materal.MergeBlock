using MMB.Demo.Repository;
using Materal.MergeBlock.Consul.Abstractions;

namespace MMB.Demo.Application;

/// <summary>
/// Demo模块
/// </summary>
[DependsOn(typeof(DemoRepositoryModule))]
public class DemoModule() : MMBModule("MMBDemo模块")
{
    /// <inheritdoc/>
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        base.OnConfigureServices(context);
        context.Services.AddConsulConfig("MMBDemo", ["MMB.Demo"]);
    }
}