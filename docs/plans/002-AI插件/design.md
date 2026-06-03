# Materal.MergeBlock.AI 设计草案

## 背景

Materal.MergeBlock 已经提供模块加载、依赖注入、配置、日志、授权、Swagger、事件总线等横切能力。多个基于 MergeBlock 开发的业务程序需要接入 AI 能力，包括配置 Agent、声明业务工具、注入提示词和业务上下文、记录工具调用审计。

Microsoft Agent Framework 已经提供 Agent、Provider、Tool Calling、Workflow、Session、MCP、上下文、中间件和遥测等核心能力。因此 MergeBlock 不应重复实现 AI 框架，而应提供面向 MergeBlock 业务应用的集成层。

## 目标

新增 `Materal.MergeBlock.AI` 模块，作为 Microsoft Agent Framework 在 MergeBlock 中的适配层。

模块目标：

- 将 MAF 接入 MergeBlock 的模块生命周期和依赖注入体系。
- 提供 `MergeBlock:AI` 配置约定。
- 支持扫描 MergeBlock 应用程序集中的业务 AI 工具并注册到 MAF。
- 支持提示词贡献器，将系统提示词、业务提示词、模块提示词聚合到 Agent 上下文。
- 支持上下文贡献器，将应用、用户、租户、权限、模块等 MergeBlock 上下文注入 AI 调用。
- 支持工具调用审计、日志和异常处理。
- 支持前端远程工具调用场景，允许 Agent 请求客户端执行工具并在结果返回后继续运行。
- 支持流式事件输出、运行暂停/恢复、取消、运行状态持久化和调试追踪。
- 保持 Provider、Agent 执行、Workflow、Session、MCP 等底层能力完全由 MAF 负责。

## 非目标

本模块不做以下事情：

- 不实现 OpenAI、Azure OpenAI、Ollama、Anthropic 等 Provider 包。
- 不定义自有 `ILLMClient` 或自有 Provider 抽象。
- 不重写 Agent、Tool Calling、Workflow、Session、MCP。
- 不封装一套独立于 MAF 的 AI SDK。
- 一期不实现长期记忆、RAG、多 Agent 编排、AI 管理后台、Prompt 可视化编辑器。

## 项目结构

建议新增三个项目：

```text
Materal.MergeBlock.AI.Abstractions
Materal.MergeBlock.AI
Materal.MergeBlock.AI.Web
```

`Materal.MergeBlock.AI.Abstractions` 放稳定暴露给业务模块使用的抽象和元数据类型：

- `AIOptions`
- `AIAgentOptions`
- `MergeBlockAIToolAttribute`
- `AIToolExecutionMode`
- `AIToolDescriptor`
- `IAIToolMetadataProvider`
- `IAIPromptContributor`
- `IAIContextProvider`
- `IAIToolCallAuditor`
- 工具、提示词、上下文、审计相关模型

`Materal.MergeBlock.AI` 放具体集成实现：

- `AIModule : MergeBlockModule`
- `DIExtensions`
- 配置读取与校验
- MAF 服务注册适配
- MergeBlock assemblies 工具扫描
- Prompt/context 聚合器
- 默认审计与日志实现
- 异常转换与记录

`Materal.MergeBlock.AI.Web` 放面向 Web/API 的 Agent Host 能力：

- Agent 流式对话 API
- Remote Tool Gateway
- SSE 事件契约
- Run/Thread/Message/ToolCall 持久化抽象
- 暂停、恢复、取消接口
- 调试追踪查询接口
- 面向前端远程工具的结果回传接口

`Materal.MergeBlock.AI.Web` 依赖 `Materal.MergeBlock.Web` 和 `Materal.MergeBlock.AI`，但不包含具体业务工具。客户端工具和业务工具应在业务模块中声明。

## 运行时验证项目

`Materal.MergeBlock\MMB` 是专门用于验证 MMB 框架实际运行行为的项目集合。AI 插件实现后，单元测试用于验证抽象和纯逻辑，真实运行时集成验证应优先使用 `Materal.MergeBlock\MMB` 下的现有项目。

建议使用：

- `Materal.MergeBlock\MMB\MMB.Core`：验证核心模块依赖、模块加载和基础业务模块组合。
- `Materal.MergeBlock\MMB\MMB.Demo`：验证 WebAPI 宿主、模块初始化、配置读取、SSE 接口、远程工具暂停/恢复等运行时行为。

