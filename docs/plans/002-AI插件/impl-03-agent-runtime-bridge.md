# Materal.MergeBlock.AI Agent Runtime Bridge 实施计划

> **面向代理执行者：** 必需子技能：按任务逐项实施本计划时，请使用 `superpowers:subagent-driven-development`（推荐）或 `superpowers:executing-plans`。步骤使用复选框（`- [ ]`）语法便于跟踪。

**目标：** 构建通用 Agent Runtime Bridge，让 `Materal.MergeBlock.AI.Web` 可以调用业务模块注册的 Agent 运行时，并将运行过程统一转换为 SSE 事件、状态持久化记录和远程工具暂停/恢复流程。

**架构：** MMB.AI 只提供运行时抽象、事件适配、上下文/提示词/工具元数据聚合，以及 Web Host 对这些抽象的调用。具体 Agent、Provider、模型、业务工具和业务授权由业务模块通过 DI 注册。

**技术栈：** C#/.NET `net8.0;net9.0;net10.0`、MergeBlock 模块、ASP.NET Core 控制器/SSE、MSTest、Microsoft Agent Framework 业务侧适配。

---

## 范围

阶段 3 以框架级通用桥接为主体，不在 `Materal.MergeBlock.AI` 或 `Materal.MergeBlock.AI.Web` 中内置任何具体业务 Agent 或 Provider。收尾阶段需要在 `MMB.Demo` 中提供一个基于 Microsoft Agent Framework Provider 的可直接运行示例，通过 GLM5.1 参数验证宿主接入链路。

包含：

- Agent 运行时抽象。
- Agent 运行请求、恢复请求和运行输出模型。
- `AIAgentController` 调用业务运行时。
- Runtime 输出到 `AgentStreamEvent` 的映射。
- 远程工具请求持久化和 SSE 输出。
- resume 校验后将工具结果交回业务运行时。
- 默认未注册运行时的清晰错误。
- MMB.Demo 中的 MAF Agent 运行时示例，用 GLM5.1 Provider 参数验证真实 MergeBlock 宿主行为。

不包含：

- 框架项目中的任何 Provider 包。
- 框架项目中的任何具体业务 Agent。
- 任何具体客户端、插件、文档、表格或桌面应用逻辑。
- RAG、长期记忆、多 Agent 编排、管理后台。

## 文件

- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRunRequest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentResumeRequest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRunOutput.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRunOutputType.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/IAIAgentRuntime.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentStreamAdapter.cs`
- 修改：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Controllers/AIAgentController.cs`
- 修改：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/AIWebModule.cs`
- 修改：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/IAIAgentStateStore.cs`
- 修改：`Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/SqliteAIAgentStateStore.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentStreamAdapterTest.cs`
- 新建：`Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentControllerRuntimeTest.cs`
- 修改：`Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application`
- 新建：`Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/Glm51AIAgentRuntime.cs`
- 新建：`Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/Glm51AgentOptions.cs`
- 新建：`Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/Glm51AIOptions.cs`
- 本地配置：`Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.WebAPI/appsettings.Development.json`，仅用于本机密钥和 MAF Provider 参数配置，不提交。

## 任务 1：定义运行时抽象

- [ ] **步骤 1：影响分析**

修改 `AIAgentController`、`AIWebModule`、`IAIAgentStateStore` 前，运行 GitNexus impact analysis。

- [ ] **步骤 2：编写运行时抽象测试**

新增测试，验证业务运行时可以返回多种输出：

- 文本增量。
- reasoning/thinking 增量。
- tool call 参数流式增量。
- 远程工具请求。
- 工具结果记录。
- script review 结果。
- heartbeat 和 recovery 事件。
- run 完成。
- 错误。

- [ ] **步骤 3：实现运行时模型**

建议模型：

```csharp
public class AIAgentRunRequest
{
    public string ThreadId { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IReadOnlyAIContext AIContext { get; init; } = default!;
    public IReadOnlyList<string> SystemMessages { get; init; } = [];
    public CancellationToken CancellationToken { get; init; }
}
```

```csharp
public class AIAgentResumeRequest
{
    public string ThreadId { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public IReadOnlyList<RemoteToolResultItem> ToolResults { get; init; } = [];
    public IReadOnlyAIContext AIContext { get; init; } = default!;
    public IReadOnlyList<string> SystemMessages { get; init; } = [];
    public CancellationToken CancellationToken { get; init; }
}
```

