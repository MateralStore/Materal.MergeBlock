# 热插拔一期 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 为 MergeBlock 增加第一期热插拔基础能力：插件 manifest、插件状态、运行时加载骨架、插件级容器、动态 Endpoint、插件任务和管理入口。

**Architecture:** 主容器保持稳定，只注册热插拔运行时、状态注册表和 Web 代理；运行时插件使用独立 `PluginLoadContext` 和插件级 `ServiceProvider`。一期优先支持手动加载、启动、停止和动态 Endpoint，不承诺所有现有 MVC Controller 插件都能完整热卸载。

**Tech Stack:** C#、.NET 8/9/10 multi-target、MSTest、ASP.NET Core Endpoint Routing、`AssemblyLoadContext`、Microsoft.Extensions.DependencyInjection。

---

## 执行约束

- 本计划对应 [design.md](E:/Project/Materal/Materal/Materal.MergeBlock/docs/plans/003-热插拔/design.md) 的一期范围。
- 实现前必须按仓库规则对被修改符号运行 GitNexus upstream impact analysis。
- 提交前必须运行 GitNexus change detection。
- 本仓库规则要求不要主动提交代码，所以任务末尾只做 `git status --short` 检查点。用户明确要求提交时，提交信息必须使用中文。
- 所有新增或修改文件保持 CRLF 行尾。
- 不修改无关项目，不批量格式化。

## 文件结构

### 新增抽象文件

- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginState.cs`：插件状态枚举。
- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginManifest.cs`：插件 manifest 模型和校验。
- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginDependencyManifest.cs`：插件依赖和版本范围匹配。
- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginRuntimeInfo.cs`：对外暴露的插件运行状态快照。
- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginOperationResult.cs`：加载、启动、停止、卸载操作结果。
- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginLifecycleContext.cs`：生命周期上下文。
- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/IHotPlugModule.cs`：可选热插拔生命周期接口。
- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/IPluginRuntime.cs`：插件运行时入口。
- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/IPluginBackgroundTask.cs`：插件级后台任务抽象。
- `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginTaskContext.cs`：插件任务上下文。

### 新增核心实现文件

- `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginManifestReader.cs`：从插件目录读取 `plugin.json`。
- `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginDependencyGraph.cs`：插件依赖排序和循环依赖检测。
- `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginStateMachine.cs`：插件状态流转校验。
- `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginInstance.cs`：运行时插件实例，持有 ALC、程序集、模块、服务容器和任务。
- `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginRuntime.cs`：`IPluginRuntime` 默认实现。
- `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginServiceProviderFactory.cs`：创建插件级服务容器。
- `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginUnloadVerifier.cs`：ALC 卸载验证。
- `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginRuntimeOptions.cs`：`MergeBlock:HotPlug` 配置。
- `Materal.MergeBlock/Materal.MergeBlock/HotPlug/ServiceCollectionExtensions.cs`：注册热插拔运行时。

### 新增 Web 文件

- `Materal.MergeBlock/Materal.MergeBlock.Web.Abstractions/HotPlug/IPluginEndpointContributor.cs`：插件 Endpoint 贡献器。
- `Materal.MergeBlock/Materal.MergeBlock.Web.Abstractions/HotPlug/PluginEndpointContributionContext.cs`：Endpoint 注册上下文。
- `Materal.MergeBlock/Materal.MergeBlock.Web/HotPlug/PluginEndpointDataSource.cs`：动态 Endpoint 数据源。
- `Materal.MergeBlock/Materal.MergeBlock.Web/HotPlug/PluginRouteManager.cs`：Endpoint 添加、移除和变更通知。
- `Materal.MergeBlock/Materal.MergeBlock.Web/HotPlug/PluginManagementEndpointContributor.cs`：最小管理 API。

### 修改现有文件

- `Materal.MergeBlock/Materal.MergeBlock/GlobalUsing.cs`：仅在测试需要内部类型时增加 `InternalsVisibleTo("Materal.MergeBlock.Test")`。
- `Materal.MergeBlock/Materal.MergeBlock/Extensions/DIExtensions.cs`：启动期注册热插拔运行时。
- `Materal.MergeBlock/Materal.MergeBlock.Web/WebModule.cs`：把动态 Endpoint DataSource 接入 ASP.NET Core 路由。
- `Materal.MergeBlock/Materal.MergeBlock.Test/Materal.MergeBlock.Test.csproj`：增加测试所需项目引用。

### 新增测试文件

- `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginManifestTest.cs`
- `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginDependencyGraphTest.cs`
- `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginStateMachineTest.cs`
- `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginRuntimeTest.cs`
- `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginEndpointDataSourceTest.cs`

## Task 0: 前置影响分析和基线检查

**Files:**

- Read: `Materal.MergeBlock/Materal.MergeBlock/PluginManager.cs`
- Read: `Materal.MergeBlock/Materal.MergeBlock/Plugin.cs`
- Read: `Materal.MergeBlock/Materal.MergeBlock/PluginLoadContext.cs`
- Read: `Materal.MergeBlock/Materal.MergeBlock/Extensions/DIExtensions.cs`
- Read: `Materal.MergeBlock/Materal.MergeBlock.Web/WebModule.cs`

- [ ] **Step 1: 检查工作区状态**

Run:

```powershell
git status --short
git -C .\Materal.MergeBlock status --short
```

Expected: 记录已有用户改动，不回滚、不清理无关文件。

- [ ] **Step 2: 运行 GitNexus 影响分析**

Use GitNexus MCP:

```text
impact(repo: "Materal", target: "PluginManager", direction: "upstream")
impact(repo: "Materal", target: "Plugin", direction: "upstream")
impact(repo: "Materal", target: "PluginLoadContext", direction: "upstream")
impact(repo: "Materal", target: "DIExtensions", file_path: "Materal.MergeBlock/Materal.MergeBlock/Extensions/DIExtensions.cs", direction: "upstream")
impact(repo: "Materal", target: "WebModule", direction: "upstream")
```

Expected: 记录 direct callers、affected processes 和 risk。若任一结果为 HIGH 或 CRITICAL，先把风险说明给用户，再继续。

- [ ] **Step 3: 运行最小基线构建**

Run:

```powershell
dotnet build .\Materal.MergeBlock\Materal.MergeBlock\Materal.MergeBlock.csproj --framework net8.0
dotnet build .\Materal.MergeBlock\Materal.MergeBlock.Web\Materal.MergeBlock.Web.csproj --framework net8.0
```

Expected: 两个项目构建通过。若失败，先确认是否由已有工作区改动导致。

## Task 1: 新增热插拔公共抽象

**Files:**

- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginState.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginManifest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginDependencyManifest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginRuntimeInfo.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginOperationResult.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginLifecycleContext.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/IHotPlugModule.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/IPluginRuntime.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/IPluginBackgroundTask.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Abstractions/HotPlug/PluginTaskContext.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.Test/Materal.MergeBlock.Test.csproj`
- Test: `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginManifestTest.cs`

- [ ] **Step 1: 增加测试项目引用**

Modify `Materal.MergeBlock/Materal.MergeBlock.Test/Materal.MergeBlock.Test.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\Materal.MergeBlock.Abstractions\Materal.MergeBlock.Abstractions.csproj" />
  <ProjectReference Include="..\Materal.MergeBlock.Web.Abstractions\Materal.MergeBlock.Web.Abstractions.csproj" />
</ItemGroup>
```

