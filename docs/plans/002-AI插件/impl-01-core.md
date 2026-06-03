# Materal.MergeBlock.AI 核心实施计划

> **面向代理执行者：** 必需子技能：按任务逐项实施本计划时，请使用 `superpowers:subagent-driven-development`（推荐）或 `superpowers:executing-plans`。步骤使用复选框（`- [ ]`）语法便于跟踪。

**目标：** 构建核心 MMB AI 集成层，提供稳定抽象，用于工具元数据、上下文冻结、提示词贡献、审计和 MergeBlock 模块注册。

**架构：** 核心阶段创建两个项目：`Materal.MergeBlock.AI.Abstractions` 用于公共契约，`Materal.MergeBlock.AI` 用于具体注册、扫描、聚合和默认日志/审计。它不实现 Provider 包，也不包含 Web/SSE 或业务专用行为。

**技术栈：** C#/.NET `net8.0;net9.0;net10.0`、MergeBlock 模块、Microsoft Agent Framework 核心集成、`Microsoft.Extensions.DependencyInjection`、`Microsoft.Extensions.Options`、MSTest。

---

## 文件

- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Materal.MergeBlock.AI.Abstractions.csproj`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/GlobalUsings.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Options/AIOptions.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Options/AIAgentOptions.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolExecutionMode.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolDescriptor.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/MergeBlockAIToolAttribute.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/IAIToolMetadataProvider.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Context/AIContextBuilderContext.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Context/IReadOnlyAIContext.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Context/AIContextSnapshot.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Context/IAIContextProvider.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Prompts/AIPromptContributionContext.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Prompts/IAIPromptContributor.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Auditing/AIToolCallStatus.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Auditing/AIToolCallAuditContext.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Auditing/IAIToolCallAuditor.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI/Materal.MergeBlock.AI.csproj`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI/GlobalUsings.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI/AIModule.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI/Extensions/DIExtensions.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI/Context/AIContextBuilder.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI/Prompts/AIPromptBuilder.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI/Tools/AIToolRegistry.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI/Tools/AIToolScanner.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI/Auditing/LoggingAIToolCallAuditor.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Test/Materal.MergeBlock.AI.Test.csproj`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Test/GlobalUsings.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Test/Options/AIOptionsTest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Test/Context/AIContextSnapshotTest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Test/Prompts/AIPromptBuilderTest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Test/Tools/AIToolScannerTest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Test/Auditing/LoggingAIToolCallAuditorTest.cs`
- 修改：`Directory.Packages.props`
- 修改：`Materal.slnx`
- 修改：`Materal.Packable.slnx`

## 任务 1：创建核心项目

- [ ] **步骤 1：为模块脚手架运行 GitNexus 影响检查**

运行：

```powershell
git status --short
```

预期：可能存在用户已有改动；不要回滚它们。

如果实现需要修改共享模块加载符号，请在修改前运行 GitNexus 影响分析。本阶段应避免修改 `MergeBlockModule`、`ModuleLoader` 或 `DIExtensions.AddMergeBlockCore`。

- [ ] **步骤 2：添加项目文件**

创建 `Materal.MergeBlock.AI.Abstractions.csproj`：

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="../../Packable.props" />
	<PropertyGroup>
		<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
		<Title>Materal MergeBlock AI抽象包</Title>
		<Description>Materal MergeBlock AI抽象包</Description>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
		<PackageReference Include="Microsoft.Extensions.Options" />
	</ItemGroup>
	<ItemGroup>
		<ProjectReference Include="..\Materal.MergeBlock.Abstractions\Materal.MergeBlock.Abstractions.csproj" />
	</ItemGroup>
</Project>
```

创建 `Materal.MergeBlock.AI.csproj`：

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="../../MergeBlockLibrary.props" />
	<PropertyGroup>
		<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
		<Title>Materal MergeBlock AI模块</Title>
		<Description>Materal MergeBlock AI模块</Description>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
		<PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" />
	</ItemGroup>
	<ItemGroup>
		<ProjectReference Include="..\Materal.MergeBlock.AI.Abstractions\Materal.MergeBlock.AI.Abstractions.csproj" />
		<ProjectReference Include="..\Materal.MergeBlock\Materal.MergeBlock.csproj" />
	</ItemGroup>
</Project>
```

创建 `Materal.MergeBlock.AI.Test.csproj`：

```xml
<Project Sdk="MSTest.Sdk/4.1.0">
	<Import Project="../../Common.props" />
	<PropertyGroup>
		<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
		<IsTestProject>true</IsTestProject>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="Microsoft.Extensions.DependencyInjection" />
	</ItemGroup>
	<ItemGroup>
		<ProjectReference Include="..\Materal.MergeBlock.AI\Materal.MergeBlock.AI.csproj" />
	</ItemGroup>
</Project>
```

