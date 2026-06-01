# Materal.MergeBlock.Shield Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增 `Materal.MergeBlock.Shield` 安全防护模块，统计短时间内 API 404 探测行为，并对达到阈值的来源进行单实例临时拦截。

**Architecture:** Shield 作为独立 MergeBlock 模块实现，依赖 `WebModule`，在 `OnPreApplicationInitialization` 注册 `ShieldMiddleware`。中间件只负责请求过滤、来源识别、调用状态存储和日志记录；计数窗口与封禁状态由 `IShieldStore` 以原子语义维护，第一版提供 `MemoryShieldStore`。

**Tech Stack:** C#/.NET `net8.0;net9.0;net10.0`，ASP.NET Core middleware，MergeBlock module lifecycle，`Microsoft.Extensions.Options`，`Microsoft.Extensions.Logging`，MSTest。

---

## Source Documents

- Design: `E:\Project\Materal\Materal\Materal.MergeBlock\docs\plans\001-安全防护插件\design.md`
- Existing middleware pattern: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.AccessLog\AccessLogModule.cs`
- Existing Web module ordering: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Web\WebModule.cs`
- Module ordering implementation: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock\PluginManager.cs`

## Global Constraints

- Keep all new and modified files in CRLF line endings.
- Do not commit automatically. If the user asks for commits, use Chinese commit messages.
- Before editing existing public symbols, run GitNexus impact analysis for the exact symbol being modified.
- Before any commit, run GitNexus change detection and report affected symbols and execution flows.
- First version must not parse `X-Forwarded-For` or `X-Real-IP`; it only consumes `HttpContext.Connection.RemoteIpAddress`.
- Do not modify MMB module ordering rules for this feature.
- Do not add Redis, database audit storage, WAF integration, CAPTCHA, or bot detection in this implementation.

## File Structure

- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\Materal.MergeBlock.Shield.csproj`
  - Packable MergeBlock module project.
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\ConstData.cs`
  - Logger scope constants.
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\ShieldOptions.cs`
  - Configuration model for `MergeBlock:Shield` with defaults.
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\ShieldModule.cs`
  - MergeBlock module registration.
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\ShieldMiddleware.cs`
  - Request filtering, block checks, hit recording, logging.
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\IShieldStore.cs`
  - Store abstraction with atomic hit recording.
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\MemoryShieldStore.cs`
  - In-memory single-instance implementation.
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\Models\ShieldBlockState.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\Models\ShieldHitContext.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\Models\ShieldHitResult.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\MSTestSettings.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\MemoryShieldStoreTests.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\ShieldMiddlewareTests.cs`
- Modify: `E:\Project\Materal\Materal\Materal.slnx`
  - Add Shield module under `/Src/07MergeBlock/Modules/`.
  - Add Shield test project under `/Test/07MergeBlock/`.
- Modify: `E:\Project\Materal\Materal\Materal.Packable.slnx`
  - Add Shield module under `/Src/07MergeBlock/Modules/`.

## Required Pre-Implementation Checks

- [ ] Run `git status --short` in `E:\Project\Materal\Materal`.
- [ ] Run `git -C E:\Project\Materal\Materal\Materal.MergeBlock status --short`.
- [ ] Read `E:\Project\Materal\Materal\Materal.MergeBlock\docs\plans\001-安全防护插件\design.md`.
- [ ] Read `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.AccessLog\AccessLogModule.cs`.
- [ ] Read `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Web\WebModule.cs`.
- [ ] Read `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock\PluginManager.cs`.

## Task 1: Create Projects And Solution Entries

**Files:**
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\Materal.MergeBlock.Shield.csproj`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\MSTestSettings.cs`
- Modify: `E:\Project\Materal\Materal\Materal.slnx`
- Modify: `E:\Project\Materal\Materal\Materal.Packable.slnx`

- [ ] **Step 1: Create the module project file**

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="../../MergeBlockLibrary.props" />
	<PropertyGroup>
		<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
		<Title>Materal MergeBlock包</Title>
		<Description>Materal MergeBlock安全防护模块</Description>
	</PropertyGroup>
	<ItemGroup>
		<FrameworkReference Include="Microsoft.AspNetCore.App" />
	</ItemGroup>
	<ItemGroup>
		<ProjectReference Include="..\Materal.MergeBlock.Web\Materal.MergeBlock.Web.csproj" />
	</ItemGroup>
</Project>
```