Keep the existing `Materal.MergeBlock.GeneratorCode` reference.

- [ ] **Step 2: 写 manifest 失败测试**

Create `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginManifestTest.cs`:

```csharp
using Materal.MergeBlock.Abstractions.HotPlug;

namespace Materal.MergeBlock.Test.HotPlug;

[TestClass]
public sealed class PluginManifestTest
{
    [TestMethod]
    public void Validate_ShouldThrow_WhenNameIsEmpty()
    {
        PluginManifest manifest = new()
        {
            Version = "1.0.0",
            EntryAssembly = "DemoPlugin.Application"
        };

        Assert.ThrowsExactly<MergeBlockException>(manifest.Validate);
    }

    [TestMethod]
    public void IsVersionSatisfied_ShouldSupportInclusiveLowerAndExclusiveUpperRange()
    {
        PluginDependencyManifest dependency = new()
        {
            Name = "CorePlugin",
            VersionRange = "[1.0.0,2.0.0)"
        };

        Assert.IsTrue(dependency.IsVersionSatisfied("1.0.0"));
        Assert.IsTrue(dependency.IsVersionSatisfied("1.5.0"));
        Assert.IsFalse(dependency.IsVersionSatisfied("2.0.0"));
    }
}
```

- [ ] **Step 3: 运行测试确认失败**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginManifestTest
```

Expected: 编译失败，提示 `Materal.MergeBlock.Abstractions.HotPlug` 或相关类型不存在。

- [ ] **Step 4: 新增抽象模型**

Create `PluginState.cs`:

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

/// <summary>
/// 插件状态
/// </summary>
public enum PluginState
{
    Discovered = 0,
    Loading = 10,
    Loaded = 20,
    Starting = 30,
    Running = 40,
    Stopping = 50,
    Stopped = 60,
    Unloading = 70,
    Unloaded = 80,
    LoadingFailed = 110,
    StartingFailed = 120,
    StoppingFailed = 130,
    UnloadFailed = 140
}
```

Create `PluginDependencyManifest.cs`:

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

/// <summary>
/// 插件依赖声明
/// </summary>
public class PluginDependencyManifest
{
    /// <summary>
    /// 插件名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 版本范围，支持精确版本或 [1.0.0,2.0.0) 格式
    /// </summary>
    public string VersionRange { get; set; } = string.Empty;
    /// <summary>
    /// 判断版本是否满足依赖范围
    /// </summary>
    public bool IsVersionSatisfied(string version)
    {
        if (string.IsNullOrWhiteSpace(VersionRange)) return true;
        if (!VersionRange.StartsWith('[') && !VersionRange.StartsWith('('))
        {
            return string.Equals(VersionRange, version, StringComparison.OrdinalIgnoreCase);
        }
        if (!Version.TryParse(version, out Version? currentVersion)) return false;
        bool includeMin = VersionRange.StartsWith('[');
        bool includeMax = VersionRange.EndsWith(']');
        string range = VersionRange.Trim('[', ']', '(', ')');
        string[] parts = range.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 2) return false;
        if (!string.IsNullOrWhiteSpace(parts[0]))
        {
            if (!Version.TryParse(parts[0], out Version? minVersion)) return false;
            int compareMin = currentVersion.CompareTo(minVersion);
            if (compareMin < 0 || compareMin == 0 && !includeMin) return false;
        }
        if (!string.IsNullOrWhiteSpace(parts[1]))
        {
            if (!Version.TryParse(parts[1], out Version? maxVersion)) return false;
            int compareMax = currentVersion.CompareTo(maxVersion);
            if (compareMax > 0 || compareMax == 0 && !includeMax) return false;
        }
        return true;
    }
}
```

Create `PluginManifest.cs`:

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

/// <summary>
/// 插件清单
/// </summary>
public class PluginManifest
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string EntryAssembly { get; set; } = string.Empty;
    public string? StartModule { get; set; }
    public List<string> PluginType { get; set; } = [];
    public List<PluginDependencyManifest> Dependencies { get; set; } = [];
    public bool IsCollectible { get; set; } = true;
    public bool AllowRuntimeLoad { get; set; } = true;
    public bool AllowRuntimeUnload { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new MergeBlockException("插件名称不能为空");
        if (string.IsNullOrWhiteSpace(Version)) throw new MergeBlockException($"插件[{Name}]版本不能为空");
        if (!Version.TryParse(Version, out _)) throw new MergeBlockException($"插件[{Name}]版本格式无效");
        if (string.IsNullOrWhiteSpace(EntryAssembly)) throw new MergeBlockException($"插件[{Name}]入口程序集不能为空");
    }
}
```

Create the remaining abstraction files:

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

public sealed class PluginRuntimeInfo
{
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string RootPath { get; init; } = string.Empty;
    public PluginState State { get; init; }
    public string? FailureMessage { get; init; }
    public IReadOnlyList<string> Dependencies { get; init; } = [];
}
```

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

public sealed class PluginOperationResult
{
    public bool Success { get; init; }
    public PluginRuntimeInfo? Plugin { get; init; }
    public string? Message { get; init; }

    public static PluginOperationResult Succeed(PluginRuntimeInfo plugin) => new()
    {
        Success = true,
        Plugin = plugin
    };

    public static PluginOperationResult Fail(string message, PluginRuntimeInfo? plugin = null) => new()
    {
        Success = false,
        Message = message,
        Plugin = plugin
    };
}
```

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

public sealed class PluginLifecycleContext(IServiceProvider hostServiceProvider, IServiceProvider pluginServiceProvider, PluginManifest manifest)
{
    public IServiceProvider HostServiceProvider { get; } = hostServiceProvider;
    public IServiceProvider PluginServiceProvider { get; } = pluginServiceProvider;
    public PluginManifest Manifest { get; } = manifest;
}
```

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

public interface IHotPlugModule
{
    Task OnPluginLoadingAsync(PluginLifecycleContext context) => Task.CompletedTask;
    Task OnPluginLoadedAsync(PluginLifecycleContext context) => Task.CompletedTask;
    Task OnPluginStartingAsync(PluginLifecycleContext context) => Task.CompletedTask;
    Task OnPluginStartedAsync(PluginLifecycleContext context) => Task.CompletedTask;
    Task OnPluginStoppingAsync(PluginLifecycleContext context) => Task.CompletedTask;
    Task OnPluginStoppedAsync(PluginLifecycleContext context) => Task.CompletedTask;
    Task OnPluginUnloadingAsync(PluginLifecycleContext context) => Task.CompletedTask;
    Task OnPluginUnloadedAsync(PluginLifecycleContext context) => Task.CompletedTask;
}
```

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

public interface IPluginRuntime
{
    Task<IReadOnlyList<PluginRuntimeInfo>> GetPluginsAsync(CancellationToken cancellationToken = default);
    Task<PluginOperationResult> LoadAsync(string pluginPath, CancellationToken cancellationToken = default);
    Task<PluginOperationResult> StartAsync(string pluginName, CancellationToken cancellationToken = default);
    Task<PluginOperationResult> StopAsync(string pluginName, CancellationToken cancellationToken = default);
    Task<PluginOperationResult> UnloadAsync(string pluginName, CancellationToken cancellationToken = default);
}
```

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

public interface IPluginBackgroundTask
{
    Task StartAsync(PluginTaskContext context, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
```