```csharp
public interface IAIAgentRuntime
{
    IAsyncEnumerable<AIAgentRunOutput> RunAsync(AIAgentRunRequest request);
    IAsyncEnumerable<AIAgentRunOutput> ResumeAsync(AIAgentResumeRequest request);
}
```

`AIAgentRunOutput` 应表达输出类型、文本、远程工具调用、错误和可扩展 metadata。它不应暴露 Provider 专用类型。

输出类型至少覆盖：`MessageDelta`、`ThinkingDelta`、`ToolCallDelta`、`ToolCallRequested`、`ToolResultCompleted`、`ScriptReviewCompleted`、`Heartbeat`、`RecoveryStarted`、`RecoveryCompleted`、`RecoveryFailed`、`RunPaused`、`RunCompleted`、`Error`。

## 任务 2：实现 Runtime 输出到 SSE 的适配

- [ ] **步骤 1：编写适配器测试**

测试以下映射：

- `MessageDelta` -> `message.delta`
- `ThinkingDelta` -> `thinking.delta`
- `ToolCallDelta` -> `tool_call.delta`
- `ToolCallRequested` -> `tool_call.requested`
- `ToolResultCompleted` -> `tool_result.completed`
- `ScriptReviewCompleted` -> `script_review.completed`
- `Heartbeat` -> `agent.heartbeat`
- `RecoveryStarted` -> `agent.recovery.started`
- `RecoveryCompleted` -> `agent.recovery.completed`
- `RecoveryFailed` -> `agent.recovery.failed`
- `RunPaused` -> `run.paused`
- `RunCompleted` -> `run.completed`
- `Error` -> `error`

- [ ] **步骤 2：实现 `AIAgentStreamAdapter`**

适配器负责：

- 维护 run 内递增 `seq`。
- 把 Runtime 输出转换为 `AgentStreamEvent`。
- 对 `MessageDelta` 填充 `payload.text`，如内部输出包含 `delta` 也同时保留。
- 对远程工具请求填充 `tool_call_id`、`name`、`arguments`。
- 对工具结果填充 `tool_call_id`、`status`、`result` 或 `error`，事件名固定为 `tool_result.completed`。
- 对错误输出填充错误消息和错误码。

## 任务 3：控制器接入业务运行时

- [ ] **步骤 1：编写控制器测试**

使用假 `IAIAgentRuntime` 验证：

- `POST /agent/chat/stream` 输出 `run.started` 后继续输出 runtime 事件。
- runtime 的 `ToolCallRequested` 会写入 state store。
- runtime 的 `ToolCallDelta`、`ThinkingDelta` 会转换为兼容 SSE。
- runtime 完成时 run 状态变为 completed。
- runtime 抛异常时输出 `error`，并将 run 状态标记为 failed。
- provider 异常不会把 API key 或 Provider 私有参数写入 stream event、checkpoint 或 debug trace。

- [ ] **步骤 2：修改 `AIAgentController`**

控制器流程：

```text
接收 chat request
  -> 初始化 state store
  -> 创建 thread/run
  -> 构建 AIContext
  -> 构建系统提示词
  -> 输出 run.started
  -> 调用 IAIAgentRuntime.RunAsync
  -> 输出 runtime SSE events
  -> 记录 stream events / tool calls / run 状态
```

如果没有注册 `IAIAgentRuntime`，返回清晰错误事件，避免业务误以为已经接入真实 Agent。

## 任务 4：恢复远程工具结果

- [ ] **步骤 1：编写 resume 测试**

验证：

- `RemoteToolGateway.ValidateResumeAsync` 通过后调用 `IAIAgentRuntime.ResumeAsync`。
- resume 输出会继续转换为 SSE。
- 工具结果写回 state store。
- run 可以从 paused 转为 completed 或 failed。
- 多 pending tool call 必须一次性提交完整匹配结果集合；缺失、额外、重复或串 run 时不会调用 runtime。
- 终态 run 不允许 resume。

- [ ] **步骤 2：实现 resume 流程**

控制器流程：

