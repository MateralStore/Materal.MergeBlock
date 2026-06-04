# Agent 服务迁移到 MMB 缺陷清单

## 背景

本文档记录将现有本地 Agent 服务迁移到 MMB / `Materal.MergeBlock.AI` 时发现的缺陷与缺口，重点标出会阻断迁移验收的事项。本文档刻意只保留通用 Agent Host、远程客户端工具、运行时桥接、模型配置和审计能力，不包含具体业务域或客户端宿主的专有能力。

基线信息：

- 源服务当前是本地 FastAPI Agent 服务，核心能力包括 SSE 对话、LangGraph checkpoint、远程客户端工具暂停/恢复、执行前审查、server-executed 工具、运行审计和 runtime watchdog。
- MMB 当前已有 `Materal.MergeBlock.AI.Abstractions`、`Materal.MergeBlock.AI`、`Materal.MergeBlock.AI.Web`、`IAIAgentRuntime`、SSE 事件适配、远程工具网关、SQLite 状态存储和 `MMB.Demo` 的 GLM5.1 示例。
- 源仓库 GitNexus 索引落后 9 个提交，尝试执行 `npx gitnexus analyze` 时失败，错误为 `Cannot destructure property 'package' of 'node.target' as it is null.` 因此本清单主要基于源码和测试结果。

验证记录：

- `uv run pytest` in source Agent service: 165 passed。
- `dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj -p:UseSharedCompilation=false`: 30 passed。
- `dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj -p:UseSharedCompilation=false`: 51 passed。
- `dotnet test --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.Test\MMB.Demo.Test.csproj --filter Glm51AIAgentRuntimeTest -p:UseSharedCompilation=false`: 6 passed。

## 阻断项

### B1. HTTP DTO 与前端 JSON 契约不兼容

严重级别：阻断

现象：

- 现有前端发送 snake_case 字段：`schema_version`、`thread_id`、`run_id`、`model_config`、`tool_results`。
- 源服务的 `AgentChatRequest` 支持 `model_config`、`reasoning`、`thinking`、`skill_request`、`script_review`。
- MMB 当前 `AgentChatRequest` 只有 `SchemaVersion`、`ThreadId`、`RunId`、`Message`。
- `Materal.MergeBlock.Web` 全局 JSON 设置为 `PropertyNamingPolicy = null`，不会自动把 `thread_id` 绑定到 `ThreadId`。

影响：

- 现有前端直接切到 MMB 后，请求体无法完整绑定。
- 模型配置、API key、reasoning/thinking、执行前审查开关和显式能力请求会丢失。
- resume 请求也存在 `frontend-tool-results-v1` 与 MMB `remote-tool-results-v1` 的契约差异。

解除条件：

- 在 MMB Web 边界新增兼容 DTO 或显式 `[JsonPropertyName]` 映射。
- 保持现有前端契约字段名不变，至少兼容：
  - `agent-chat-request-v1`
  - `frontend-tool-results-v1`
  - `agent-stream-v1`
  - `agent-skill-catalog-v1`
- `AgentChatRequest` 补齐 `model_config`、`reasoning`、`thinking`、`skill_request`、`script_review`。

相关位置：

- Source Agent service `app/agent/schemas.py`
- Source frontend `src/features/agent-chat/useAgentChatStream.ts`
- `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentChatRequest.cs`
- `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/RemoteToolResultsRequest.cs`
- `Materal.MergeBlock/Materal.MergeBlock.Web/WebModule.cs`

### B2. 缺少真实业务 Agent Runtime

严重级别：阻断

现象：

- 源服务的核心执行流包含模型流、工具参数增量、工具参数校验重试、server-executed 工具执行、远程客户端工具暂停、resume 后继续、checkpoint 修复、取消和 watchdog。
- MMB 当前只有通用 `IAIAgentRuntime` 抽象。
- `MMB.Demo` 的 `Glm51AIAgentRuntime` 只是普通 GLM 对话加 `use-remote-tool` 触发的演示远程工具，不具备源服务的完整业务运行时行为。

