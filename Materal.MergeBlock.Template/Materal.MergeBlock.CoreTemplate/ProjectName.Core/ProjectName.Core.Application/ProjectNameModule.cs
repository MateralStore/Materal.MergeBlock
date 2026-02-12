using Materal.MergeBlock.Abstractions;
using Materal.Utils.Extensions;
using Microsoft.Extensions.Configuration;

namespace ProjectName.Core.Application;

/// <summary>
/// ProjectName模块
/// </summary>
public abstract class ProjectNameModule(string moduleName) : MergeBlockModule(moduleName)
{
    /// <inheritdoc/>
    public override void OnPreConfigureServices(ServiceConfigurationContext context)
    {
        if (context.Configuration is not IConfigurationBuilder configurationBuilder) return;
        Type moduleType = GetType();
        string configFilePath = moduleType.Assembly.GetDirectoryPath();
        configFilePath = Path.Combine(configFilePath, $"{moduleType.Namespace}.json");
        configurationBuilder.AddJsonFile(configFilePath, optional: true, reloadOnChange: true);
    }
}