```csharp
namespace Materal.MergeBlock.Abstractions.HotPlug;

public sealed class PluginTaskContext(IServiceProvider serviceProvider, PluginManifest manifest)
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public PluginManifest Manifest { get; } = manifest;
}
```

- [ ] **Step 5: 运行测试确认通过**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginManifestTest
```

Expected: `PluginManifestTest` 通过。

- [ ] **Step 6: 检查点**

Run:

```powershell
git status --short
```

Expected: 只出现本任务相关文件和已有用户改动。不提交。

## Task 2: 实现 manifest 读取和依赖排序

**Files:**

- Create: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginManifestReader.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginDependencyGraph.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock/GlobalUsing.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginDependencyGraphTest.cs`

- [ ] **Step 1: GitNexus 影响分析**

Use GitNexus MCP:

```text
impact(repo: "Materal", target: "Plugin", direction: "upstream")
impact(repo: "Materal", target: "PluginManager", direction: "upstream")
```

Expected: 记录风险。本任务新增旁路实现，不修改 `Plugin` 和 `PluginManager` 行为。

- [ ] **Step 2: 开放核心内部类型给测试项目**

Modify `Materal.MergeBlock/Materal.MergeBlock/GlobalUsing.cs`:

```csharp
global using Materal.Extensions.DependencyInjection;
global using Materal.MergeBlock.Abstractions;
global using Materal.MergeBlock.Abstractions.HotPlug;
global using Materal.MergeBlock.HotPlug;
global using Materal.Utils.Extensions;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using System.Reflection;
global using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Materal.MergeBlock.Test")]
```

- [ ] **Step 3: 写依赖排序失败测试**

Create `PluginDependencyGraphTest.cs`:

```csharp
using Materal.MergeBlock.Abstractions.HotPlug;
using Materal.MergeBlock.HotPlug;

namespace Materal.MergeBlock.Test.HotPlug;

[TestClass]
public sealed class PluginDependencyGraphTest
{
    [TestMethod]
    public void Sort_ShouldPutDependencyBeforeDependent()
    {
        PluginManifest core = new()
        {
            Name = "CorePlugin",
            Version = "1.0.0",
            EntryAssembly = "CorePlugin.Application"
        };
        PluginManifest demo = new()
        {
            Name = "DemoPlugin",
            Version = "1.0.0",
            EntryAssembly = "DemoPlugin.Application",
            Dependencies = [new PluginDependencyManifest { Name = "CorePlugin", VersionRange = "[1.0.0,2.0.0)" }]
        };

        IReadOnlyList<PluginManifest> result = PluginDependencyGraph.Sort([demo, core]);

        Assert.AreEqual("CorePlugin", result[0].Name);
        Assert.AreEqual("DemoPlugin", result[1].Name);
    }

    [TestMethod]
    public void Sort_ShouldThrow_WhenDependencyMissing()
    {
        PluginManifest demo = new()
        {
            Name = "DemoPlugin",
            Version = "1.0.0",
            EntryAssembly = "DemoPlugin.Application",
            Dependencies = [new PluginDependencyManifest { Name = "CorePlugin" }]
        };

        Assert.ThrowsExactly<MergeBlockException>(() => PluginDependencyGraph.Sort([demo]));
    }
}
```

- [ ] **Step 4: 运行测试确认失败**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginDependencyGraphTest
```

Expected: 编译失败，提示 `PluginDependencyGraph` 不存在。

- [ ] **Step 5: 实现 manifest reader**

Create `PluginManifestReader.cs`:

```csharp
using System.Text.Json;

namespace Materal.MergeBlock.HotPlug;

internal sealed class PluginManifestReader
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public PluginManifest Read(string pluginPath)
    {
        if (string.IsNullOrWhiteSpace(pluginPath)) throw new MergeBlockException("插件路径不能为空");
        string manifestPath = Path.Combine(pluginPath, "plugin.json");
        if (!File.Exists(manifestPath)) throw new MergeBlockException($"插件清单文件不存在：{manifestPath}");
        string json = File.ReadAllText(manifestPath);
        PluginManifest manifest = JsonSerializer.Deserialize<PluginManifest>(json, JsonSerializerOptions)
            ?? throw new MergeBlockException($"插件清单文件格式无效：{manifestPath}");
        manifest.Validate();
        return manifest;
    }
}
```

- [ ] **Step 6: 实现依赖排序**

Create `PluginDependencyGraph.cs`:

```csharp
namespace Materal.MergeBlock.HotPlug;

internal static class PluginDependencyGraph
{
    public static IReadOnlyList<PluginManifest> Sort(IReadOnlyCollection<PluginManifest> manifests)
    {
        Dictionary<string, PluginManifest> manifestMap = manifests.ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
        List<PluginManifest> result = [];
        HashSet<string> visiting = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);
        foreach (PluginManifest manifest in manifests)
        {
            Visit(manifest, manifestMap, visiting, visited, result);
        }
        return result;
    }

    private static void Visit(PluginManifest manifest, Dictionary<string, PluginManifest> manifestMap, HashSet<string> visiting, HashSet<string> visited, List<PluginManifest> result)
    {
        if (visited.Contains(manifest.Name)) return;
        if (!visiting.Add(manifest.Name)) throw new MergeBlockException($"插件依赖存在循环：{manifest.Name}");
        foreach (PluginDependencyManifest dependency in manifest.Dependencies)
        {
            if (!manifestMap.TryGetValue(dependency.Name, out PluginManifest? dependencyManifest))
            {
                throw new MergeBlockException($"插件[{manifest.Name}]依赖的插件[{dependency.Name}]不存在");
            }
            if (!dependency.IsVersionSatisfied(dependencyManifest.Version))
            {
                throw new MergeBlockException($"插件[{manifest.Name}]依赖插件[{dependency.Name}]版本不满足要求");
            }
            Visit(dependencyManifest, manifestMap, visiting, visited, result);
        }
        visiting.Remove(manifest.Name);
        visited.Add(manifest.Name);
        result.Add(manifest);
    }
}
```

- [ ] **Step 7: 运行测试确认通过**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginDependencyGraphTest
```

Expected: `PluginDependencyGraphTest` 通过。

## Task 3: 实现插件状态机

**Files:**

- Create: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginStateMachine.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginStateMachineTest.cs`

- [ ] **Step 1: 写失败测试**

Create `PluginStateMachineTest.cs`:

```csharp
using Materal.MergeBlock.Abstractions.HotPlug;
using Materal.MergeBlock.HotPlug;

namespace Materal.MergeBlock.Test.HotPlug;

[TestClass]
public sealed class PluginStateMachineTest
{
    [TestMethod]
    public void MoveTo_ShouldAllowNormalLoadStartStopFlow()
    {
        PluginState state = PluginState.Discovered;

        state = PluginStateMachine.MoveTo("DemoPlugin", state, PluginState.Loading);
        state = PluginStateMachine.MoveTo("DemoPlugin", state, PluginState.Loaded);
        state = PluginStateMachine.MoveTo("DemoPlugin", state, PluginState.Starting);
        state = PluginStateMachine.MoveTo("DemoPlugin", state, PluginState.Running);
        state = PluginStateMachine.MoveTo("DemoPlugin", state, PluginState.Stopping);
        state = PluginStateMachine.MoveTo("DemoPlugin", state, PluginState.Stopped);

        Assert.AreEqual(PluginState.Stopped, state);
    }