不要为了验证 AI 插件另建无关 Demo 项目。若需要增加运行时验证，应优先在 `MMB.Demo` 中增加 AI 插件配置、测试工具和最小 API 调用路径。

## 配置约定

配置根节点使用 `MergeBlock:AI`。配置只描述 MergeBlock 关心的集成行为，不承载 Provider 专有细节。

示例：

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

Provider 相关配置由业务项目按 MAF 官方文档处理。例如业务项目自行引用 MAF Provider 包，并在应用启动或配置中注册 Azure OpenAI、OpenAI、Ollama 等 Provider。

## 工具声明

业务模块通过 Attribute 或元数据提供器声明 AI 工具。Attribute 适合常见场景，元数据提供器适合动态工具或需要复杂授权规则的场景。

工具声明只描述工具契约和执行方式，不承担审计职责。审计由 `IAIToolCallAuditor` 统一处理。

工具元数据必须明确执行模式：

```csharp
public enum AIToolExecutionMode
{
    Local,
    Remote
}

public sealed class AIToolDescriptor
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public AIToolExecutionMode ExecutionMode { get; init; }
    public string? RequiredPermission { get; init; }
    public Type? InputType { get; init; }
    public Type? ResultType { get; init; }
}
```

示例：

```csharp
[MergeBlockAITool("查询订单信息", ExecutionMode = AIToolExecutionMode.Local)]
public class OrderAITool
{
    public Task<OrderDTO> GetOrderAsync(Guid id)
    {
        // 调用业务服务或仓储
    }
}
```

远程工具可以只声明契约，不在服务端实现实际执行逻辑：

```csharp
[MergeBlockAITool("执行客户端操作", ExecutionMode = AIToolExecutionMode.Remote)]
public sealed class RunClientActionTool
{
}
```

工具注册流程：

```text
MergeBlock 加载模块
  -> AIModule 获取 MergeBlockContext.MergeBlockAssemblies
  -> 扫描 AI 工具类型和方法
  -> 读取工具元数据、描述、授权要求
  -> 注册到 MAF
```

## 远程工具网关

除服务端直接执行的业务工具外，模块还需要支持前端远程工具。远程工具指 Agent 需要调用的工具不在服务端进程中执行，而是在浏览器、桌面客户端、插件宿主或其他前端宿主中执行。

典型流程：

```text
Agent 请求调用工具
  -> MMB.AI 校验工具名称、参数、权限
  -> MMB.AI.Web 通过 SSE 向前端发送 tool_call.requested
  -> Agent 运行进入 paused 状态
  -> 前端执行工具并提交 tool result
  -> MMB.AI.Web 校验 run、thread、tool_call_id 和结果状态
  -> MMB.AI 将工具结果交还给 MAF
  -> Agent 继续运行
```

远程工具网关应提供：

- 远程工具契约注册，包括名称、描述、输入结构、结果结构、权限级别。
- 工具参数校验。
- 工具调用权限校验。
- 待执行工具调用记录。
- 工具结果回传校验。
- 超时、取消和错误结果处理。

远程工具和服务端工具都属于 AI 工具，但执行方式不同。设计上应明确区分：

```text
服务端工具：Agent -> 服务端执行 -> 返回结果
远程工具：Agent -> 服务端请求前端执行 -> 前端返回结果 -> Agent 继续
```

例如浏览器、桌面客户端或插件宿主中的文件读取、内容编辑、界面操作等能力，都应建模为远程工具，而不是服务端工具。

运行时按 `AIToolDescriptor.ExecutionMode` 分流：

```text
Local
  -> 服务端直接执行工具
  -> 记录审计
  -> 把结果交给 Agent

Remote
  -> 记录 pending tool call
  -> 通过 SSE 发 tool_call.requested
  -> run 进入 paused
  -> 前端回传结果
  -> 校验并记录审计
  -> 把结果交给 Agent
```

## SSE 事件契约

`Materal.MergeBlock.AI.Web` 应提供稳定的流式事件契约，用于 Agent 服务端和前端客户端通信。事件采用 Server-Sent Events 输出，事件数据使用结构化 JSON。

建议基础事件：

