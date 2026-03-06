using Materal.Extensions.DependencyInjection;
using Materal.Extensions.DependencyInjection.AspNetCore;
using Materal.MergeBlock.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Materal.MergeBlock.Web;

/// <summary>
/// MergeBlock程序
/// </summary>
public static class MergeBlockProgram
{
    /// <summary>
    /// 配置
    /// </summary>
    public static event Action<IServiceCollection>? OnConfigureServices;
    /// <summary>
    /// 应用初始化
    /// </summary>
    public static event Action<IServiceProvider>? OnApplicationInitialization;
    /// <summary>
    /// 证书目录（默认当前目录）
    /// </summary>
    public static string CertificateDirectory
    {
        get => KestrelCertificateHelper.CertificateDirectory;
        set => KestrelCertificateHelper.CertificateDirectory = value;
    }
    /// <summary>
    /// 运行
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task RunAsync(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // 清除 URLs 配置，避免与 ConfigureKestrel 冲突
        builder.WebHost.UseSetting(WebHostDefaults.PreventHostingStartupKey, "true");

        builder.AddMateralServiceProvider();
        builder.Services.AddSingleton(builder);
        builder.AddMergeBlockCore(args);

        // 配置 Kestrel 根据 URLs 自动绑定证书
        builder.WebHost.ConfigureKestrel(options =>
        {
            KestrelCertificateHelper.Configure(args, options);
        });

        OnConfigureServices?.Invoke(builder.Services);
        WebApplication app = builder.Build();
        app.UseMergeBlock();
        OnApplicationInitialization?.Invoke(app.Services);
        await app.RunAsync();
    }
}