- [ ] **步骤 3：添加包版本**

修改仓库级 `Directory.Packages.props`：

```xml
<PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.8" />
```

本阶段不要添加 OpenAI、Azure OpenAI、Anthropic、Ollama 或其他 Provider 包版本。

- [ ] **步骤 4：添加项目到解决方案**

将以下路径添加到 `Materal.slnx` 和 `Materal.Packable.slnx` 中靠近现有 MergeBlock 项目的位置：

```xml
<Project Path="Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Materal.MergeBlock.AI.Abstractions.csproj" />
<Project Path="Materal.MergeBlock/Materal.MergeBlock.AI/Materal.MergeBlock.AI.csproj" />
```

- [ ] **步骤 5：验证项目还原**

运行：

```powershell
dotnet restore .\Materal.slnx
```

预期：所有目标框架还原成功。

- [ ] **步骤 6：添加 global using 文件**

创建 `Materal.MergeBlock.AI.Abstractions/GlobalUsings.cs`：

```csharp
global using Materal.MergeBlock.Abstractions;
```

创建 `Materal.MergeBlock.AI/GlobalUsings.cs`：

```csharp
global using Materal.MergeBlock.AI.Abstractions.Auditing;
global using Materal.MergeBlock.AI.Abstractions.Context;
global using Materal.MergeBlock.AI.Abstractions.Options;
global using Materal.MergeBlock.AI.Abstractions.Prompts;
global using Materal.MergeBlock.AI.Abstractions.Tools;
global using Materal.MergeBlock.AI.Auditing;
global using Materal.MergeBlock.AI.Extensions;
global using Materal.MergeBlock.AI.Prompts;
global using Materal.MergeBlock.AI.Tools;
global using Materal.MergeBlock.Abstractions;
global using Materal.MergeBlock.Extensions;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using System.Collections.ObjectModel;
global using System.Reflection;
```

创建 `Materal.MergeBlock.AI.Test/GlobalUsings.cs`：

```csharp
global using Materal.MergeBlock.AI;
global using Materal.MergeBlock.AI.Abstractions.Auditing;
global using Materal.MergeBlock.AI.Abstractions.Context;
global using Materal.MergeBlock.AI.Abstractions.Options;
global using Materal.MergeBlock.AI.Abstractions.Prompts;
global using Materal.MergeBlock.AI.Abstractions.Tools;
global using Materal.MergeBlock.AI.Auditing;
global using Materal.MergeBlock.AI.Extensions;
global using Materal.MergeBlock.AI.Prompts;
global using Materal.MergeBlock.AI.Tools;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.VisualStudio.TestTools.UnitTesting;
```

## 任务 2：实现 Options

- [ ] **步骤 1：编写 options 测试**

创建 `Materal.MergeBlock.AI.Test/Options/AIOptionsTest.cs`：

```csharp
namespace Materal.MergeBlock.AI.Test.Options;

[TestClass]
public class AIOptionsTest
{
    [TestMethod]
    public void NewOptions_ShouldUseSafeDefaults()
    {
        AIOptions options = new();

        Assert.AreEqual("MergeBlock:AI", AIOptions.ConfigKey);
        Assert.IsTrue(options.Enable);
        Assert.AreEqual("default", options.DefaultAgentName);
        Assert.IsTrue(options.ScanTools);
        Assert.IsTrue(options.RequireToolAuthorization);
        Assert.IsTrue(options.AuditToolCalls);
    }
}
```

- [ ] **步骤 2：实现 options**

创建 `AIOptions.cs`：

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Options;

/// <summary>
/// AI配置
/// </summary>
public class AIOptions : IOptions
{
    /// <summary>
    /// 配置节点
    /// </summary>
    public const string ConfigKey = "MergeBlock:AI";
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; } = true;
    /// <summary>
    /// 默认Agent名称
    /// </summary>
    public string DefaultAgentName { get; set; } = "default";
    /// <summary>
    /// 是否扫描工具
    /// </summary>
    public bool ScanTools { get; set; } = true;
    /// <summary>
    /// 是否要求工具授权
    /// </summary>
    public bool RequireToolAuthorization { get; set; } = true;
    /// <summary>
    /// 是否审计工具调用
    /// </summary>
    public bool AuditToolCalls { get; set; } = true;
}
```

创建 `AIAgentOptions.cs`：

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Options;

/// <summary>
/// AI Agent配置
/// </summary>
public class AIAgentOptions
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = "default";
}
```