    [TestMethod]
    public void MoveTo_ShouldThrow_WhenStartFromDiscovered()
    {
        Assert.ThrowsExactly<MergeBlockException>(() =>
            PluginStateMachine.MoveTo("DemoPlugin", PluginState.Discovered, PluginState.Starting));
    }
}
```

- [ ] **Step 2: 运行测试确认失败**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginStateMachineTest
```

Expected: 编译失败，提示 `PluginStateMachine` 不存在。

- [ ] **Step 3: 实现状态机**

Create `PluginStateMachine.cs`:

```csharp
namespace Materal.MergeBlock.HotPlug;

internal static class PluginStateMachine
{
    private static readonly Dictionary<PluginState, PluginState[]> Transitions = new()
    {
        [PluginState.Discovered] = [PluginState.Loading],
        [PluginState.Loading] = [PluginState.Loaded, PluginState.LoadingFailed],
        [PluginState.Loaded] = [PluginState.Starting, PluginState.Unloading],
        [PluginState.Starting] = [PluginState.Running, PluginState.StartingFailed],
        [PluginState.Running] = [PluginState.Stopping],
        [PluginState.Stopping] = [PluginState.Stopped, PluginState.StoppingFailed],
        [PluginState.Stopped] = [PluginState.Starting, PluginState.Unloading],
        [PluginState.Unloading] = [PluginState.Unloaded, PluginState.UnloadFailed],
        [PluginState.LoadingFailed] = [PluginState.Loading, PluginState.Unloading],
        [PluginState.StartingFailed] = [PluginState.Starting, PluginState.Unloading],
        [PluginState.StoppingFailed] = [PluginState.Stopping],
        [PluginState.UnloadFailed] = [PluginState.Unloading]
    };

    public static PluginState MoveTo(string pluginName, PluginState currentState, PluginState targetState)
    {
        if (!Transitions.TryGetValue(currentState, out PluginState[]? nextStates) || !nextStates.Contains(targetState))
        {
            throw new MergeBlockException($"插件[{pluginName}]状态不能从[{currentState}]切换到[{targetState}]");
        }
        return targetState;
    }
}
```

- [ ] **Step 4: 运行测试确认通过**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginStateMachineTest
```

Expected: `PluginStateMachineTest` 通过。

## Task 4: 实现运行时插件实例和插件级容器

**Files:**

- Create: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginInstance.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginServiceProviderFactory.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginRuntimeOptions.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginRuntimeTest.cs`

- [ ] **Step 1: GitNexus 影响分析**

Use GitNexus MCP:

```text
impact(repo: "Materal", target: "PluginLoadContext", direction: "upstream")
impact(repo: "Materal", target: "ModuleLoader", direction: "upstream")
```

Expected: 记录影响。本任务复用 `PluginLoadContext`，不改 `ModuleLoader`。

- [ ] **Step 2: 写插件实例失败测试**

Add to `PluginRuntimeTest.cs`:

```csharp
using Materal.MergeBlock.Abstractions.HotPlug;
using Materal.MergeBlock.HotPlug;
using Microsoft.Extensions.DependencyInjection;

namespace Materal.MergeBlock.Test.HotPlug;

[TestClass]
public sealed class PluginRuntimeTest
{
    [TestMethod]
    public void ToRuntimeInfo_ShouldExposeManifestAndState()
    {
        PluginManifest manifest = new()
        {
            Name = "DemoPlugin",
            DisplayName = "Demo 插件",
            Version = "1.0.0",
            EntryAssembly = "DemoPlugin.Application"
        };
        PluginInstance instance = new(manifest, "E:\\Plugins\\DemoPlugin");

        PluginRuntimeInfo info = instance.ToRuntimeInfo();

        Assert.AreEqual("DemoPlugin", info.Name);
        Assert.AreEqual("Demo 插件", info.DisplayName);
        Assert.AreEqual(PluginState.Discovered, info.State);
    }
}
```

- [ ] **Step 3: 运行测试确认失败**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginRuntimeTest
```

Expected: 编译失败，提示 `PluginInstance` 不存在。

- [ ] **Step 4: 实现配置和插件实例**

Create `PluginRuntimeOptions.cs`:

```csharp
namespace Materal.MergeBlock.HotPlug;

internal sealed class PluginRuntimeOptions : IOptions
{
    public const string ConfigKey = "MergeBlock:HotPlug";
    public bool Enable { get; set; } = true;
    public string PluginRootPath { get; set; } = "Plugins";
    public bool AutoLoadOnStartup { get; set; } = false;
    public bool WatchPluginFolder { get; set; } = false;
    public bool VerifyUnload { get; set; } = true;
}
```

Create `PluginInstance.cs`:

```csharp
using System.Runtime.Loader;

namespace Materal.MergeBlock.HotPlug;

internal sealed class PluginInstance(PluginManifest manifest, string rootPath) : IDisposable
{
    private readonly List<Assembly> _assemblies = [];
    private readonly List<IMergeBlockModule> _modules = [];
    private readonly List<IPluginBackgroundTask> _backgroundTasks = [];
    private ServiceProvider? _serviceProvider;
    private bool _disposed;

    public PluginManifest Manifest { get; } = manifest;
    public string RootPath { get; } = rootPath;
    public PluginState State { get; private set; } = PluginState.Discovered;
    public string? FailureMessage { get; private set; }
    public PluginLoadContext? LoadContext { get; private set; }
    public IReadOnlyList<Assembly> Assemblies => _assemblies;
    public IReadOnlyList<IMergeBlockModule> Modules => _modules;
    public IReadOnlyList<IPluginBackgroundTask> BackgroundTasks => _backgroundTasks;
    public IServiceProvider? ServiceProvider => _serviceProvider;

    public void MoveTo(PluginState state) => State = PluginStateMachine.MoveTo(Manifest.Name, State, state);

    public void SetFailed(PluginState state, Exception exception)
    {
        State = state;
        FailureMessage = exception.Message;
    }

    public void SetLoadContext(PluginLoadContext loadContext) => LoadContext = loadContext;

    public void AddAssembly(Assembly assembly) => _assemblies.Add(assembly);

    public void AddModule(IMergeBlockModule module) => _modules.Add(module);

    public void SetServiceProvider(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _backgroundTasks.AddRange(serviceProvider.GetServices<IPluginBackgroundTask>());
    }

    public PluginLoadContext? DetachLoadContext()
    {
        PluginLoadContext? loadContext = LoadContext;
        LoadContext = null;
        _assemblies.Clear();
        _modules.Clear();
        _backgroundTasks.Clear();
        return loadContext;
    }

    public PluginRuntimeInfo ToRuntimeInfo() => new()
    {
        Name = Manifest.Name,
        DisplayName = string.IsNullOrWhiteSpace(Manifest.DisplayName) ? Manifest.Name : Manifest.DisplayName,
        Version = Manifest.Version,
        RootPath = RootPath,
        State = State,
        FailureMessage = FailureMessage,
        Dependencies = [.. Manifest.Dependencies.Select(m => m.Name)]
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _serviceProvider?.Dispose();
    }
}
```

Create `PluginServiceProviderFactory.cs`:

```csharp
namespace Materal.MergeBlock.HotPlug;

