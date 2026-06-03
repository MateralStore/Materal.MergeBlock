# Materal.MergeBlock.AI.Web 实施计划

> **面向代理执行者：** 必需子技能：按任务逐项实施本计划时，请使用 `superpowers:subagent-driven-development`（推荐）或 `superpowers:executing-plans`。步骤使用复选框（`- [ ]`）语法便于跟踪。

**目标：** 构建 Web Agent Host 层，提供 SSE 对话、Remote Tool Gateway、run 暂停/恢复/取消、持久化调试追踪，以及远程客户端工具结果恢复能力。

**架构：** `Materal.MergeBlock.AI.Web` 依赖 `Materal.MergeBlock.AI` 和 `Materal.MergeBlock.Web`。它负责 HTTP/SSE 契约和 run 持久化；核心工具描述符和审计器继续保留在核心 AI 项目中。

**技术栈：** ASP.NET Core 控制器、`IAsyncEnumerable` 或流式响应体写入、`System.Text.Json`、`Microsoft.Data.Sqlite`、MergeBlock Web 模块、MSTest。

---

## 文件

- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Materal.MergeBlock.AI.Web.csproj`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/GlobalUsings.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/AIWebModule.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Controllers/AIAgentController.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentStreamEvent.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentChatRequest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/RemoteToolResultsRequest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/CancelAgentRunRequest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentRunStatus.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/RemoteTools/RemoteToolGateway.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/RemoteTools/RemoteToolPendingCall.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/IAIAgentStateStore.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/SqliteAIAgentStateStore.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Streaming/SseEventWriter.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Cancellation/AIAgentCancellationRegistry.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Materal.MergeBlock.AI.Web.Test.csproj`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/GlobalUsings.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/RemoteTools/RemoteToolGatewayTest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Persistence/SqliteAIAgentStateStoreTest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Streaming/SseEventWriterTest.cs`
- 修改：`Directory.Packages.props`
- 修改：`Materal.slnx`
- 修改：`Materal.Packable.slnx`

## 任务 1：创建 AI.Web 项目

- [ ] **步骤 1：确认阶段 1 已通过**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj
```

预期：通过。

- [ ] **步骤 2：创建项目文件**

创建 `Materal.MergeBlock.AI.Web.csproj`：

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="../../MergeBlockLibrary.props" />
	<PropertyGroup>
		<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
		<Title>Materal MergeBlock AI Web模块</Title>
		<Description>Materal MergeBlock AI Web模块</Description>
	</PropertyGroup>
	<ItemGroup>
		<FrameworkReference Include="Microsoft.AspNetCore.App" />
	</ItemGroup>
	<ItemGroup>
		<PackageReference Include="Microsoft.Data.Sqlite" />
	</ItemGroup>
	<ItemGroup>
		<ProjectReference Include="..\Materal.MergeBlock.AI\Materal.MergeBlock.AI.csproj" />
		<ProjectReference Include="..\Materal.MergeBlock.Web\Materal.MergeBlock.Web.csproj" />
	</ItemGroup>
</Project>
```

- [ ] **步骤 3：添加测试项目**

创建 `Materal.MergeBlock.AI.Web.Test.csproj`：

```xml
<Project Sdk="MSTest.Sdk/4.1.0">
	<Import Project="../../Common.props" />
	<PropertyGroup>
		<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
		<IsTestProject>true</IsTestProject>
	</PropertyGroup>
	<ItemGroup>
		<ProjectReference Include="..\Materal.MergeBlock.AI.Web\Materal.MergeBlock.AI.Web.csproj" />
	</ItemGroup>