- `run.started`
- `message.delta`
- `thinking.delta`
- `tool_call.delta`
- `tool_call.requested`
- `tool_result.completed`
- `run.paused`
- `run.cancelled`
- `run.completed`
- `error`

事件基础字段：

```json
{
  "schema_version": "agent-stream-v1",
  "thread_id": "thread_xxx",
  "run_id": "run_xxx",
  "seq": 1,
  "event": "tool_call.requested",
  "payload": {}
}
```

`seq` 在同一个 run 内递增，用于前端排序、调试追踪和断线后的事件核对。事件契约应保持向后兼容，新增字段优先放入 `payload`。

远程工具请求事件示例：

```json
{
  "schema_version": "agent-stream-v1",
  "thread_id": "thread_001",
  "run_id": "run_001",
  "seq": 12,
  "event": "tool_call.requested",
  "payload": {
    "tool_call_id": "tool_001",
    "name": "runClientAction",
    "arguments": {
      "action": "..."
    }
  }
}
```

前端收到远程工具请求后，通过恢复接口提交工具结果。恢复接口必须校验回传的 `tool_call_id` 是否正好匹配当前 run 中待处理的工具调用，避免串 run、串会话或重复提交。

## 运行状态持久化

Agent 运行过程不能只存在内存中。为支持暂停恢复、取消、调试和审计，模块应提供运行状态持久化抽象。

建议持久化对象：

- Session：一次用户会话或线程。
- Run：一次 Agent 执行。
- Message：用户、助手、工具消息。
- StreamEvent：流式输出事件。
- ToolCall：工具调用记录。
- Checkpoint：Agent 暂停点或恢复所需状态。

持久化能力应支持：

- 开始 run。
- 完成 run。
- 记录消息。
- 记录流式事件。
- 记录工具调用开始、请求、完成、失败、拒绝。
- 查询 session。
- 查询 run。
- 查询 debug trace。
- 根据 run 恢复待处理工具调用。

默认实现可以先使用轻量存储满足本地开发和桌面插件场景；抽象层应允许业务替换为数据库、事件总线或分布式存储。

运行状态持久化也是审计能力的一部分。特别是远程工具可能修改用户文档或业务数据，必须能追踪调用者、工具名称、参数摘要、执行结果和错误信息。

## 上下文注入

通过 `IAIContextProvider` 将 MergeBlock 业务上下文注入 AI 调用。上下文应以结构化形式传递，避免只拼接自然语言。

常见上下文：

- 应用名称
- 模块名称
- 当前用户 ID
- 当前租户 ID
- 当前权限或角色
- 当前请求链路标识
- 当前语言和区域设置

建议接口语义：

```csharp
public interface IAIContextProvider
{
    Task ProvideAsync(AIContextBuilderContext context);
}
```

`AIContextBuilderContext` 是可写上下文，只允许在上下文构建阶段使用。所有 `IAIContextProvider` 执行完毕后，框架必须将上下文冻结为只读快照。

建议流程：

```text
创建 AIContextBuilderContext
  -> 执行 IAIContextProvider 写入结构化上下文
  -> 冻结为 IReadOnlyAIContext
  -> 后续提示词、工具授权、审计都读取只读快照
```

冻结时应复制内部字典，避免贡献器持有原始引用后继续修改。

## 提示词注入

通过 `IAIPromptContributor` 扩展提示词。每个贡献器负责一个明确来源，例如系统约束、业务模块说明、当前用户权限说明、工具使用规则。

建议接口语义：

```csharp
public interface IAIPromptContributor
{
    Task ContributeAsync(AIPromptContributionContext context);
}
```

`AIPromptContributionContext` 只能追加提示词，并读取冻结后的 `IReadOnlyAIContext`。它不能修改 AI 上下文，也不能变更工具权限、用户、租户等结构化数据。

建议接口语义：

```csharp
public sealed class AIPromptContributionContext
{
    public IReadOnlyAIContext AIContext { get; init; } = default!;

    public void AddSystemMessage(string message)
    {
        // 追加系统提示词
    }
}
```

聚合顺序建议：

1. 框架级系统约束
2. 应用级提示词
3. 模块级提示词
4. 当前用户、租户、权限相关提示词
5. 当前请求临时提示词

## 工具调用审计