internal sealed class PluginServiceProviderFactory(IServiceProvider hostServiceProvider)
{
    public ServiceProvider Create(PluginInstance pluginInstance)
    {
        ServiceCollection services = new();
        services.AddSingleton(pluginInstance.Manifest);
        services.AddSingleton(hostServiceProvider);
        services.AddLogging();
        foreach (Assembly assembly in pluginInstance.Assemblies)
        {
            services.AddAutoService(assembly);
        }
        return services.BuildServiceProvider();
    }
}
```

- [ ] **Step 5: 运行测试确认通过**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginRuntimeTest
```

Expected: 当前 `PluginRuntimeTest` 通过。

## Task 5: 实现 IPluginRuntime 加载、启动、停止骨架

**Files:**

- Create: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginRuntime.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/ServiceCollectionExtensions.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock/Extensions/DIExtensions.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginRuntimeTest.cs`

- [ ] **Step 1: GitNexus 影响分析**

Use GitNexus MCP:

```text
impact(repo: "Materal", target: "AddMergeBlockCore", file_path: "Materal.MergeBlock/Materal.MergeBlock/Extensions/DIExtensions.cs", direction: "upstream")
```

Expected: 记录影响。若风险为 HIGH 或 CRITICAL，先向用户说明。

- [ ] **Step 2: 补运行时注册失败测试**

Add to `PluginRuntimeTest.cs`:

```csharp
[TestMethod]
public async Task GetPluginsAsync_ShouldReturnLoadedPlugin()
{
    ServiceCollection services = new();
    services.AddLogging();
    services.AddMergeBlockHotPlug();
    await using ServiceProvider serviceProvider = services.BuildServiceProvider();
    IPluginRuntime runtime = serviceProvider.GetRequiredService<IPluginRuntime>();

    string pluginPath = CreateManifestOnlyPlugin("DemoPlugin");
    PluginOperationResult loadResult = await runtime.LoadAsync(pluginPath);
    IReadOnlyList<PluginRuntimeInfo> plugins = await runtime.GetPluginsAsync();

    Assert.IsTrue(loadResult.Success);
    Assert.AreEqual(1, plugins.Count);
    Assert.AreEqual("DemoPlugin", plugins[0].Name);
    Assert.AreEqual(PluginState.Loaded, plugins[0].State);
}