- [ ] **步骤 3：运行 options 测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIOptionsTest
```

预期：通过。

## 任务 3：实现工具元数据

- [ ] **步骤 1：编写工具元数据测试**

创建 `Materal.MergeBlock.AI.Test/Tools/AIToolDescriptorTest.cs`：

```csharp
namespace Materal.MergeBlock.AI.Test.Tools;

[TestClass]
public class AIToolDescriptorTest
{
    [TestMethod]
    public void Descriptor_ShouldPreserveExecutionMode()
    {
        AIToolDescriptor local = new()
        {
            Name = "queryOrder",
            Description = "查询订单",
            ExecutionMode = AIToolExecutionMode.Local,
            InputType = typeof(QueryOrderInput),
            ResultType = typeof(QueryOrderResult)
        };

        AIToolDescriptor remote = new()
        {
            Name = "runClientAction",
            Description = "执行客户端操作",
            ExecutionMode = AIToolExecutionMode.Remote,
            InputType = typeof(RunClientActionInput),
            ResultType = typeof(Dictionary<string, object?>)
        };

        Assert.AreEqual(AIToolExecutionMode.Local, local.ExecutionMode);
        Assert.AreEqual(AIToolExecutionMode.Remote, remote.ExecutionMode);
    }

    private sealed class QueryOrderInput;
    private sealed class QueryOrderResult;
    private sealed class RunClientActionInput;
}
```

- [ ] **步骤 2：实现工具契约**

创建 `AIToolExecutionMode.cs`、`AIToolDescriptor.cs`、`MergeBlockAIToolAttribute.cs` 和 `IAIToolMetadataProvider.cs`，使用设计文档中的中文 XML 注释风格。

核心接口语义：

```csharp
public enum AIToolExecutionMode
{
    Local,
    Remote
}

public class AIToolDescriptor
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public AIToolExecutionMode ExecutionMode { get; init; }
    public string? RequiredPermission { get; init; }
    public Type? InputType { get; init; }
    public Type? ResultType { get; init; }
    public Type? ImplementationType { get; init; }
}
```

- [ ] **步骤 3：运行元数据测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIToolDescriptorTest
```

预期：通过。

## 任务 4：实现上下文冻结

- [ ] **步骤 1：编写上下文快照测试**

创建 `Materal.MergeBlock.AI.Test/Context/AIContextSnapshotTest.cs`，验证快照不受源字典后续修改影响，并且 `Items` 为只读视图。

- [ ] **步骤 2：实现上下文契约**

创建 `IReadOnlyAIContext.cs`、`AIContextSnapshot.cs`、`AIContextBuilderContext.cs` 和 `IAIContextProvider.cs`。

关键实现要求：

- `AIContextSnapshot` 构造时复制内部字典。
- `GetRequired<T>` 在缺少项目时抛出 `KeyNotFoundException`。
- `AIContextBuilderContext.Freeze()` 返回只读快照。

- [ ] **步骤 3：运行上下文测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIContextSnapshotTest
```

预期：通过。

## 任务 5：实现提示词贡献

- [ ] **步骤 1：编写提示词构建器测试**

创建 `Materal.MergeBlock.AI.Test/Prompts/AIPromptBuilderTest.cs`，验证贡献器只能读取冻结上下文并追加系统消息。

- [ ] **步骤 2：实现提示词契约**

创建 `IAIPromptContributor.cs`、`AIPromptContributionContext.cs` 和 `AIPromptBuilder.cs`。

关键要求：

- `AIPromptContributionContext` 暴露 `IReadOnlyAIContext`。
- 贡献器只能通过 `AddSystemMessage` 追加提示词。
- 空白提示词不应加入结果。
- `AIPromptBuilder` 按注册顺序执行贡献器。

- [ ] **步骤 3：运行提示词测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIPromptBuilderTest
```

预期：通过。

## 任务 6：实现工具扫描器和注册表

- [ ] **步骤 1：编写扫描器测试**

创建 `Materal.MergeBlock.AI.Test/Tools/AIToolScannerTest.cs`，验证扫描器能发现本地和远程工具，并能读取 `MergeBlockAIToolAttribute` 上的名称和执行模式。

- [ ] **步骤 2：实现扫描器**

创建 `AIToolScanner.cs` 和 `AIToolRegistry.cs`。

核心要求：

- 扫描指定程序集中的类型级 `MergeBlockAIToolAttribute`。
- 未显式指定工具名称时使用类型名。
- `AIToolRegistry.Register` 拒绝空白工具名称。
- `AIToolRegistry.GetRequired` 找不到工具时抛出 `KeyNotFoundException`。

