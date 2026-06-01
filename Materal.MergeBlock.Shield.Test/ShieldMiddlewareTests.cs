using System.Net;
using Materal.MergeBlock.Shield.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Materal.MergeBlock.Shield.Test;

[TestClass]
public sealed class ShieldMiddlewareTests
{
    [TestMethod]
    public async Task InvokeAsync_ShouldRecordApi404()
    {
        FakeShieldStore store = new();
        ShieldMiddleware middleware = CreateMiddleware(store, new ShieldOptions { NotFoundLimit = 10 });
        DefaultHttpContext context = CreateContext("/api/missing", IPAddress.Parse("127.0.0.1"));

        await middleware.InvokeAsync(context, _ =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        Assert.AreEqual("127.0.0.1", store.RecordedSource);
        Assert.AreEqual("/api/missing", store.RecordedContext.Path);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldBlock_WhenSourceAlreadyBlocked()
    {
        FakeShieldStore store = new()
        {
            BlockState = new ShieldBlockState(true, DateTimeOffset.UtcNow.AddMinutes(10)),
        };
        ShieldMiddleware middleware = CreateMiddleware(store, new ShieldOptions { BlockedStatusCode = StatusCodes.Status401Unauthorized });
        DefaultHttpContext context = CreateContext("/api/blocked", IPAddress.Parse("127.0.0.1"));
        bool nextCalled = false;

        await middleware.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.IsFalse(nextCalled);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldSkipWhiteListIp()
    {
        FakeShieldStore store = new();
        ShieldMiddleware middleware = CreateMiddleware(store, new ShieldOptions { WhiteListIPs = ["127.0.0.1"] });
        DefaultHttpContext context = CreateContext("/api/missing", IPAddress.Parse("127.0.0.1"));

        await middleware.InvokeAsync(context, _ =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        Assert.IsNull(store.RecordedSource);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldSkipOptionsRequest()
    {
        FakeShieldStore store = new();
        ShieldMiddleware middleware = CreateMiddleware(store, new ShieldOptions());
        DefaultHttpContext context = CreateContext("/api/missing", IPAddress.Parse("127.0.0.1"));
        context.Request.Method = HttpMethods.Options;

        await middleware.InvokeAsync(context, _ =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        Assert.IsNull(store.RecordedSource);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldSkipAuthenticatedRequestByDefault()
    {
        FakeShieldStore store = new();
        ShieldMiddleware middleware = CreateMiddleware(store, new ShieldOptions { TrackAuthenticatedRequests = false });
        DefaultHttpContext context = CreateContext("/api/missing", IPAddress.Parse("127.0.0.1"));
        context.User = new(new System.Security.Claims.ClaimsIdentity("Test"));

        await middleware.InvokeAsync(context, _ =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        Assert.IsNull(store.RecordedSource);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldSkipExcludedPath()
    {
        FakeShieldStore store = new();
        ShieldMiddleware middleware = CreateMiddleware(store, new ShieldOptions { ExcludePathPrefixes = ["/swagger"] });
        DefaultHttpContext context = CreateContext("/swagger/missing", IPAddress.Parse("127.0.0.1"));

        await middleware.InvokeAsync(context, _ =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        Assert.IsNull(store.RecordedSource);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldBlockAfterThreshold_WithMemoryStore()
    {
        MemoryShieldStore store = new();
        ShieldMiddleware middleware = CreateMiddleware(store, new ShieldOptions
        {
            MonitorPathPrefixes = ["/api"],
            WindowSeconds = 60,
            NotFoundLimit = 2,
            BlockedSeconds = 600,
            BlockedStatusCode = StatusCodes.Status429TooManyRequests,
        });

        DefaultHttpContext first = CreateContext("/api/missing1", IPAddress.Parse("127.0.0.1"));
        await middleware.InvokeAsync(first, _ =>
        {
            first.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        DefaultHttpContext second = CreateContext("/api/missing2", IPAddress.Parse("127.0.0.1"));
        await middleware.InvokeAsync(second, _ =>
        {
            second.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        DefaultHttpContext third = CreateContext("/api/blocked", IPAddress.Parse("127.0.0.1"));
        bool nextCalled = false;
        await middleware.InvokeAsync(third, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.IsFalse(nextCalled);
        Assert.AreEqual(StatusCodes.Status429TooManyRequests, third.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_ShouldSkipNonMonitorPath()
    {
        FakeShieldStore store = new();
        ShieldMiddleware middleware = CreateMiddleware(store, new ShieldOptions { MonitorPathPrefixes = ["/api"] });
        DefaultHttpContext context = CreateContext("/frontend/missing", IPAddress.Parse("127.0.0.1"));

        await middleware.InvokeAsync(context, _ =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        Assert.IsNull(store.RecordedSource);
    }

    private static ShieldMiddleware CreateMiddleware(IShieldStore store, ShieldOptions options)
        => new(new StaticOptionsMonitor<ShieldOptions>(options), store, NullLogger<ShieldMiddleware>.Instance);

    private static DefaultHttpContext CreateContext(string path, IPAddress ipAddress)
    {
        DefaultHttpContext context = new();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = path;
        context.Connection.RemoteIpAddress = ipAddress;
        return context;
    }

    private sealed class FakeShieldStore : IShieldStore
    {
        public ShieldBlockState BlockState { get; set; }
        public string? RecordedSource { get; private set; }
        public ShieldHitContext RecordedContext { get; private set; }

        public ValueTask<ShieldBlockState> GetBlockStateAsync(string source, DateTimeOffset now, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(BlockState);

        public ValueTask<ShieldHitResult> RecordNotFoundAsync(string source, ShieldHitContext hitContext, ShieldOptions options, CancellationToken cancellationToken = default)
        {
            RecordedSource = source;
            RecordedContext = hitContext;
            return ValueTask.FromResult(new ShieldHitResult(1, false, false, hitContext.Timestamp, hitContext.Timestamp, null));
        }
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;
        public T Get(string? name) => value;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
