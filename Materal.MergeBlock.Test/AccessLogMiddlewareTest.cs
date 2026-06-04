using Materal.MergeBlock.AccessLog;
using Materal.MergeBlock.AccessLog.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Materal.MergeBlock.Test;

[TestClass]
public sealed class AccessLogMiddlewareTest
{
    [TestMethod]
    public async Task InvokeAsync_ShouldNotBufferSseResponse()
    {
        FakeAccessLogService accessLogService = new();
        AccessLogMiddleware middleware = new(accessLogService);
        DefaultHttpContext context = new();
        MemoryStream responseBody = new();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/agent/chat/stream";
        context.Request.Headers.Accept = "text/event-stream";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        context.Response.Body = responseBody;
        long flushedLength = 0;

        await middleware.InvokeAsync(context, async httpContext =>
        {
            httpContext.Response.ContentType = "text/event-stream";
            await httpContext.Response.WriteAsync("data: first\r\n\r\n");
            await httpContext.Response.Body.FlushAsync();
            flushedLength = responseBody.Length;
        });

        Assert.IsTrue(flushedLength > 0);
        Assert.IsNotNull(accessLogService.LastResponse);
        Assert.IsNull(accessLogService.LastResponse.Body);
    }

    private sealed class FakeAccessLogService : IAccessLogService
    {
        public ResponseModel? LastResponse { get; private set; }

        public void WriteTraceLog(DateTime startTime, DateTime? endTime, RequestModel request, ResponseModel response, long elapsedMilliseconds, Exception? exception = null)
            => LastResponse = response;

        public void WriteDebugLog(DateTime startTime, DateTime? endTime, RequestModel request, ResponseModel response, long elapsedMilliseconds, Exception? exception = null)
            => LastResponse = response;

        public void WriteInformationLog(DateTime startTime, DateTime? endTime, RequestModel request, ResponseModel response, long elapsedMilliseconds, Exception? exception = null)
            => LastResponse = response;

        public void WriteWarningLog(DateTime startTime, DateTime? endTime, RequestModel request, ResponseModel response, long elapsedMilliseconds, Exception? exception = null)
            => LastResponse = response;

        public void WriteErrorLog(DateTime startTime, DateTime? endTime, RequestModel request, ResponseModel response, long elapsedMilliseconds, Exception? exception = null)
            => LastResponse = response;

        public void WriteCriticalLog(DateTime startTime, DateTime? endTime, RequestModel request, ResponseModel response, long elapsedMilliseconds, Exception? exception = null)
            => LastResponse = response;

        public void WriteAccessLog(DateTime startTime, DateTime? endTime, LogLevel logLevel, RequestModel request, ResponseModel response, long elapsedMilliseconds, Exception? exception = null)
            => LastResponse = response;
    }
}