- [ ] **Step 2: Create the test project file**

```xml
<Project Sdk="MSTest.Sdk/4.1.0">
	<Import Project="../../Common.props" />
	<PropertyGroup>
		<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
		<IsTestProject>true</IsTestProject>
	</PropertyGroup>
	<ItemGroup>
		<FrameworkReference Include="Microsoft.AspNetCore.App" />
	</ItemGroup>
	<ItemGroup>
		<ProjectReference Include="..\Materal.MergeBlock.Shield\Materal.MergeBlock.Shield.csproj" />
	</ItemGroup>
</Project>
```

- [ ] **Step 3: Create MSTest settings**

```csharp
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
```

- [ ] **Step 4: Add solution entries**

Add this project entry in `Materal.slnx` and `Materal.Packable.slnx` under `/Src/07MergeBlock/Modules/`:

```xml
<Project Path="Materal.MergeBlock/Materal.MergeBlock.Shield/Materal.MergeBlock.Shield.csproj" />
```

Add this project entry in `Materal.slnx` under `/Test/07MergeBlock/`:

```xml
<Project Path="Materal.MergeBlock/Materal.MergeBlock.Shield.Test/Materal.MergeBlock.Shield.Test.csproj" />
```

- [ ] **Step 5: Verify project restore**

Run:

```powershell
dotnet restore .\Materal.MergeBlock\Materal.MergeBlock.Shield\Materal.MergeBlock.Shield.csproj
dotnet restore .\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj
```

Expected: both commands exit with code `0`.

## Task 2: Add Options And Store Contracts

**Files:**
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\ConstData.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\ShieldOptions.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\IShieldStore.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\Models\ShieldBlockState.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\Models\ShieldHitContext.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\Models\ShieldHitResult.cs`

- [ ] **Step 1: Add constants**

```csharp
namespace Materal.MergeBlock.Shield;

/// <summary>
/// 常量数据
/// </summary>
public static class ConstData
{
    /// <summary>
    /// 日志记录器作用域名称
    /// </summary>
    public const string ShieldScopeName = "Shield";
}
```

- [ ] **Step 2: Add options**

```csharp
namespace Materal.MergeBlock.Shield;

/// <summary>
/// Shield配置
/// </summary>
[Options(ConfigKey)]
public class ShieldOptions : IOptions
{
    /// <summary>
    /// 配置节点
    /// </summary>
    public const string ConfigKey = "MergeBlock:Shield";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// 监控路径前缀
    /// </summary>
    public string[] MonitorPathPrefixes { get; set; } = ["/api"];

    /// <summary>
    /// 排除路径前缀
    /// </summary>
    public string[] ExcludePathPrefixes { get; set; } = [];

    /// <summary>
    /// IP白名单
    /// </summary>
    public string[] WhiteListIPs { get; set; } = [];

    /// <summary>
    /// 404统计时间窗口秒数
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// 时间窗口内允许的404次数
    /// </summary>
    public int NotFoundLimit { get; set; } = 10;

    /// <summary>
    /// 封禁秒数
    /// </summary>
    public int BlockedSeconds { get; set; } = 600;

    /// <summary>
    /// 被拦截时返回的HTTP状态码
    /// </summary>
    public int BlockedStatusCode { get; set; } = StatusCodes.Status429TooManyRequests;

    /// <summary>
    /// 是否统计已认证请求
    /// </summary>
    public bool TrackAuthenticatedRequests { get; set; }
}
```

- [ ] **Step 3: Add store models**

```csharp
namespace Materal.MergeBlock.Shield.Models;

/// <summary>
/// Shield封禁状态
/// </summary>
/// <param name="IsBlocked">是否已封禁</param>
/// <param name="BlockedUntil">封禁结束时间</param>
public readonly record struct ShieldBlockState(bool IsBlocked, DateTimeOffset? BlockedUntil);
```

```csharp
namespace Materal.MergeBlock.Shield.Models;

/// <summary>
/// Shield命中上下文
/// </summary>
/// <param name="Path">请求路径</param>
/// <param name="Method">HTTP方法</param>
/// <param name="UserAgent">User-Agent</param>
/// <param name="Timestamp">命中时间</param>
public readonly record struct ShieldHitContext(string Path, string Method, string? UserAgent, DateTimeOffset Timestamp);
```

```csharp
namespace Materal.MergeBlock.Shield.Models;

