using Materal.MergeBlock.Abstractions.Extensions;
using Microsoft.AspNetCore.Builder;

namespace Materal.MergeBlock.ExceptionInterceptor
{
    /// <summary>
    /// 异常拦截器模块
    /// </summary>
    public class ExceptionInterceptorModule() : MergeBlockModule("异常拦截模块")
    {

        /// <inheritdoc/>
        public override void OnConfigureServices(ServiceConfigurationContext context)
        {
            IMvcBuilder? mvcBuilder = context.Services.GetSingletonInstance<IMvcBuilder>();
            if (mvcBuilder is null) return;
            mvcBuilder.AddMvcOptions(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });
        }

        /// <inheritdoc/>
        public override void OnPostConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddHostedServiceDecorator<ExceptionInterceptorHostedServiceDecorator>();
        }

        /// <inheritdoc/>
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            AdvancedContext advancedContext = context.ServiceProvider.GetRequiredService<AdvancedContext>();
            if (advancedContext.App is IApplicationBuilder app)
            {
                app.UseMiddleware<StreamingExceptionMiddleware>();
            }
        }
    }
}