```text
接收 remote tool results
  -> 初始化 state store
  -> 校验 thread/run/tool_call_id 完全匹配
  -> 记录工具结果和 tool_result.completed 事件
  -> 构建 AIContext 和系统提示词
  -> 调用 IAIAgentRuntime.ResumeAsync
  -> 输出 runtime SSE events
  -> 更新 run 状态
```

Runtime bridge 必须支持多轮 MAF tool calling。业务 runtime 收到工具参数验证失败时，应把失败作为工具结果或 observation 回填给 MAF，让模型有机会重试；Provider 异常统一转换为 `error` 事件并写入 run trace。

## 任务 5：补齐 provider-neutral 配置、script review、skill 和 watchdog

- [ ] **步骤 1：编写配置脱敏和 adapter 映射测试**

验证 runtime request 只接收 provider-neutral 配置摘要，`api_key` 不进入 stream event、checkpoint、debug trace。`reasoning` 和 `thinking` 开关由业务 adapter 映射，不暴露 Provider 类型。

- [ ] **步骤 2：实现 script review gate 接入点**

`runWordScript` 等远程编辑工具发给前端前，允许业务注册 script review gate。审查通过时继续发出 `tool_call.requested`；审查拒绝时记录 `script_review.completed`，再输出 `tool_result.completed`，其中 `payload.status = "failed"`，让 Agent 重写脚本。

- [ ] **步骤 3：实现 skill catalog 和按需加载 runtime 输出**

`GET /agent/skills` 返回轻量 catalog。`loadWordAgentSkill` 由业务 runtime 作为 server-executed、model-visible 工具处理，只允许读取已注册 skill 根目录内的 `SKILL.md` 和被引用的相对文本文件。加载事件必须进入 stream/debug trace。

- [ ] **步骤 4：实现 runtime watchdog 输出**

支持 idle timeout、thinking-only timeout、heartbeat 和最多一次 recovery prompt/observation。输出映射为 `agent.heartbeat`、`agent.recovery.started`、`agent.recovery.completed`、`agent.recovery.failed`。恢复失败后 run 进入 failed。

## 任务 6：MMB.Demo MAF + GLM5.1 真实 Provider 验证

- [ ] **步骤 1：新增 MMB.Demo GLM5.1 示例配置**

在 `MMB.Demo.WebAPI/appsettings.Development.json` 中放置本机真实调用配置。该文件由开发者本地维护，不提交仓库。MMB.Demo 只负责配置 MAF Provider 需要的 GLM5.1 参数，不手写 Z.AI/OpenAI-compatible HTTP Provider。

建议配置结构：

```json
{
  "MergeBlock": {
    "AI": {
      "GLM51": {
        "Enable": true,
        "BaseUrl": "https://api.z.ai/api/paas/v4/",
        "ApiKey": "<your-api-key>",
        "Model": "glm-5.1",
        "Temperature": 0.6,
        "MaxOutputTokens": 2048
      }
    }
  }
}
```

实现要求：

- `ApiKey` 只从 `appsettings.Development.json` 或等价开发环境配置读取，不写入仓库内默认配置。
- `BaseUrl`、`Model`、`Temperature`、`MaxOutputTokens` 提供可覆盖默认值。
- 未配置 `ApiKey` 时，运行时应输出清晰的 `error` 事件，错误消息说明需要配置 `MergeBlock:AI:GLM51:ApiKey`。
- 示例必须使用 Microsoft Agent Framework 的 Provider 接入能力。GLM5.1 只作为 MAF Provider 的模型配置参数，不在 `MMB.Demo` 中自行实现 HTTP Provider。
- 不把 MAF Provider 类型、Z.AI Provider 类型或任何模型厂商类型暴露到 `Materal.MergeBlock.AI.Abstractions`。

- [ ] **步骤 2：新增 MMB.Demo MAF Agent 对话运行时**

在 `MMB.Demo` 中注册一个用于验证的 `IAIAgentRuntime`。该 runtime 通过 MAF Agent 调用已配置的 GLM5.1 Provider 完成普通对话，同时保留一个固定触发词来验证远程工具暂停/恢复链路。

普通对话行为：