影响：

- 即使 Web API 路由可用，也无法执行源服务原有的 Agent 工作流。
- Agent 不会绑定真实远程客户端工具，也不会在模型请求远程工具时进入完整暂停/恢复循环。
- 多轮工具调用、参数重试、checkpoint resume 和 abandoned tool call repair 都不可用。

解除条件：

- 新增业务模块级 `IAIAgentRuntime` 实现。
- Runtime 必须支持：
  - 模型流式输出到 `message.delta`。
  - reasoning/thinking 到 `thinking.delta`。
  - tool call chunk 到 `tool_call.delta`。
  - 远程客户端工具请求到 `tool_call.requested` + `run.paused`。
  - server-executed 工具直接返回 `tool_result.completed`。
  - resume 后把 tool result 回填给 Agent 继续运行。
  - 工具参数校验失败后让模型重试。
  - Provider 异常、取消和 timeout 进入 trace。

相关位置：

- Source Agent service `app/agent/graph.py`
- `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/IAIAgentRuntime.cs`
- `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/Glm51AIAgentRuntime.cs`

### B3. 远程客户端工具契约未迁移

严重级别：阻断

现象：

- 源服务当前维护了一组前端执行的远程客户端工具。
- 这些工具带输入 schema、结果 schema、权限、参数校验和模型可见描述。
- MMB 当前只有通用 `AIToolDescriptor`，没有源服务远程工具的注册、schema、权限和校验。

影响：

- 模型无法知道有哪些远程客户端工具可调用。
- 服务端无法校验远程工具参数。
- 前端无法收到稳定的 tool request。
- 修改型或高风险工具无法进入执行前审查和审计链路。

解除条件：

- 新增业务模块级远程工具元数据提供器，注册源服务当前依赖的远程客户端工具契约。
- 为每个工具提供输入 schema、结果 schema、权限级别和描述。
- 在 runtime 中基于注册表校验工具名和参数。
- 高风险远程工具的 Agent-facing schema 必须保持稳定，不应在迁移中扩展成另一套复杂协议。

相关位置：

- Source Agent service `app/agent/remote_tool_registry.py`
- `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolDescriptor.cs`
- `Materal.MergeBlock/Materal.MergeBlock.AI/Tools/AIToolRegistry.cs`

### B4. 暂停/恢复协议与前端预期不完全一致

严重级别：阻断

现象：

- 源服务在远程工具请求后输出 `tool_call.requested`，随后输出 `run.paused`，payload 包含 `reason = waiting_for_frontend_tool` 和 `tool_call_ids`。
- MMB `RunPaused` 当前默认 payload 为空，除非业务 runtime 额外传 metadata。
- 源服务对外 run 状态使用 `paused`；MMB 当前持久化状态使用 `waiting_tool_result`。
- 源服务 resume schema 是 `frontend-tool-results-v1`；MMB 模型名为 `RemoteToolResultsRequest`，默认 `SchemaVersion` 是 `remote-tool-results-v1`。

影响：

- 现有前端审批面板依赖 `run.paused.payload.tool_call_ids` 聚合待执行工具。
- 如果状态或 schema 不兼容，前端可能无法提交工具结果，或调试界面显示异常。

解除条件：

- `run.paused` 必须输出兼容 payload：`reason` 和 `tool_call_ids`。
- resume 请求必须兼容 `frontend-tool-results-v1` 和 `tool_results[].tool_call_id`。
- 对外 run 状态至少兼容 `paused`；内部可继续使用 `waiting_tool_result`，但 Web 边界需要映射。
- 多 pending tool call 必须一次性精确匹配，缺失、额外、重复、跨 run、终态 run 均拒绝。

相关位置：