</Project>
```

- [ ] **步骤 4：添加 global using 文件**

按核心计划风格创建 `Materal.MergeBlock.AI.Web/GlobalUsings.cs` 和 `Materal.MergeBlock.AI.Web.Test/GlobalUsings.cs`，引用 AI 抽象、Web 模型、持久化、远程工具、SSE 写入器、ASP.NET Core、SQLite 和 MSTest 相关命名空间。

- [ ] **步骤 5：添加项目到解决方案**

添加：

```xml
<Project Path="Materal.MergeBlock/Materal.MergeBlock.AI.Web/Materal.MergeBlock.AI.Web.csproj" />
<Project Path="Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Materal.MergeBlock.AI.Web.Test.csproj" />
```

## 任务 2：实现流式事件契约

- [ ] **步骤 1：编写流式事件测试**

创建 `SseEventWriterTest.cs`，验证 `SseEventWriter.Format` 输出 `event: tool_call.requested`、包含 `"schema_version":"agent-stream-v1"`，并以空行结束。

- [ ] **步骤 2：实现流式模型**

创建 `AgentStreamEvent.cs`：

```csharp
namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent流式事件
/// </summary>
public class AgentStreamEvent
{
    public string SchemaVersion { get; init; } = "agent-stream-v1";
    public string ThreadId { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public int Seq { get; init; }
    public string Event { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?> Payload { get; init; } = new Dictionary<string, object?>();
}
```

创建 `SseEventWriter.cs`，使用 `JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower`，格式为：

```text
event: {event}
data: {json}

```

- [ ] **步骤 3：运行流式测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter SseEventWriterTest
```

预期：通过。

## 任务 3：实现状态存储

- [ ] **步骤 1：编写状态存储测试**

创建 `SqliteAIAgentStateStoreTest.cs`，验证 store 可以持久化 session、run、stream event 和远程工具调用，并能通过 `GetRunTraceAsync` 读取 run、事件和工具调用记录。

- [ ] **步骤 2：实现持久化接口**

创建 `IAIAgentStateStore.cs`：

```csharp
namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// AI Agent状态存储
/// </summary>
public interface IAIAgentStateStore
{
    Task InitializeAsync();
    Task UpsertSessionAsync(string threadId);
    Task StartRunAsync(string runId, string threadId);
    Task CompleteRunAsync(string runId, string status, string? errorMessage = null);
    Task RecordStreamEventAsync(AgentStreamEvent streamEvent);
    Task RecordToolCallAsync(RemoteToolPendingCall toolCall);
    Task<AgentRunTrace> GetRunTraceAsync(string runId);
}
```

同时创建 `AgentRunTrace` 和 `RemoteToolPendingCall` 模型，包含 run、events、tool calls、`ToolCallId`、`ThreadId`、`RunId`、`ToolName`、`Status`、`Arguments` 等属性。

- [ ] **步骤 3：实现 SQLite 表结构**

创建 `SqliteAIAgentStateStore.cs`，包含以下表：

```sql
create table if not exists ai_agent_sessions (
  thread_id text primary key,
  created_at text not null,
  updated_at text not null
);
create table if not exists ai_agent_runs (
  run_id text primary key,
  thread_id text not null,
  status text not null,
  started_at text not null,
  completed_at text,
  error_message text
);
create table if not exists ai_agent_stream_events (
  id integer primary key autoincrement,
  thread_id text not null,
  run_id text not null,
  seq integer not null,
  event_type text not null,
  payload_json text not null,
  created_at text not null
);
create table if not exists ai_agent_tool_calls (
  id text primary key,
  thread_id text not null,
  run_id text not null,
  tool_name text not null,
  status text not null,
  arguments_json text,
  result_json text,
  error_json text,
  created_at text not null,
  completed_at text
);
```

时间戳使用 `DateTimeOffset.UtcNow.ToString("O")`，payload 使用 `System.Text.Json` 序列化。

- [ ] **步骤 4：运行持久化测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter SqliteAIAgentStateStoreTest
```

预期：通过。

## 任务 4：实现 Remote Tool Gateway

- [ ] **步骤 1：编写 gateway 恢复校验测试**

创建 `RemoteToolGatewayTest.cs`，验证提交的 `ToolCallId` 与当前 run 中待处理工具调用不完全匹配时，`ValidateResumeAsync` 抛出 `InvalidOperationException`。

- [ ] **步骤 2：实现请求模型**

创建 `RemoteToolResultsRequest.cs`：

```csharp
namespace Materal.MergeBlock.AI.Web.Models;

public class RemoteToolResultsRequest
{
    public string SchemaVersion { get; init; } = "remote-tool-results-v1";
    public string ThreadId { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public IReadOnlyList<RemoteToolResultItem> ToolResults { get; init; } = [];
}

public class RemoteToolResultItem
{
    public string ToolCallId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?>? Result { get; init; }
    public IReadOnlyDictionary<string, object?>? Error { get; init; }
}
```

- [ ] **步骤 3：实现 gateway 校验**

创建 `RemoteToolGateway.cs`，实现以下校验：

- run 必须存在。
- `thread_id` 必须与 run 匹配。
- 请求中的工具结果 ID 必须与当前 run 中状态为 `requested` 的工具调用 ID 完全一致。
- 不允许缺失、额外或串 run 的工具结果。

不匹配时抛出清晰的 `InvalidOperationException`。

- [ ] **步骤 4：运行 gateway 测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter RemoteToolGatewayTest
```

预期：通过。

## 任务 5：实现控制器契约

- [ ] **步骤 1：添加请求模型**

创建 `AgentChatRequest.cs`：

```csharp
namespace Materal.MergeBlock.AI.Web.Models;

public class AgentChatRequest
{
    public string SchemaVersion { get; init; } = "agent-chat-request-v1";
    public string ThreadId { get; init; } = string.Empty;
    public string? RunId { get; init; }
    public string Message { get; init; } = string.Empty;
}
```

创建 `CancelAgentRunRequest.cs`：

```csharp
namespace Materal.MergeBlock.AI.Web.Models;

public class CancelAgentRunRequest
{
    public string ThreadId { get; init; } = string.Empty;
    public string Reason { get; init; } = "user_requested";
    public string Source { get; init; } = "agent_chat_ui";
}
```

- [ ] **步骤 2：实现控制器骨架**

创建 `AIAgentController.cs`，提供：

- `POST /agent/chat/stream`：设置 `text/event-stream`，创建或使用传入 run，记录 session/run，写出 `run.started` 事件。
- `POST /agent/chat/resume/stream`：先调用 `RemoteToolGateway.ValidateResumeAsync`，校验通过后以 SSE 写出恢复事件。

该骨架故意保持最小化。等 Remote Tool Gateway 测试证明暂停/恢复校验后，再补充 MAF run 编排。

- [ ] **步骤 3：实现 AIWebModule**

创建 `AIWebModule.cs`：

```csharp
namespace Materal.MergeBlock.AI.Web;

[DependsOn(typeof(AIModule), typeof(WebModule))]
public class AIWebModule() : MergeBlockModule("AI Web模块")
{
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<RemoteToolGateway>();
        context.Services.AddSingleton<IAIAgentStateStore>(_ => new SqliteAIAgentStateStore("data/ai-agent.sqlite3"));
        base.OnConfigureServices(context);
    }
}
```

- [ ] **步骤 4：构建 AI.Web**

运行：

```powershell
dotnet build .\Materal.MergeBlock\Materal.MergeBlock.AI.Web\Materal.MergeBlock.AI.Web.csproj
```

预期：构建成功。

## 任务 6：最终 AI.Web 验证

- [ ] **步骤 1：运行 AI.Web 测试**

运行：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj
```

预期：通过。

- [ ] **步骤 2：将 AI.Web 运行时验证加入 MMB.Demo**

使用现有运行时测试项目，不要创建新的 demo host。修改 `Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.Application\DemoModule.cs` 或合适的 demo 模块，使其依赖 `AIWebModule`：

```csharp
[DependsOn(typeof(AIWebModule))]
public class DemoModule() : MergeBlockModule("Demo模块")
{
}
```

如果现有 demo 模块已有 `DependsOn`，请把 `typeof(AIWebModule)` 加入现有依赖列表，不要替换当前依赖。

- [ ] **步骤 3：向 MMB.Demo.WebAPI 添加最小 AI 配置**

修改 `Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.WebAPI\appsettings.json`，合并以下最小 AI 配置并保留现有值：

```json
{
  "MergeBlock": {
    "AI": {
      "Enable": true,
      "DefaultAgentName": "default",
      "ScanTools": true,
      "RequireToolAuthorization": true,
      "AuditToolCalls": true
    }
  }
}
```

- [ ] **步骤 4：构建 MMB.Demo 运行时宿主**

运行：

```powershell
dotnet build .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.slnx
```

预期：通过。

- [ ] **步骤 5：运行 MMB.Demo.WebAPI 验证路由**

运行：

```powershell
dotnet run --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.WebAPI\MMB.Demo.WebAPI.csproj
```

预期：WebAPI 启动并映射 `Materal.MergeBlock.AI.Web` 的 AI 路由。确认启动和路由注册后停止进程。

- [ ] **步骤 6：构建解决方案**

运行：

```powershell
dotnet build .\Materal.slnx
```

预期：通过。

- [ ] **步骤 7：任何提交前运行 GitNexus change detection**

运行 GitNexus change detection，确认影响范围限定在 AI core、AI.Web、测试、包元数据和解决方案文件。

- [ ] **步骤 8：用户确认后的提交命令**

仅在用户明确要求提交时运行：

```powershell
git add .\Directory.Packages.props .\Materal.slnx .\Materal.Packable.slnx .\Materal.MergeBlock\Materal.MergeBlock.AI.Web .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test .\Materal.MergeBlock\MMB\MMB.Demo
git commit -m "feat: 添加AI远程工具主机"
```