private static string CreateManifestOnlyPlugin(string name)
{
    string pluginPath = Path.Combine(Path.GetTempPath(), "MergeBlockHotPlugTests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(pluginPath);
    File.WriteAllText(Path.Combine(pluginPath, "plugin.json"), $$"""
    {
      "Name": "{{name}}",
      "DisplayName": "{{name}}",
      "Version": "1.0.0",
      "EntryAssembly": "{{name}}.Application",
      "AllowRuntimeLoad": true,
      "AllowRuntimeUnload": true
    }
    """);
    return pluginPath;
}
```

- [ ] **Step 3: 运行测试确认失败**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginRuntimeTest
```

Expected: 编译失败，提示 `AddMergeBlockHotPlug` 或 `PluginRuntime` 不存在。

- [ ] **Step 4: 实现运行时注册扩展**

Create `ServiceCollectionExtensions.cs`:

```csharp
namespace Materal.MergeBlock.HotPlug;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMergeBlockHotPlug(this IServiceCollection services)
    {
        services.TryAddSingleton<PluginManifestReader>();
        services.TryAddSingleton<PluginServiceProviderFactory>();
        services.TryAddSingleton<PluginUnloadVerifier>();
        services.TryAddSingleton<IPluginRuntime, PluginRuntime>();
        return services;
    }
}
```

- [ ] **Step 5: 实现运行时骨架**

Create `PluginRuntime.cs`:

```csharp
namespace Materal.MergeBlock.HotPlug;

internal sealed class PluginRuntime(PluginManifestReader manifestReader, PluginServiceProviderFactory serviceProviderFactory, IServiceProvider hostServiceProvider, ILogger<PluginRuntime>? logger = null) : IPluginRuntime
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, PluginInstance> _plugins = new(StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<PluginRuntimeInfo>> GetPluginsAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return [.. _plugins.Values.Select(m => m.ToRuntimeInfo())];
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<PluginOperationResult> LoadAsync(string pluginPath, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            PluginManifest manifest = manifestReader.Read(pluginPath);
            if (!manifest.AllowRuntimeLoad) return PluginOperationResult.Fail($"插件[{manifest.Name}]不允许运行时加载");
            if (_plugins.ContainsKey(manifest.Name)) return PluginOperationResult.Fail($"插件[{manifest.Name}]已加载");
            PluginInstance instance = new(manifest, pluginPath);
            try
            {
                instance.MoveTo(PluginState.Loading);
                LoadAssemblies(instance);
                ServiceProvider serviceProvider = serviceProviderFactory.Create(instance);
                instance.SetServiceProvider(serviceProvider);
                await InvokeLifecycleAsync(instance, m => m.OnPluginLoadingAsync(CreateLifecycleContext(instance)));
                instance.MoveTo(PluginState.Loaded);
                await InvokeLifecycleAsync(instance, m => m.OnPluginLoadedAsync(CreateLifecycleContext(instance)));
                _plugins.Add(manifest.Name, instance);
                return PluginOperationResult.Succeed(instance.ToRuntimeInfo());
            }
            catch (Exception exception)
            {
                logger?.LogError(exception, "加载插件[{PluginName}]失败", manifest.Name);
                instance.SetFailed(PluginState.LoadingFailed, exception);
                instance.Dispose();
                return PluginOperationResult.Fail(exception.Message, instance.ToRuntimeInfo());
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<PluginOperationResult> StartAsync(string pluginName, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_plugins.TryGetValue(pluginName, out PluginInstance? instance)) return PluginOperationResult.Fail($"插件[{pluginName}]未加载");
            try
            {
                instance.MoveTo(PluginState.Starting);
                PluginLifecycleContext context = CreateLifecycleContext(instance);
                await InvokeLifecycleAsync(instance, m => m.OnPluginStartingAsync(context));
                foreach (IPluginBackgroundTask task in instance.BackgroundTasks)
                {
                    await task.StartAsync(new PluginTaskContext(instance.ServiceProvider!, instance.Manifest), cancellationToken);
                }
                instance.MoveTo(PluginState.Running);
                await InvokeLifecycleAsync(instance, m => m.OnPluginStartedAsync(context));
                return PluginOperationResult.Succeed(instance.ToRuntimeInfo());
            }
            catch (Exception exception)
            {
                logger?.LogError(exception, "启动插件[{PluginName}]失败", pluginName);
                instance.SetFailed(PluginState.StartingFailed, exception);
                return PluginOperationResult.Fail(exception.Message, instance.ToRuntimeInfo());
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<PluginOperationResult> StopAsync(string pluginName, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_plugins.TryGetValue(pluginName, out PluginInstance? instance)) return PluginOperationResult.Fail($"插件[{pluginName}]未加载");
            try
            {
                instance.MoveTo(PluginState.Stopping);
                PluginLifecycleContext context = CreateLifecycleContext(instance);
                await InvokeLifecycleAsync(instance, m => m.OnPluginStoppingAsync(context));
                foreach (IPluginBackgroundTask task in instance.BackgroundTasks.Reverse())
                {
                    await task.StopAsync(cancellationToken);
                }
                instance.MoveTo(PluginState.Stopped);
                await InvokeLifecycleAsync(instance, m => m.OnPluginStoppedAsync(context));
                return PluginOperationResult.Succeed(instance.ToRuntimeInfo());
            }
            catch (Exception exception)
            {
                logger?.LogError(exception, "停止插件[{PluginName}]失败", pluginName);
                instance.SetFailed(PluginState.StoppingFailed, exception);
                return PluginOperationResult.Fail(exception.Message, instance.ToRuntimeInfo());
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<PluginOperationResult> UnloadAsync(string pluginName, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_plugins.TryGetValue(pluginName, out PluginInstance? instance)) return PluginOperationResult.Fail($"插件[{pluginName}]未加载");
            if (instance.State == PluginState.Running) return PluginOperationResult.Fail($"插件[{pluginName}]运行中，不能卸载", instance.ToRuntimeInfo());
            instance.MoveTo(PluginState.Unloading);
            await InvokeLifecycleAsync(instance, m => m.OnPluginUnloadingAsync(CreateLifecycleContext(instance)));
            _plugins.Remove(pluginName);
            instance.Dispose();
            instance.LoadContext?.Unload();
            instance.MoveTo(PluginState.Unloaded);
            return PluginOperationResult.Succeed(instance.ToRuntimeInfo());
        }
        finally
        {
            _lock.Release();
        }
    }

    private static void LoadAssemblies(PluginInstance instance)
    {
        string assemblyPath = Path.Combine(instance.RootPath, instance.Manifest.EntryAssembly + ".dll");
        if (!File.Exists(assemblyPath)) return;
        PluginLoadContext loadContext = new(assemblyPath, instance.Manifest.Name, instance.Manifest.IsCollectible);
        instance.SetLoadContext(loadContext);
        Assembly assembly = loadContext.LoadFromAssemblyName(new AssemblyName(instance.Manifest.EntryAssembly));
        instance.AddAssembly(assembly);
        foreach (Type type in assembly.GetTypesByFilter(IMergeBlockModule.IsMergeBlockModule))
        {
            if (Activator.CreateInstance(type) is IMergeBlockModule module) instance.AddModule(module);
        }
    }

    private PluginLifecycleContext CreateLifecycleContext(PluginInstance instance) => new(hostServiceProvider, instance.ServiceProvider ?? hostServiceProvider, instance.Manifest);

    private static async Task InvokeLifecycleAsync(PluginInstance instance, Func<IHotPlugModule, Task> action)
    {
        foreach (IHotPlugModule module in instance.Modules.OfType<IHotPlugModule>())
        {
            await action(module);
        }
    }
}
```

- [ ] **Step 6: 接入 AddMergeBlockCore**

Modify `Materal.MergeBlock/Materal.MergeBlock/Extensions/DIExtensions.cs`:

```csharp
services.AddMergeBlockHotPlug();
```

Place it after `services.AddMergeBlockLoggerFactory();` and before `PluginManager pluginManager = new();`.

- [ ] **Step 7: 运行测试确认通过**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginRuntimeTest
```

Expected: `PluginRuntimeTest` 通过。

## Task 6: 实现动态 Endpoint 数据源

**Files:**

- Create: `Materal.MergeBlock/Materal.MergeBlock.Web.Abstractions/HotPlug/IPluginEndpointContributor.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Web.Abstractions/HotPlug/PluginEndpointContributionContext.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Web/HotPlug/PluginEndpointDataSource.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.Web/HotPlug/PluginRouteManager.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.Web/WebModule.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginEndpointDataSourceTest.cs`

- [ ] **Step 1: GitNexus 影响分析**

Use GitNexus MCP:

```text
impact(repo: "Materal", target: "WebModule", direction: "upstream")
```

Expected: 记录影响。若风险为 HIGH 或 CRITICAL，先向用户说明。

- [ ] **Step 2: 写 Endpoint DataSource 失败测试**

Create `PluginEndpointDataSourceTest.cs`:

```csharp
using Materal.MergeBlock.Abstractions.HotPlug;
using Materal.MergeBlock.HotPlug;
using Materal.MergeBlock.Web.Abstractions.HotPlug;
using Materal.MergeBlock.Web.HotPlug;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Materal.MergeBlock.Test.HotPlug;

[TestClass]
public sealed class PluginEndpointDataSourceTest
{
    [TestMethod]
    public void AddEndpoints_ShouldExposeEndpoint()
    {
        PluginEndpointDataSource dataSource = new();
        RouteEndpoint endpoint = new(
            context => context.Response.WriteAsync("ok"),
            RoutePatternFactory.Parse("/plugins/demo/ping"),
            0,
            [],
            "DemoPing");

        dataSource.ReplacePluginEndpoints("DemoPlugin", [endpoint]);

        Assert.AreEqual(1, dataSource.Endpoints.Count);
        Assert.AreEqual("DemoPing", dataSource.Endpoints[0].DisplayName);
    }

    [TestMethod]
    public void RemovePlugin_ShouldRemoveOnlyTargetPluginEndpoints()
    {
        PluginEndpointDataSource dataSource = new();
        RouteEndpoint demo = new(_ => Task.CompletedTask, RoutePatternFactory.Parse("/demo"), 0, [], "Demo");
        RouteEndpoint core = new(_ => Task.CompletedTask, RoutePatternFactory.Parse("/core"), 0, [], "Core");

        dataSource.ReplacePluginEndpoints("DemoPlugin", [demo]);
        dataSource.ReplacePluginEndpoints("CorePlugin", [core]);
        dataSource.RemovePluginEndpoints("DemoPlugin");

        Assert.AreEqual(1, dataSource.Endpoints.Count);
        Assert.AreEqual("Core", dataSource.Endpoints[0].DisplayName);
    }
}
```

- [ ] **Step 3: 运行测试确认失败**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginEndpointDataSourceTest
```

Expected: 编译失败，提示 `PluginEndpointDataSource` 不存在或测试项目缺少 Web 引用。

- [ ] **Step 4: 给测试项目增加 Web 引用**

Modify `Materal.MergeBlock/Materal.MergeBlock.Test/Materal.MergeBlock.Test.csproj`:

```xml
<ProjectReference Include="..\Materal.MergeBlock.Web\Materal.MergeBlock.Web.csproj" />
```

- [ ] **Step 5: 新增 Web 抽象**

Create `IPluginEndpointContributor.cs`:

```csharp
namespace Materal.MergeBlock.Web.Abstractions.HotPlug;

public interface IPluginEndpointContributor
{
    Task ContributeAsync(PluginEndpointContributionContext context);
}
```

Create `PluginEndpointContributionContext.cs`:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Materal.MergeBlock.Web.Abstractions.HotPlug;

public sealed class PluginEndpointContributionContext(string pluginName)
{
    private readonly List<Endpoint> _endpoints = [];
    public string PluginName { get; } = pluginName;
    public IReadOnlyList<Endpoint> Endpoints => _endpoints;

    public void MapGet(string pattern, RequestDelegate requestDelegate, string? displayName = null)
    {
        RouteEndpoint endpoint = new(
            requestDelegate,
            RoutePatternFactory.Parse(pattern),
            0,
            [],
            displayName ?? $"{PluginName}:{pattern}");
        _endpoints.Add(endpoint);
    }
}
```

- [ ] **Step 6: 实现动态 DataSource**

Create `PluginEndpointDataSource.cs`:

```csharp
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Materal.MergeBlock.Web.HotPlug;

public sealed class PluginEndpointDataSource : EndpointDataSource
{
    private readonly object _lock = new();
    private readonly Dictionary<string, IReadOnlyList<Endpoint>> _pluginEndpoints = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource _changeTokenSource = new();

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            lock (_lock)
            {
                return [.. _pluginEndpoints.Values.SelectMany(m => m)];
            }
        }
    }

    public override IChangeToken GetChangeToken() => new CancellationChangeToken(_changeTokenSource.Token);

    public void ReplacePluginEndpoints(string pluginName, IReadOnlyList<Endpoint> endpoints)
    {
        lock (_lock)
        {
            _pluginEndpoints[pluginName] = endpoints;
            NotifyChanged();
        }
    }

    public void RemovePluginEndpoints(string pluginName)
    {
        lock (_lock)
        {
            if (_pluginEndpoints.Remove(pluginName)) NotifyChanged();
        }
    }

    private void NotifyChanged()
    {
        CancellationTokenSource previous = _changeTokenSource;
        _changeTokenSource = new CancellationTokenSource();
        previous.Cancel();
        previous.Dispose();
    }
}
```

Create `PluginRouteManager.cs`:

```csharp
using Materal.MergeBlock.Web.Abstractions.HotPlug;

