using ProjectName.ModuleName.Repository;
using Materal.MergeBlock.Consul.Abstractions;

namespace ProjectName.ModuleName.Application;

/// <summary>
/// ModuleName模块
/// </summary>
[DependsOn(typeof(ModuleNameRepositoryModule))]
public class ModuleNameModule() : ProjectNameModule("ProjectNameModuleName模块")
{
    /// <inheritdoc/>
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        base.OnConfigureServices(context);
        context.Services.AddConsulConfig("ProjectNameModuleName", ["ProjectName.ModuleName"]);
    }
}