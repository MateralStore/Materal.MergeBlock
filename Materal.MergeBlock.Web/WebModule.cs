using Materal.MergeBlock.Web.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Materal.MergeBlock.Web
{
    /// <summary>
    /// Web模块
    /// </summary>
    public class WebModule() : MergeBlockModule("Web模块")
    {
        /// <summary>
        /// 配置服务前
        /// </summary>
        /// <param name="context"></param>
        public override void OnConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddSignalR();
            IConfigurationSection? section = context.Configuration?.GetSection(WebOptions.ConfigKey);
            WebOptions? webOptions = null;
            if (section is not null)
            {
                context.Services.Configure<WebOptions>(section);
                webOptions = section.Get<WebOptions>();
            }
            IMvcBuilder mvcBuilder = context.Services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = true;
                options.Filters.Add<ActionPageQueryFilterAttribute>();
            }).AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
            context.Services.AddSingleton(mvcBuilder);
            MergeBlockContext? mergeBlockContext = context.Services.GetSingletonInstance<MergeBlockContext>();
            if (mergeBlockContext is not null)
            {
                foreach (Assembly assembly in mergeBlockContext.MergeBlockAssemblies)
                {
                    mvcBuilder.AddApplicationPart(assembly);
                }
            }
            context.Services.AddEndpointsApiExplorer();//添加API探索器
            if (webOptions is not null)
            {
                if (webOptions.HttpBodyMaxSize is not null)
                {
                    context.Services.Configure<KestrelServerOptions>(options =>
                    {
                        options.Limits.MaxRequestBodySize = webOptions.HttpBodyMaxSize.Value;
                    });
                }
            }
            ListeningUris urlsList = [];
            if (context.Configuration is not null)
            {
                string? urlsValue = context.Configuration.Get<string>("URLS");
                if (!string.IsNullOrWhiteSpace(urlsValue))
                {
                    urlsList.AddRange(urlsValue.Split(";").Select(m => new Uri(m)));
                }
                urlsValue = context.Configuration.Get<string>("ASPNETCORE_URLS");
                if (!string.IsNullOrWhiteSpace(urlsValue))
                {
                    foreach (Uri uri in urlsValue.Split(";").Select(m => new Uri(m)))
                    {
                        if (urlsList.Any(m => m.AbsoluteUri == uri.AbsoluteUri)) continue;
                        urlsList.Add(uri);
                    }
                }
            }
            context.Services.AddSingleton(urlsList);
        }
        /// <summary>
        /// 应用程序初始化前
        /// </summary>
        /// <param name="context"></param>
        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            AdvancedContext advancedContext = context.ServiceProvider.GetRequiredService<AdvancedContext>();
            if (advancedContext.App is not WebApplication webApplication) return;
            webApplication.Use(async (httpContext, next) => // 使请求可重复读取
            {
                httpContext.Request.EnableBuffering();
                await next.Invoke(httpContext);
            });
        }
        /// <summary>
        /// 应用程序初始化
        /// </summary>
        /// <param name="context"></param>
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            AdvancedContext advancedContext = context.ServiceProvider.GetRequiredService<AdvancedContext>();
            if (advancedContext.App is not WebApplication webApplication) return;
            webApplication.MapControllers();
            AutoUseHttpsRedirection(webApplication);
        }
        /// <summary>
        /// 自动使用Https重定向
        /// </summary>
        /// <param name="app"></param>
        private static void AutoUseHttpsRedirection(WebApplication app)
        {
            ListeningUris urlsList = app.Services.GetRequiredService<ListeningUris>();
            if (urlsList.Any(m => m.Scheme == "https"))
            {
                app.UseHttpsRedirection(); //启用HTTPS重定向
            }
        }
    }
}