namespace Materal.MergeBlock.Web.HotPlug;

public sealed class PluginRouteManager(PluginEndpointDataSource dataSource)
{
    public async Task RegisterAsync(string pluginName, IServiceProvider pluginServiceProvider)
    {
        IEnumerable<IPluginEndpointContributor> contributors = pluginServiceProvider.GetServices<IPluginEndpointContributor>();
        PluginEndpointContributionContext context = new(pluginName);
        foreach (IPluginEndpointContributor contributor in contributors)
        {
            await contributor.ContributeAsync(context);
        }
        dataSource.ReplacePluginEndpoints(pluginName, context.Endpoints);
    }

    public void Remove(string pluginName) => dataSource.RemovePluginEndpoints(pluginName);
}
```

- [ ] **Step 7: 在 WebModule 中接入 DataSource**

Modify `WebModule.OnConfigureServices`:

```csharp
context.Services.TryAddSingleton<PluginEndpointDataSource>();
context.Services.TryAddSingleton<PluginRouteManager>();
```

Modify `WebModule.OnApplicationInitialization` before or after `webApplication.MapControllers();`:

```csharp
PluginEndpointDataSource? pluginEndpointDataSource = context.ServiceProvider.GetService<PluginEndpointDataSource>();
if (pluginEndpointDataSource is not null)
{
    webApplication.DataSources.Add(pluginEndpointDataSource);
}
```

- [ ] **Step 8: 运行测试确认通过**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginEndpointDataSourceTest
```

Expected: `PluginEndpointDataSourceTest` 通过。

## Task 7: 让 PluginRuntime 启停 Endpoint 和后台任务

**Files:**

- Modify: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginRuntime.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginServiceProviderFactory.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginRuntimeTest.cs`

- [ ] **Step 1: 写停止时任务反向停止测试**

Add to `PluginRuntimeTest.cs`:

```csharp
[TestMethod]
public async Task StopAsync_ShouldMoveRunningPluginToStopped()
{
    ServiceCollection services = new();
    services.AddLogging();
    services.AddMergeBlockHotPlug();
    await using ServiceProvider serviceProvider = services.BuildServiceProvider();
    IPluginRuntime runtime = serviceProvider.GetRequiredService<IPluginRuntime>();

    string pluginPath = CreateManifestOnlyPlugin("DemoPlugin");
    await runtime.LoadAsync(pluginPath);
    PluginOperationResult startResult = await runtime.StartAsync("DemoPlugin");
    PluginOperationResult stopResult = await runtime.StopAsync("DemoPlugin");

    Assert.IsTrue(startResult.Success);
    Assert.IsTrue(stopResult.Success);
    Assert.AreEqual(PluginState.Stopped, stopResult.Plugin?.State);
}
```

- [ ] **Step 2: 运行测试**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~StopAsync_ShouldMoveRunningPluginToStopped
```

Expected: 测试通过或因状态流转问题失败。若失败，只修 `PluginRuntime.StartAsync` 和 `StopAsync`。

- [ ] **Step 3: 接入 PluginRouteManager**

Modify `PluginRuntime` constructor to accept optional route manager:

```csharp
internal sealed class PluginRuntime(
    PluginManifestReader manifestReader,
    PluginServiceProviderFactory serviceProviderFactory,
    IServiceProvider hostServiceProvider,
    ILogger<PluginRuntime>? logger = null) : IPluginRuntime
```

Inside `StartAsync`, after background task start, resolve route manager by reflection-safe service lookup:

```csharp
object? routeManager = hostServiceProvider.GetService(Type.GetType("Materal.MergeBlock.Web.HotPlug.PluginRouteManager, Materal.MergeBlock.Web"));
if (routeManager is not null)
{
    MethodInfo? registerMethod = routeManager.GetType().GetMethod("RegisterAsync");
    if (registerMethod is not null)
    {
        object? task = registerMethod.Invoke(routeManager, [instance.Manifest.Name, instance.ServiceProvider!]);
        if (task is Task routeTask) await routeTask;
    }
}
```

Inside `StopAsync`, before state becomes `Stopped`, remove routes:

```csharp
object? routeManager = hostServiceProvider.GetService(Type.GetType("Materal.MergeBlock.Web.HotPlug.PluginRouteManager, Materal.MergeBlock.Web"));
if (routeManager is not null)
{
    MethodInfo? removeMethod = routeManager.GetType().GetMethod("Remove");
    removeMethod?.Invoke(routeManager, [instance.Manifest.Name]);
}
```

This avoids a core project reference to `Materal.MergeBlock.Web`.

- [ ] **Step 4: 运行插件运行时测试**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug.PluginRuntimeTest
```

Expected: `PluginRuntimeTest` 全部通过。

## Task 8: 添加最小管理 API

**Files:**

- Create: `Materal.MergeBlock/Materal.MergeBlock.Web/HotPlug/PluginManagementEndpointContributor.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.Web/WebModule.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginEndpointDataSourceTest.cs`

- [ ] **Step 1: 写管理 Endpoint 贡献器测试**

Add to `PluginEndpointDataSourceTest.cs`:

```csharp
[TestMethod]
public async Task ManagementContributor_ShouldExposePluginListEndpoint()
{
    ServiceCollection services = new();
    services.AddLogging();
    services.AddMergeBlockHotPlug();
    await using ServiceProvider serviceProvider = services.BuildServiceProvider();
    PluginEndpointContributionContext context = new("MergeBlockHotPlug");
    PluginManagementEndpointContributor contributor = new(serviceProvider.GetRequiredService<IPluginRuntime>());

    await contributor.ContributeAsync(context);

    Assert.IsTrue(context.Endpoints.Any(m => m.DisplayName == "MergeBlockHotPlug:GetPlugins"));
}
```

- [ ] **Step 2: 运行测试确认失败**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~ManagementContributor_ShouldExposePluginListEndpoint
```