- 请求消息不包含远程工具触发词时，将用户消息、系统提示词和上下文传给 MAF Agent。
- 读取 MAF Agent 的 streaming 输出，逐段转换为 `message.delta`。
- 随后输出 `run.completed`。
- 该路径用于证明 `AIAgentController -> IAIAgentRuntime -> MAF Agent -> MAF Provider(GLM5.1) -> AIAgentStreamAdapter -> SSE` 的真实模型对话链路可用。

远程工具对话行为：

- 请求消息包含约定触发词，例如 `use-remote-tool`。
- 不调用 GLM5.1，直接输出 `tool_call.requested`，工具名使用通用示例名 `runClientAction`。
- 记录 pending tool call。
- 输出 `run.paused`。
- resume 收到匹配工具结果后，输出 `message.delta`，内容可以是 `Remote tool completed`。
- 随后输出 `run.completed`。

- [ ] **步骤 3：新增 MMB.Demo 对话运行时测试或最小验证辅助**

在 `MMB.Demo` 或 `Materal.MergeBlock.AI.Web.Test` 中增加可自动验证的 runtime 测试，至少覆盖：

- 未配置 `ApiKey` 时会产生清晰的 `error` 输出。
- 使用可替换的 fake GLM provider 时，普通消息会产生 `message.delta` 和 `run.completed`。
- 远程工具消息会产生 `tool_call.requested` 和 `run.paused`。
- resume 后会产生 `message.delta` 和 `run.completed`。

- [ ] **步骤 4：构建 MMB.Demo**

运行：

```powershell
dotnet build .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.slnx
```

- [ ] **步骤 5：运行 WebAPI 验证 GLM5.1 普通流式对话**

准备 `MMB.Demo.WebAPI/appsettings.Development.json`，填入真实 GLM5.1 API Key。运行 `MMB.Demo.WebAPI`，调用：

```powershell
curl -N -X POST http://127.0.0.1:5000/agent/chat/stream -H "Content-Type: application/json" -d "{\"threadId\":\"demo-thread\",\"message\":\"hello\"}"
```

预期包含：

```text
event: run.started
event: message.delta
event: run.completed
```

其中 `message.delta` 的内容应来自 GLM5.1，而不是固定 `Echo` 文本。

- [ ] **步骤 6：运行 WebAPI 验证远程工具式对话**

调用触发远程工具的消息：

```powershell
curl -N -X POST http://127.0.0.1:5000/agent/chat/stream -H "Content-Type: application/json" -d "{\"threadId\":\"demo-thread\",\"message\":\"use-remote-tool\"}"
```

预期包含：

```text
event: run.started
event: tool_call.requested
event: run.paused
```

从 `tool_call.requested` 的返回数据中取出 `run_id` 和 `tool_call_id`，调用 resume 接口：

```powershell
curl -N -X POST http://127.0.0.1:5000/agent/chat/resume/stream -H "Content-Type: application/json" -d "{\"threadId\":\"demo-thread\",\"runId\":\"{run_id}\",\"toolResults\":[{\"toolCallId\":\"{tool_call_id}\",\"status\":\"completed\",\"result\":{\"ok\":true}}]}"
```

预期包含：

```text
event: tool_result.completed
event: message.delta
event: run.completed
```

阶段 3 只有在上述两个 MMB.Demo 运行时对话路径都可用时，才视为完成。单元测试通过但 MMB.Demo 不能通过 GLM5.1 完成真实普通流式对话，不算阶段 3 完成。

## 任务 7：最终验证

- [ ] 运行 AI core 测试。
- [ ] 运行 AI.Web 测试。
- [ ] 运行 MMB.Demo 构建。
- [ ] 准备本机 `MMB.Demo.WebAPI/appsettings.Development.json`，确认 GLM5.1 API Key 未写入仓库跟踪文件。
- [ ] 运行 MMB.Demo.WebAPI，验证 GLM5.1 普通流式对话输出 `run.started`、`message.delta`、`run.completed`。
- [ ] 运行 MMB.Demo.WebAPI，验证远程工具式对话输出 `tool_call.requested`、`run.paused`，并可通过 resume 输出 `tool_result.completed`、`message.delta`、`run.completed`。
- [ ] 检查阶段 3 相关文件 CRLF。
- [ ] 运行 GitNexus change detection。

仅在用户明确要求提交时提交，提交信息使用中文。