- Source Agent service `app/agent/graph.py`
- Source Agent service `app/api/agent_chat.py`
- `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentStreamAdapter.cs`
- `Materal.MergeBlock/Materal.MergeBlock.AI.Web/RemoteTools/RemoteToolGateway.cs`
- `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentRunStatus.cs`

### B5. 模型配置与 Provider 选择未兼容

严重级别：阻断

现象：

源服务支持请求级模型配置：

- `provider`
- `adapter`
- `model`
- `base_url`
- `api_key`
- `temperature`
- `max_tokens`
- `reasoning`
- `thinking`

源服务支持多个 Provider 和兼容适配策略。MMB Demo 当前只实现了 GLM5.1 配置示例，并且从 MMB Demo 配置读取，不接收现有前端的 `model_config`。

影响：

- 前端模型设置面板无法继续控制模型。
- reasoning/thinking 等 Provider 适配行为丢失。
- 执行前审查也无法复用当前 Agent 选中的模型配置。

解除条件：

- 在 MMB Web DTO 中接收 provider-neutral `model_config`。
- Runtime 内或业务 adapter 内实现 Provider 映射。
- 保证 `api_key` 不进入 stream event、checkpoint、debug trace 或日志。
- 保留 `reasoning` 和 `thinking` 的兼容语义。

相关位置：

- Source Agent service `app/agent/provider_registry.py`
- Source Agent service `app/agent/model_factory.py`
- Source Agent service `app/agent/schemas.py`
- `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/Glm51AIOptions.cs`

## 重大缺口

### M1. server-executed 工具缺失

严重级别：重大

缺口：

- 源服务包含 server-executed、model-visible 的运行时工具。
- 这些工具由服务端执行，进入 stream/debug trace，但不暂停给前端。
- MMB 当前只有 `GET /agent/skills` 的 catalog provider，没有模型调用 server-executed 工具后返回工具结果的 runtime 闭环。

影响：

- Agent 不能按需加载运行时知识包或工作流辅助信息。
- 复杂任务不能通过计划类工具外化步骤，watchdog recovery 后也缺少明确下一步工具。

解除条件：

- 增加业务模块级能力目录 provider。
- 增加 server-executed 工具读取能力，限制只能访问已注册能力根目录内的允许文件。
- 增加计划类 server-executed 工具，记录计划事件和工具结果。

### M2. 系统提示词和安全边界未迁移

严重级别：重大

缺口：

- 源服务系统提示词包含工具使用流程、文本安全规则、行动规则、功能边界和高风险操作限制。
- MMB 当前只有通用 prompt contributor 框架，没有迁移源服务专用的 prompt contributor。

影响：

- 模型可能跳过先获取证据、再执行小步骤、最后验证结果的工作流。
- 高风险工具调用缺少迁移前已有的行为约束。

解除条件：

- 新增业务模块级 `IAIPromptContributor` 实现。
- 将源服务系统提示词中的核心通用规则迁移为 MMB prompt contributor。
- 保持长文本知识包按需加载，不把所有 reference 内容塞进默认 prompt。

### M3. 执行前审查 gate 未实现

严重级别：重大

缺口：

- 源服务可在高风险远程工具发给前端前调用独立 LLM 审查。
- MMB 当前只支持记录 `script_review.completed` 事件和表，没有审查器、审查 prompt、启用开关和拦截点。

影响：

- 高风险远程工具可能未经服务端预审直接发送给前端执行。

解除条件：

- 新增可插拔执行前审查接口。
- 在高风险远程工具发出前执行审查。
- 拒绝时输出 `script_review.completed` 和 failed `tool_result.completed`，不暂停给前端执行。
- 审查失败应 fail closed。

### M4. runtime watchdog / recovery 未实现

严重级别：重大

缺口：

- 源服务有 idle timeout、thinking-only timeout、heartbeat 和最多一次 recovery。
- MMB 当前支持相关事件类型，但没有实际 watchdog 逻辑。

影响：

- 模型长时间只输出 thinking 或无输出时，前端只能等待。
- 无法自动注入 recovery observation 让模型重新行动。