工具调用是业务风险最高的部分。模块应提供 `IAIToolCallAuditor`，记录工具调用前后的关键信息。

`IAIToolCallAuditor` 是审计扩展点，不是业务工具接口。业务工具不应通过实现 `IAIToolCallAuditor` 来声明自己。工具类型和执行方式由 `AIToolDescriptor` 表达，审计器只消费工具调用上下文。

建议接口语义：

```csharp
public interface IAIToolCallAuditor
{
    Task AuditAsync(AIToolCallAuditContext context);
}

public sealed class AIToolCallAuditContext
{
    public string ToolName { get; init; } = string.Empty;
    public AIToolExecutionMode ExecutionMode { get; init; }
    public string ThreadId { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
```

审计内容建议：

- Agent 名称
- 工具名称
- 调用参数摘要
- 当前用户、租户、模块
- 调用开始和结束时间
- 调用结果状态
- 异常信息
- 是否经过授权
- 是否为远程工具调用
- 远程工具调用的暂停、恢复和结果回传状态

默认实现可以使用现有日志系统记录。后续如果业务需要，可以替换为数据库、事件总线或专门审计服务。

## 安全边界

一期至少应保留以下安全边界：

- 默认不允许未标记的业务服务自动暴露为 AI 工具。
- 工具元数据中应可声明授权要求。
- 开启 `RequireToolAuthorization` 时，工具调用前必须经过授权校验。
- 审计开启时，所有工具调用都必须记录成功或失败结果。
- 工具参数日志应支持脱敏，避免泄漏密钥、Token、身份证号、手机号等敏感信息。
- 远程工具结果必须校验 `run_id`、`thread_id`、`tool_call_id` 和待处理工具列表。
- 远程工具默认不能跨会话恢复。
- 修改型远程工具应支持更严格的权限或审批策略。

## 一期范围

一期范围需要满足基础 MAF 适配、通用 Web Agent Host 和远程工具流程：

- 新增 `Materal.MergeBlock.AI.Abstractions`。
- 新增 `Materal.MergeBlock.AI`。
- 新增 `Materal.MergeBlock.AI.Web`。
- 实现 `AIModule`。
- 实现 `MergeBlock:AI` 配置读取。
- 实现工具扫描与 MAF 注册适配。
- 实现远程工具契约注册。
- 实现 Remote Tool Gateway。
- 实现 Agent SSE 事件输出。
- 实现 Agent run 暂停、恢复、取消接口。
- 实现运行状态持久化抽象和默认实现。
- 实现 debug trace 查询接口。
- 实现提示词贡献器聚合。
- 实现上下文贡献器聚合。
- 实现工具调用审计接口与默认日志实现。
- 提供一个最小 Demo，验证服务端工具和前端远程工具都能通过 MAF Agent 调用。

## 后续扩展

后续可按业务需求独立扩展：

- RAG 集成。
- 长期记忆。
- 多 Agent 编排。
- MCP Server 管理。
- Prompt 版本管理。
- AI 调用指标面板。
- 工具调用审批流。
- 管理后台。
- 业务专用工具包。
- 业务专用合规审查器的具体规则实现。

这些能力不应进入一期，以免模糊 MMB.AI 的核心边界。

## 验证策略

一期验证重点：

- 单元测试配置绑定和默认值。
- 单元测试工具扫描规则。
- 单元测试提示词贡献器排序和聚合。
- 单元测试上下文贡献器聚合。
- 单元测试审计接口在成功和异常路径均被调用。
- 单元测试远程工具契约注册和参数校验。
- 单元测试远程工具恢复时 `run_id`、`thread_id`、`tool_call_id` 匹配校验。
- 单元测试 run 暂停、恢复、取消状态流转。
- 单元测试 SSE 事件序号递增和事件格式。
- 集成测试验证服务端工具可以注册到 MAF 并被 Agent 调用。
- 集成测试验证远程工具请求发出、run 暂停、工具结果回传、run 继续执行。
- 运行时验证使用 `Materal.MergeBlock\MMB\MMB.Demo`，确认 AI 模块在真实 MergeBlock WebAPI 宿主中可以完成配置读取、模块初始化、路由注册和远程工具流程。

如果改动公共抽象或模块加载行为，按仓库规则在实现前使用 GitNexus 做影响分析，提交前使用 GitNexus change detection 检查影响范围。