/// <summary>
/// Shield命中结果
/// </summary>
/// <param name="HitCount">当前窗口命中次数</param>
/// <param name="IsBlocked">是否已封禁</param>
/// <param name="IsNewlyBlocked">是否本次新触发封禁</param>
/// <param name="FirstHitAt">当前窗口首次命中时间</param>
/// <param name="LastHitAt">当前窗口最后命中时间</param>
/// <param name="BlockedUntil">封禁结束时间</param>
public readonly record struct ShieldHitResult(int HitCount, bool IsBlocked, bool IsNewlyBlocked, DateTimeOffset? FirstHitAt, DateTimeOffset? LastHitAt, DateTimeOffset? BlockedUntil);
```

- [ ] **Step 4: Add store interface**

```csharp
using Materal.MergeBlock.Shield.Models;

namespace Materal.MergeBlock.Shield;

/// <summary>
/// Shield状态存储
/// </summary>
public interface IShieldStore
{
    /// <summary>
    /// 获取来源封禁状态
    /// </summary>
    /// <param name="source">来源标识</param>
    /// <param name="now">当前时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>封禁状态</returns>
    ValueTask<ShieldBlockState> GetBlockStateAsync(string source, DateTimeOffset now, CancellationToken cancellationToken = default);

    /// <summary>
    /// 原子记录一次404命中，并在达到阈值时写入封禁状态
    /// </summary>
    /// <param name="source">来源标识</param>
    /// <param name="hitContext">命中上下文</param>
    /// <param name="options">Shield配置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>命中结果</returns>
    ValueTask<ShieldHitResult> RecordNotFoundAsync(string source, ShieldHitContext hitContext, ShieldOptions options, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 5: Build the module project**

Run:

```powershell
dotnet build .\Materal.MergeBlock\Materal.MergeBlock.Shield\Materal.MergeBlock.Shield.csproj
```

Expected: build exits with code `0`.

## Task 3: Implement MemoryShieldStore With Tests

**Files:**
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\MemoryShieldStore.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\MemoryShieldStoreTests.cs`

- [ ] **Step 1: Write failing store tests**

```csharp
using Materal.MergeBlock.Shield.Models;

namespace Materal.MergeBlock.Shield.Test;

[TestClass]
public sealed class MemoryShieldStoreTests
{
    [TestMethod]
    public async Task RecordNotFoundAsync_ShouldBlock_WhenLimitReached()
    {
        MemoryShieldStore store = new();
        ShieldOptions options = new()
        {
            WindowSeconds = 60,
            NotFoundLimit = 2,
            BlockedSeconds = 600,
        };
        DateTimeOffset now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        ShieldHitResult first = await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/missing1", "GET", null, now), options);
        ShieldHitResult second = await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/missing2", "GET", null, now.AddSeconds(1)), options);

        Assert.AreEqual(1, first.HitCount);
        Assert.IsFalse(first.IsBlocked);
        Assert.AreEqual(2, second.HitCount);
        Assert.IsTrue(second.IsBlocked);
        Assert.IsTrue(second.IsNewlyBlocked);
        Assert.AreEqual(now.AddSeconds(601), second.BlockedUntil);
    }

    [TestMethod]
    public async Task RecordNotFoundAsync_ShouldIgnoreExpiredHits()
    {
        MemoryShieldStore store = new();
        ShieldOptions options = new()
        {
            WindowSeconds = 10,
            NotFoundLimit = 2,
            BlockedSeconds = 600,
        };
        DateTimeOffset now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/old", "GET", null, now), options);
        ShieldHitResult result = await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/new", "GET", null, now.AddSeconds(11)), options);

        Assert.AreEqual(1, result.HitCount);
        Assert.IsFalse(result.IsBlocked);
    }

    [TestMethod]
    public async Task GetBlockStateAsync_ShouldClearExpiredBlock()
    {
        MemoryShieldStore store = new();
        ShieldOptions options = new()
        {
            WindowSeconds = 60,
            NotFoundLimit = 1,
            BlockedSeconds = 5,
        };
        DateTimeOffset now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/missing", "GET", null, now), options);
        ShieldBlockState blocked = await store.GetBlockStateAsync("127.0.0.1", now.AddSeconds(1));
        ShieldBlockState expired = await store.GetBlockStateAsync("127.0.0.1", now.AddSeconds(6));

        Assert.IsTrue(blocked.IsBlocked);
        Assert.IsFalse(expired.IsBlocked);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj --filter MemoryShieldStoreTests
```

Expected: fail because `MemoryShieldStore` does not exist.

- [ ] **Step 3: Implement MemoryShieldStore**

```csharp
using Materal.MergeBlock.Shield.Models;

namespace Materal.MergeBlock.Shield;

/// <summary>
/// 内存Shield状态存储
/// </summary>
public class MemoryShieldStore : IShieldStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, SourceState> _states = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public ValueTask<ShieldBlockState> GetBlockStateAsync(string source, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            if (!_states.TryGetValue(source, out SourceState? state)) return ValueTask.FromResult(new ShieldBlockState(false, null));
            if (state.BlockedUntil is null) return ValueTask.FromResult(new ShieldBlockState(false, null));
            if (state.BlockedUntil > now) return ValueTask.FromResult(new ShieldBlockState(true, state.BlockedUntil));
            state.BlockedUntil = null;
            return ValueTask.FromResult(new ShieldBlockState(false, null));
        }
    }

    /// <inheritdoc/>
    public ValueTask<ShieldHitResult> RecordNotFoundAsync(string source, ShieldHitContext hitContext, ShieldOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            SourceState state = GetOrCreateState(source);
            DateTimeOffset now = hitContext.Timestamp;
            if (state.BlockedUntil is not null && state.BlockedUntil > now)
            {
                return ValueTask.FromResult(CreateResult(state, false));
            }
            if (state.BlockedUntil is not null && state.BlockedUntil <= now)
            {
                state.BlockedUntil = null;
            }

            int windowSeconds = Math.Max(1, options.WindowSeconds);
            DateTimeOffset minHitTime = now.AddSeconds(-windowSeconds);
            state.Hits.RemoveAll(m => m < minHitTime);
            state.Hits.Add(now);

            bool newlyBlocked = false;
            int notFoundLimit = Math.Max(1, options.NotFoundLimit);
            if (state.Hits.Count >= notFoundLimit)
            {
                int blockedSeconds = Math.Max(1, options.BlockedSeconds);
                state.BlockedUntil = now.AddSeconds(blockedSeconds);
                newlyBlocked = true;
            }
            return ValueTask.FromResult(CreateResult(state, newlyBlocked));
        }
    }

    private SourceState GetOrCreateState(string source)
    {
        if (_states.TryGetValue(source, out SourceState? state)) return state;
        state = new SourceState();
        _states[source] = state;
        return state;
    }

    private static ShieldHitResult CreateResult(SourceState state, bool newlyBlocked)
    {
        DateTimeOffset? firstHit = state.Hits.Count > 0 ? state.Hits[0] : null;
        DateTimeOffset? lastHit = state.Hits.Count > 0 ? state.Hits[^1] : null;
        return new ShieldHitResult(state.Hits.Count, state.BlockedUntil is not null, newlyBlocked, firstHit, lastHit, state.BlockedUntil);
    }

    private sealed class SourceState
    {
        public List<DateTimeOffset> Hits { get; } = [];
        public DateTimeOffset? BlockedUntil { get; set; }
    }
}
```

- [ ] **Step 4: Run store tests**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj --filter MemoryShieldStoreTests
```

Expected: all `MemoryShieldStoreTests` pass.

## Task 4: Implement ShieldMiddleware With Tests

**Files:**
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\ShieldMiddleware.cs`
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\ShieldMiddlewareTests.cs`

- [ ] **Step 1: Write failing middleware tests**

```csharp
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

    private static ShieldMiddleware CreateMiddleware(FakeShieldStore store, ShieldOptions options)
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
```

- [ ] **Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj --filter ShieldMiddlewareTests
```

Expected: fail because `ShieldMiddleware` does not exist.

- [ ] **Step 3: Implement ShieldMiddleware**

```csharp
using Materal.MergeBlock.Shield.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Materal.MergeBlock.Shield;

/// <summary>
/// Shield中间件
/// </summary>
/// <param name="optionsMonitor">Shield配置</param>
/// <param name="store">Shield状态存储</param>
/// <param name="logger">日志记录器</param>
public class ShieldMiddleware(IOptionsMonitor<ShieldOptions> optionsMonitor, IShieldStore store, ILogger<ShieldMiddleware> logger) : IMiddleware
{
    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ShieldOptions options = optionsMonitor.CurrentValue;
        if (!options.Enable || !TryGetSource(context, out string? source) || ShouldSkip(context, options))
        {
            await next(context);
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        try
        {
            ShieldBlockState blockState = await store.GetBlockStateAsync(source, now, context.RequestAborted);
            if (blockState.IsBlocked)
            {
                context.Response.StatusCode = NormalizeStatusCode(options.BlockedStatusCode);
                logger.LogWarning("Shield blocked request. Source: {Source}, Path: {Path}, BlockedUntil: {BlockedUntil}", source, context.Request.Path.Value, blockState.BlockedUntil);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Shield failed to read block state. Source: {Source}, Path: {Path}", source, context.Request.Path.Value);
            await next(context);
            return;
        }

        await next(context);
        if (context.Response.StatusCode != StatusCodes.Status404NotFound) return;

        try
        {
            ShieldHitContext hitContext = new(context.Request.Path.Value ?? string.Empty, context.Request.Method, context.Request.Headers.UserAgent.ToString(), DateTimeOffset.UtcNow);
            ShieldHitResult result = await store.RecordNotFoundAsync(source, hitContext, options, context.RequestAborted);
            logger.LogInformation("Shield recorded 404. Source: {Source}, Path: {Path}, HitCount: {HitCount}", source, hitContext.Path, result.HitCount);
            if (result.IsNewlyBlocked)
            {
                logger.LogWarning("Shield blocked source. Source: {Source}, HitCount: {HitCount}, BlockedUntil: {BlockedUntil}, FirstHitAt: {FirstHitAt}, LastHitAt: {LastHitAt}", source, result.HitCount, result.BlockedUntil, result.FirstHitAt, result.LastHitAt);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Shield failed to record 404. Source: {Source}, Path: {Path}", source, context.Request.Path.Value);
        }
    }

    private static bool TryGetSource(HttpContext context, out string? source)
    {
        source = context.Connection.RemoteIpAddress?.ToString();
        return !string.IsNullOrWhiteSpace(source);
    }

    private static bool ShouldSkip(HttpContext context, ShieldOptions options)
    {
        if (HttpMethods.IsOptions(context.Request.Method)) return true;
        if (!options.TrackAuthenticatedRequests && context.User.Identity?.IsAuthenticated == true) return true;
        string path = context.Request.Path.Value ?? string.Empty;
        if (IsWhiteListed(context, options)) return true;
        if (MatchesAnyPrefix(path, options.ExcludePathPrefixes)) return true;
        return !MatchesAnyPrefix(path, options.MonitorPathPrefixes);
    }

    private static bool IsWhiteListed(HttpContext context, ShieldOptions options)
    {
        string? ip = context.Connection.RemoteIpAddress?.ToString();
        return !string.IsNullOrWhiteSpace(ip) && options.WhiteListIPs.Any(m => string.Equals(m, ip, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesAnyPrefix(string path, IEnumerable<string> prefixes)
        => prefixes.Where(m => !string.IsNullOrWhiteSpace(m)).Select(NormalizePrefix).Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static string NormalizePrefix(string prefix)
    {
        prefix = prefix.Trim();
        return prefix.StartsWith('/') ? prefix : $"/{prefix}";
    }

    private static int NormalizeStatusCode(int statusCode)
        => statusCode is >= 100 and <= 599 ? statusCode : StatusCodes.Status429TooManyRequests;
}
```

- [ ] **Step 4: Run middleware tests**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj --filter ShieldMiddlewareTests
```

Expected: all `ShieldMiddlewareTests` pass.

## Task 5: Implement ShieldModule Registration

**Files:**
- Create: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield\ShieldModule.cs`

- [ ] **Step 1: Run GitNexus impact analysis before referencing existing lifecycle symbols**

Run GitNexus impact analysis for `WebModule` because `ShieldModule` depends on its pipeline order:

```text
gitnexus_impact({ repo: "Materal", target: "WebModule", direction: "upstream" })
```

Expected: review output confirms this plan only relies on `WebModule.OnPreApplicationInitialization` and `WebModule.OnApplicationInitialization` ordering; if risk is HIGH or CRITICAL, stop and report to the user before editing.

- [ ] **Step 2: Implement module registration**

```csharp
using Materal.MergeBlock.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
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
```

- [ ] **Step 3: Build Shield module**

Run:

```powershell
dotnet build .\Materal.MergeBlock\Materal.MergeBlock.Shield\Materal.MergeBlock.Shield.csproj
```

Expected: build exits with code `0`.

## Task 6: Add Integration-Focused Middleware Coverage

**Files:**
- Modify: `E:\Project\Materal\Materal\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\ShieldMiddlewareTests.cs`

- [ ] **Step 1: Add threshold-to-block middleware test with real store**

Add this test to `ShieldMiddlewareTests`:

```csharp
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
```

- [ ] **Step 2: Add non-monitor path test**

Add this test to `ShieldMiddlewareTests`:

```csharp
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
```

- [ ] **Step 3: Run all Shield tests**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj
```

Expected: all Shield tests pass for `net8.0`, `net9.0`, and `net10.0`.

## Task 7: Verification And Packaging Checks

**Files:**
- No source edits unless verification reveals compile errors in files created by earlier tasks.

- [ ] **Step 1: Build Shield module directly**

Run:

```powershell
dotnet build .\Materal.MergeBlock\Materal.MergeBlock.Shield\Materal.MergeBlock.Shield.csproj
```

Expected: build exits with code `0`.

- [ ] **Step 2: Test Shield module directly**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj
```

Expected: test exits with code `0`.

- [ ] **Step 3: Build main solution**

Run:

```powershell
dotnet build .\Materal.slnx
```

Expected: build exits with code `0`.

- [ ] **Step 4: Build packable solution**

Run:

```powershell
dotnet build .\Materal.Packable.slnx
```

Expected: build exits with code `0`.

- [ ] **Step 5: Run GitNexus change detection before any commit**

Run:

```text
gitnexus_detect_changes({ repo: "Materal", scope: "all" })
```

Expected: changed symbols are limited to the new Shield module and solution/project metadata. Report any unexpected affected execution flows to the user before staging.

- [ ] **Step 6: Confirm line endings**

Run:

```powershell
$files = @(
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\Materal.MergeBlock.Shield.csproj',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\ConstData.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\ShieldOptions.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\ShieldModule.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\ShieldMiddleware.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\IShieldStore.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\MemoryShieldStore.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\Models\ShieldBlockState.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\Models\ShieldHitContext.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield\Models\ShieldHitResult.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\Materal.MergeBlock.Shield.Test.csproj',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\MSTestSettings.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\MemoryShieldStoreTests.cs',
  '.\Materal.MergeBlock\Materal.MergeBlock.Shield.Test\ShieldMiddlewareTests.cs'
)
foreach ($file in $files) {
  $text = [System.IO.File]::ReadAllText($file)
  $lfOnly = [regex]::Matches($text, "(?<!`r)`n").Count
  "$file LFOnly=$lfOnly"
}
```

Expected: every listed file prints `LFOnly=0`.

## User-Confirmed Commit Checkpoints

Only run these commands if the user explicitly asks for commits.

```powershell
git add .\Materal.MergeBlock\Materal.MergeBlock.Shield .\Materal.MergeBlock\Materal.MergeBlock.Shield.Test .\Materal.slnx .\Materal.Packable.slnx
git commit -m "feat: 添加安全防护模块"
```

## Completion Criteria

- `Materal.MergeBlock.Shield` builds for `net8.0`, `net9.0`, and `net10.0`.
- Shield module is included in `Materal.slnx` and `Materal.Packable.slnx`.
- Shield test project is included in `Materal.slnx`.
- API 404 hits under monitored paths are recorded by source IP.
- Source IPs are taken only from `HttpContext.Connection.RemoteIpAddress`.
- Whitelist, excluded paths, non-monitored paths, `OPTIONS`, and authenticated requests are skipped according to options.
- A source reaching `NotFoundLimit` inside `WindowSeconds` is temporarily blocked for `BlockedSeconds`.
- Blocked requests return `BlockedStatusCode`.
- Store failures are logged and requests are allowed through.
- GitNexus change detection has been run before any commit.

## Self-Review

- Spec coverage: this plan maps design goals to module project creation, options, middleware behavior, memory store behavior, module registration, tests, and verification.
- Placeholder scan: no implementation step relies on unresolved placeholders.
- Type consistency: `ShieldOptions`, `IShieldStore`, `ShieldHitContext`, `ShieldHitResult`, `ShieldBlockState`, `MemoryShieldStore`, `ShieldMiddleware`, and `ShieldModule` use consistent names and signatures across tasks.
