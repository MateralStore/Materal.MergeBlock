using Materal.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Materal.MergeBlock.ExceptionInterceptor
{
    /// <summary>
    /// 流式输出异常处理中间件
    /// </summary>
    /// <remarks>
    /// 用于捕获 IAsyncEnumerable 流式输出过程中发生的异常。
    /// 由于流式响应已经开始写入，无法修改 HTTP 状态码，
    /// 只能在响应流中写入错误信息或记录日志。
    /// </remarks>
    public partial class StreamingExceptionMiddleware(RequestDelegate next, IOptionsMonitor<ExceptionOptions> exceptionConfig, ILogger<StreamingExceptionMiddleware>? logger = null)
    {
        /// <summary>
        /// 调用
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex) when (context.Response.HasStarted)
            {
                // 响应已开始，无法修改状态码，只能记录日志并尝试写入错误信息
                try
                {
                    string message;
                    if (ex is MergeBlockModuleException or ValidationException or HttpCodeException)
                    {
                        if (ex is AggregateException aggregateException)
                        {
                            ex = aggregateException.InnerException ?? ex;
                        }
                        message = ex.Message;
                    }
                    else
                    {
                        message = ex.GetErrorMessage();
                    }
                    logger?.LogError(ex, "流式输出过程中发生异常");
                    if (!exceptionConfig.CurrentValue.ShowException)
                    {
                        message = exceptionConfig.CurrentValue.ErrorMessage;
                    }
                    ResultModel resultModel = ResultModel.Fail(message);
                    string errorJson = resultModel.ToJson();
                    await context.Response.WriteAsync($"\n[!!STREAM_ERROR_START!!]{errorJson}[!!STREAM_ERROR_END!!]\n");
                }
                catch
                {
                    // 如果写入失败，忽略（连接可能已断开）
                }
            }
        }
    }
}