Expected: 编译失败，提示 `PluginManagementEndpointContributor` 不存在。

- [ ] **Step 3: 实现管理 Endpoint 贡献器**

Create `PluginManagementEndpointContributor.cs`:

```csharp
using Materal.MergeBlock.Web.Abstractions.HotPlug;
using Microsoft.AspNetCore.Http;

namespace Materal.MergeBlock.Web.HotPlug;

public sealed class PluginManagementEndpointContributor(IPluginRuntime pluginRuntime) : IPluginEndpointContributor
{
    public Task ContributeAsync(PluginEndpointContributionContext context)
    {
        context.MapGet("/MergeBlockHotPlugAPI/Plugins", async httpContext =>
        {
            IReadOnlyList<PluginRuntimeInfo> plugins = await pluginRuntime.GetPluginsAsync(httpContext.RequestAborted);
            await httpContext.Response.WriteAsJsonAsync(plugins, httpContext.RequestAborted);
        }, "MergeBlockHotPlug:GetPlugins");
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 4: 在 WebModule 注册管理贡献器**

Modify `WebModule.OnConfigureServices`:

```csharp
context.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPluginEndpointContributor, PluginManagementEndpointContributor>());
```

Also add the needed global using or explicit using:

```csharp
using Materal.MergeBlock.Web.Abstractions.HotPlug;
using Materal.MergeBlock.Web.HotPlug;
```

- [ ] **Step 5: 运行测试确认通过**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~PluginEndpointDataSourceTest
```

Expected: `PluginEndpointDataSourceTest` 通过。

## Task 9: 添加卸载验证工具

**Files:**

- Create: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginUnloadVerifier.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock/HotPlug/PluginRuntime.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.Test/HotPlug/PluginRuntimeTest.cs`

- [ ] **Step 1: 写卸载结果测试**

Add to `PluginRuntimeTest.cs`:

```csharp
[TestMethod]
public async Task UnloadAsync_ShouldRemoveStoppedPluginFromRuntime()
{
    ServiceCollection services = new();
    services.AddLogging();
    services.AddMergeBlockHotPlug();
    await using ServiceProvider serviceProvider = services.BuildServiceProvider();
    IPluginRuntime runtime = serviceProvider.GetRequiredService<IPluginRuntime>();

    string pluginPath = CreateManifestOnlyPlugin("DemoPlugin");
    await runtime.LoadAsync(pluginPath);
    PluginOperationResult unloadResult = await runtime.UnloadAsync("DemoPlugin");
    IReadOnlyList<PluginRuntimeInfo> plugins = await runtime.GetPluginsAsync();

    Assert.IsTrue(unloadResult.Success);
    Assert.AreEqual(0, plugins.Count);
}
```

- [ ] **Step 2: 实现卸载验证工具**

Create `PluginUnloadVerifier.cs`:

```csharp
namespace Materal.MergeBlock.HotPlug;

internal sealed class PluginUnloadVerifier
{
    public bool Verify(PluginLoadContext? loadContext)
    {
        if (loadContext is null) return true;
        WeakReference weakReference = new(loadContext, true);
        loadContext.Unload();
        for (int i = 0; weakReference.IsAlive && i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        return !weakReference.IsAlive;
    }
}
```

- [ ] **Step 3: 在 PluginRuntime.UnloadAsync 中使用验证工具**

Modify constructor to include `PluginUnloadVerifier unloadVerifier`.

Replace direct `instance.LoadContext?.Unload();` with:

```csharp
PluginLoadContext? loadContext = instance.DetachLoadContext();
instance.Dispose();
bool unloaded = unloadVerifier.Verify(loadContext);
if (!unloaded)
{
    instance.SetFailed(PluginState.UnloadFailed, new MergeBlockException($"插件[{pluginName}]加载上下文未能释放"));
    return PluginOperationResult.Fail(instance.FailureMessage!, instance.ToRuntimeInfo());
}
```

Only remove plugin from `_plugins` after verification succeeds. For manifest-only plugin with null ALC, verification returns `true` immediately.

- [ ] **Step 4: 运行测试确认通过**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~UnloadAsync_ShouldRemoveStoppedPluginFromRuntime
```

Expected: 测试通过。

## Task 10: 集成验证和收尾

**Files:**

- Review: all files changed in this plan.

- [ ] **Step 1: 运行热插拔测试**

Run:

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.Test\Materal.MergeBlock.Test.csproj --framework net8.0 --filter FullyQualifiedName~HotPlug
```

Expected: HotPlug 相关测试全部通过。

- [ ] **Step 2: 运行直接相关项目构建**

Run:

```powershell
dotnet build .\Materal.MergeBlock\Materal.MergeBlock.Abstractions\Materal.MergeBlock.Abstractions.csproj --framework net8.0
dotnet build .\Materal.MergeBlock\Materal.MergeBlock\Materal.MergeBlock.csproj --framework net8.0
dotnet build .\Materal.MergeBlock\Materal.MergeBlock.Web.Abstractions\Materal.MergeBlock.Web.Abstractions.csproj --framework net8.0
dotnet build .\Materal.MergeBlock\Materal.MergeBlock.Web\Materal.MergeBlock.Web.csproj --framework net8.0
```

Expected: 四个项目构建通过。

- [ ] **Step 3: 检查 CRLF**

Run:

```powershell
$files = @(
  ".\Materal.MergeBlock\Materal.MergeBlock.Abstractions\HotPlug\PluginState.cs",
  ".\Materal.MergeBlock\Materal.MergeBlock.Abstractions\HotPlug\PluginManifest.cs",
  ".\Materal.MergeBlock\Materal.MergeBlock.Abstractions\HotPlug\PluginDependencyManifest.cs",
  ".\Materal.MergeBlock\Materal.MergeBlock\HotPlug\PluginRuntime.cs",
  ".\Materal.MergeBlock\Materal.MergeBlock.Web\HotPlug\PluginEndpointDataSource.cs"
)
foreach ($file in $files) {
  $text = [System.IO.File]::ReadAllText((Resolve-Path $file))
  $lfOnly = ([regex]::Matches($text, "(?<!`r)`n")).Count
  "$file LFOnly=$lfOnly"
}
```

Expected: 每个文件 `LFOnly=0`。

- [ ] **Step 4: 运行 GitNexus change detection**

Use GitNexus MCP:

```text
detect_changes(repo: "Materal", scope: "all")
```

Expected: 影响范围集中在 MergeBlock 热插拔相关抽象、运行时和 Web 动态 Endpoint。如果出现无关模块影响，先检查 diff。

- [ ] **Step 5: 最终工作区检查**

Run:

```powershell
git status --short
git -C .\Materal.MergeBlock status --short
```

Expected: 只包含本计划产生的文件、文档和已有用户改动。不自动提交。

## 交付标准

- HotPlug 测试通过。
- 直接相关四个项目 `net8.0` 构建通过。
- `IPluginRuntime` 可以加载 manifest-only 插件并返回状态。
- 插件可以从 `Loaded` 启动到 `Running`，再停止到 `Stopped`。
- 插件停止后可以卸载并从运行时列表移除。
- Web 项目接入动态 Endpoint DataSource。
- 管理 Endpoint 能暴露插件列表查询。
- GitNexus change detection 输出符合预期。

