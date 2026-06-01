using Materal.MergeBlock.Abstractions;
using Materal.MergeBlock.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Materal.MergeBlock.Shield;

/// <summary>
/// Shield安全防护模块
/// </summary>
[DependsOn(typeof(WebModule))]
public class ShieldModule() : MergeBlockModule("安全防护模块")
{
    /// <inheritdoc/>
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        IConfigurationSection? section = context.Configuration?.GetSection(ShieldOptions.ConfigKey);
        if (section is not null)
        {
            context.Services.Configure<ShieldOptions>(section);
        }
        context.Services.TryAddSingleton<IShieldStore, MemoryShieldStore>();
        context.Services.TryAddSingleton<ShieldMiddleware>();
    }

    /// <inheritdoc/>
    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        AdvancedContext advancedContext = context.ServiceProvider.GetRequiredService<AdvancedContext>();
        if (advancedContext.App is IApplicationBuilder app)
        {
            app.UseMiddleware<ShieldMiddleware>();
        }
    }
}
