using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MMB.Demo.Abstractions.Enums;
using MMB.Demo.Application.Hubs;
using MMB.Demo.Repository;
using System.Security.Claims;

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
        context.Services.AddAuthorizationBuilder()
        .AddPolicy(DemoAuthorizationPolicies.AdminOnly, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireAssertion(handlerContext => handlerContext.User.Claims.Any(IsAdminRoleClaim));
        });
    }
    private static bool IsAdminRoleClaim(Claim claim)
    {
        bool isRoleClaim = claim.Type == ClaimTypes.Role || string.Equals(claim.Type, "role", StringComparison.OrdinalIgnoreCase) || string.Equals(claim.Type, "Role", StringComparison.OrdinalIgnoreCase);
        if (!isRoleClaim) return false;
        return string.Equals(claim.Value, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase) || claim.Value == ((int)UserRole.Admin).ToString();
    }
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        base.OnApplicationInitialization(context);
        AdvancedContext advancedContext = context.ServiceProvider.GetRequiredService<AdvancedContext>();
        if (advancedContext.App is not WebApplication webApplication) return;
        webApplication.MapHub<TestHub>("/hubs/test");
    }
}