解除条件：

- 在业务 runtime 或通用 runtime adapter 中加入 watchdog。
- 输出 `agent.heartbeat`、`agent.recovery.started`、`agent.recovery.completed`、`agent.recovery.failed`。
- recovery 失败后 run 进入 failed。

### M5. checkpoint 语义不足

严重级别：重大

缺口：

- 源服务使用 checkpoint 保存真实中断点和消息状态。
- MMB SQLite 目前有 checkpoint 表，但只保存 metadata 和 model config summary，尚不足以恢复完整 Agent 内部状态。

影响：

- 服务重启、进程异常或 abandoned frontend tool call 后，无法稳定恢复模型上下文。

解除条件：

- 明确 MMB 运行时状态保存策略。
- 如果使用 MAF，需要保存 MAF session / thread / tool result replay 所需状态。
- 如果重建源服务行为，需要保存消息、pending tool calls、assistant reasoning content 和工具结果 replay 信息。

## 普通缺口

### N1. debug trace 结构需要兼容前端

严重级别：普通

缺口：

- 源服务 debug trace 返回 session/run/message/event/tool_call/timeline。
- MMB 已有 run trace、session trace 和 debug trace summary，但 timeline 聚合结构与现有前端不一定一致。

建议：

- 对齐现有 debug trace 响应，或在前端增加 MMB trace adapter。

### N2. 取消接口语义需要补事件

严重级别：普通

缺口：

- 源服务 cancel 会记录 `run.cancelled` stream event，并完成 run。
- MMB 当前 cancel 主要更新状态并取消 token，需要确认是否记录同等 stream event。

建议：

- cancel 成功时记录 `run.cancelled` 事件，payload 包含 `status`、`reason`、`source`。

### N3. 工具审计需要脱敏策略

严重级别：普通

缺口：

- MMB 已有 `IAIToolCallAuditor`，但远程工具参数可能包含用户内容、执行参数、错误和模型上下文。

建议：

- 为远程工具审计增加参数摘要和敏感字段脱敏。
- 避免 API key、Token 和大段用户内容进入日志。

## 建议优先级

第一阶段必须解除阻断项：

1. 兼容 HTTP DTO 和 JSON 字段。
2. 注册远程客户端工具契约。
3. 实现业务 `IAIAgentRuntime` 最小闭环。
4. 对齐 `run.paused` / resume 协议。
5. 接入请求级模型配置。

第二阶段补齐安全与稳定能力：

1. 迁移业务 prompt contributor。
2. 实现 server-executed 工具。
3. 实现执行前审查 gate。
4. 实现 watchdog / recovery。
5. 完善 checkpoint 与 debug trace。

第三阶段做端到端验收：

1. 前端发起普通对话，MMB 返回 `run.started`、`message.delta`、`run.completed`。
2. 前端触发只读远程工具，MMB 输出 `tool_call.requested`、`run.paused`，前端 resume 后继续。
3. 前端触发高风险远程工具，执行前审查通过后暂停给前端执行，resume 后继续并可验证。
4. 执行前审查拒绝时不发给前端，直接返回 failed tool result。
5. 多工具请求一次暂停，多结果精确 resume。
6. 模型 reasoning/thinking、工具参数增量、heartbeat、recovery 和取消事件均进入 stream/debug trace。

## 验收口径

迁移不能只以 MMB AI Core/Web 测试通过作为完成标准。只有满足以下条件，才算源服务功能迁移可验收：

- 现有前端无需大改即可调用 MMB Agent API。
- 所有 Agent-facing 工具名称、参数、事件和 resume 行为兼容现有协议。
- 上下文获取、远程客户端执行、结果验证、知识包加载、执行前审查、计划工具、取消和 debug trace 都有端到端测试。
- API key 和敏感配置不进入持久化审计或流式事件。
- 源服务当前测试所覆盖的关键行为，在 MMB 侧有等价测试或端到端验证。
