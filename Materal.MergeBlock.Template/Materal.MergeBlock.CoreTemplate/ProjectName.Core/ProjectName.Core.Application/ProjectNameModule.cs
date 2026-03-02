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
        string configFileName = $"{moduleType.Namespace}.json";
        string configFullPath = Path.Combine(configFilePath, configFileName);
        if (File.Exists(configFullPath))
        {
            configurationBuilder.AddJsonFile(configFullPath, optional: true, reloadOnChange: true);
        }
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;
        if (environment.Equals(Environments.Development, StringComparison.OrdinalIgnoreCase))
        {
            string developmentConfigFileName = $"{moduleType.Namespace}.Development.json";
            string developmentConfigFullPath = Path.Combine(configFilePath, developmentConfigFileName);
            if (File.Exists(developmentConfigFullPath))
            {
                configurationBuilder.AddJsonFile(developmentConfigFullPath, optional: true, reloadOnChange: true);
            }
        }
    }
}