- [ ] **步骤 3：运行扫描器测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIToolScannerTest
```

预期：通过。

## 任务 7：实现审计

- [ ] **步骤 1：编写审计器测试**

创建 `Materal.MergeBlock.AI.Test/Auditing/LoggingAIToolCallAuditorTest.cs`，验证默认日志审计器可以接受远程工具上下文。

- [ ] **步骤 2：实现审计契约**

创建 `AIToolCallStatus.cs`、`AIToolCallAuditContext.cs`、`IAIToolCallAuditor.cs` 和 `LoggingAIToolCallAuditor.cs`。

状态常量：

```csharp
public const string Requested = "requested";
public const string Started = "started";
public const string Completed = "completed";
public const string Failed = "failed";
public const string Rejected = "rejected";
public const string Cancelled = "cancelled";
```

默认实现应记录工具名称、执行模式、thread、run 和状态。

- [ ] **步骤 3：运行审计器测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter LoggingAIToolCallAuditorTest
```

预期：通过。

## 任务 8：实现 AIModule 和 DI

- [ ] **步骤 1：编写 DI 注册测试**

创建 `Materal.MergeBlock.AI.Test/AIModuleTest.cs`，验证 `AddMergeBlockAI()` 注册 `AIToolRegistry`、`AIPromptBuilder` 和默认 `IAIToolCallAuditor`。

- [ ] **步骤 2：实现 DI 扩展**

创建 `Extensions/DIExtensions.cs`：

```csharp
namespace Materal.MergeBlock.AI.Extensions;

/// <summary>
/// 依赖注入扩展
/// </summary>
public static class DIExtensions
{
    /// <summary>
    /// 添加MergeBlock AI
    /// </summary>
    public static IServiceCollection AddMergeBlockAI(this IServiceCollection services)
    {
        services.AddSingleton<AIToolRegistry>();
        services.AddSingleton<AIToolScanner>();
        services.AddSingleton<AIPromptBuilder>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAIToolCallAuditor, LoggingAIToolCallAuditor>());
        return services;
    }
}
```

创建 `AIModule.cs`：

```csharp
namespace Materal.MergeBlock.AI;

/// <summary>
/// AI模块
/// </summary>
public class AIModule() : MergeBlockModule("AI模块")
{
    /// <inheritdoc />
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        if (context.Configuration is not null)
        {
            context.Services.Configure<AIOptions>(context.Configuration.GetSection(AIOptions.ConfigKey));
        }
        context.Services.AddMergeBlockAI();
        base.OnConfigureServices(context);
    }
}
```

- [ ] **步骤 3：运行核心测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj
```

预期：通过。

## 任务 9：最终核心验证

- [ ] **步骤 1：确认 CRLF**

运行：

```powershell
$paths = @(
  '.\Materal.MergeBlock\Materal.MergeBlock.AI.Abstractions',
  '.\Materal.MergeBlock\Materal.MergeBlock.AI',
  '.\Materal.MergeBlock\Materal.MergeBlock.AI.Test'
)
foreach ($path in $paths) {
  Get-ChildItem -LiteralPath $path -Recurse -File | Where-Object Extension -in '.cs','.csproj' | ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    $badLf = 0
    for ($i = 0; $i -lt $bytes.Length; $i++) {
      if ($bytes[$i] -eq 10 -and ($i -eq 0 -or $bytes[$i - 1] -ne 13)) { $badLf++ }
    }
    if ($badLf -gt 0) { throw "发现 LF 行尾: $($_.FullName)" }
  }
}
```

预期：无异常。

- [ ] **步骤 2：构建和测试**

运行：

```powershell
dotnet build .\Materal.slnx
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj
```

预期：两个命令都通过。

- [ ] **步骤 3：验证 MMB 运行时项目仍可构建**

运行：

```powershell
dotnet build .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.slnx
```

预期：`MMB.Demo` 在添加 AI 核心项目后仍可构建。这确认现有运行时测试项目没有被包、解决方案或模块改动破坏。

- [ ] **步骤 4：任何提交前运行 GitNexus change detection**

为 MergeBlock 仓库运行 GitNexus change detection，并确认受影响符号符合新增 AI 项目与解决方案/包元数据的范围。

- [ ] **步骤 5：用户确认后的提交命令**

仅在用户明确要求提交时运行：

```powershell
git add .\Directory.Packages.props .\Materal.slnx .\Materal.Packable.slnx .\Materal.MergeBlock\Materal.MergeBlock.AI.Abstractions .\Materal.MergeBlock\Materal.MergeBlock.AI .\Materal.MergeBlock\Materal.MergeBlock.AI.Test
git commit -m "feat: 添加AI核心抽象与模块"
```
