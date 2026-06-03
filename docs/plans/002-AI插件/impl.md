# Materal.MergeBlock.AI 实施计划

> **面向代理执行者：** 必需子技能：按任务逐项实施本计划时，请使用 `superpowers:subagent-driven-development`（推荐）或 `superpowers:executing-plans`。步骤使用复选框（`- [ ]`）语法便于跟踪。

**目标：** 构建 Materal.MergeBlock AI 插件，将其作为 Microsoft Agent Framework 的集成层；随后增加 Web Agent Host 和通用 Agent Runtime Bridge 支持。

**架构：** 实施拆分为三个可独立测试的阶段。阶段 1 构建核心抽象和 MergeBlock 模块；阶段 2 构建 Web/SSE 远程工具主机；阶段 3 构建通用 Agent Runtime Bridge，使业务模块可以通过 DI 接入自己的 MAF Agent 运行时。

**技术栈：** C#/.NET `net8.0;net9.0;net10.0`、MergeBlock 模块生命周期、Microsoft Agent Framework、`Microsoft.Extensions.AI`、ASP.NET Core 控制器/SSE、MSTest、SQLite 默认本地持久化实现。

---

## 计划文件

本文件夹包含拆分后的实施计划：

- `design.md`：已确认的设计与范围。
- `impl-01-core.md`：核心抽象、上下文、提示词、工具元数据、审计和 `AIModule`。
- `impl-02-web-agent-host.md`：`Materal.MergeBlock.AI.Web`、Remote Tool Gateway、SSE 契约、run 持久化、暂停/恢复/取消和调试追踪。
- `impl-03-agent-runtime-bridge.md`：通用 Agent Runtime Bridge、业务运行时接入点和 SSE 运行输出适配。

## 阶段顺序

1. **先做核心：** 实现 `Materal.MergeBlock.AI.Abstractions` 和 `Materal.MergeBlock.AI`。
2. **再做 Web Agent Host：** 仅基于核心扩展点实现 `Materal.MergeBlock.AI.Web`。
3. **最后做 Runtime Bridge：** 让 `Materal.MergeBlock.AI.Web` 调用业务注册的 Agent 运行时，并把运行输出转换为统一 SSE 事件。

阶段 1 测试通过前不要开始阶段 2。阶段 2 能证明一个假的远程工具可以暂停并恢复 run 前，不要开始阶段 3。

## 全局约束

- 不要在 MMB 中实现 OpenAI、Azure OpenAI、Ollama、Anthropic 等 Provider 包。
- 不要引入自定义 `ILLMClient` 抽象。
- 不要把具体业务模块逻辑放入 `Materal.MergeBlock.AI` 或 `Materal.MergeBlock.AI.Web`。
- 使用 `AIToolDescriptor.ExecutionMode` 分流 `Local` 和 `Remote` 工具。
- `IAIPromptContributor` 只能读取 `IReadOnlyAIContext`，不能修改 AI 上下文。
- `IAIToolCallAuditor` 是审计扩展点，不是业务工具声明接口。
- 所有新增或修改文件都保持 CRLF 行尾。
- 不要自动提交。如果用户要求提交，提交信息必须使用中文。

## 实施前必做检查

- [ ] 在 `E:\Project\Materal\Materal` 和 `E:\Project\Materal\Materal\Materal.MergeBlock` 中运行 `git status --short`。
- [ ] 阅读 `Materal.MergeBlock/docs/plans/002-AI插件/design.md`。
- [ ] 阅读相邻模块项目文件，例如 `Materal.MergeBlock.Logger`、`Materal.MergeBlock.EventBus` 和 `Materal.MergeBlock.Web`。
- [ ] 修改公共符号前，对要修改的符号运行 GitNexus 影响分析。
- [ ] 任何提交前，运行 GitNexus change detection 并报告受影响符号和执行流。

## 包策略

在仓库级 `Directory.Packages.props` 中增加 MAF 相关包版本，不要写到单个项目文件中。核心计划假设实现使用 Microsoft Learn 文档中的当前 Microsoft Agent Framework 包族，并避免在 MMB 中引入 Provider 专用包。

Provider 包属于业务应用。例如消费方业务 host 可以引用 OpenAI、Azure OpenAI 或 OpenAI 兼容 Provider 包，但 `Materal.MergeBlock.AI` 不能引用。

## 验证摘要

先运行最小范围测试：

```powershell
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj
dotnet test .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj
```

然后运行 MergeBlock 构建：

```powershell
dotnet build .\Materal.slnx
```

只有目标测试和构建通过后，才运行解决方案级测试：

```powershell
dotnet test .\Materal.slnx
```

运行时验证应使用 `Materal.MergeBlock\MMB` 下现有的 MergeBlock 运行时测试项目，尤其是 `Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.WebAPI`。除非 `MMB.Demo` 无法承载场景，否则不要为了 AI 插件单独创建运行时 demo。

## 阶段验收门槛

阶段 1 完成条件：

- 核心项目可为 `net8.0`、`net9.0` 和 `net10.0` 构建。
- 工具描述符能区分 `Local` 和 `Remote`。
- 上下文会冻结为只读快照。
- 提示词贡献器不能修改上下文。
- 审计器能收到本地/远程执行模式。

阶段 2 完成条件：

- `Materal.MergeBlock.AI.Web` 暴露 streaming、resume、cancel、session、run 和 debug trace 端点。
- 一个假的远程工具能产生 `tool_call.requested`，暂停 run，接受匹配的工具结果，并恢复。
- 不匹配的 `run_id`、`thread_id` 或 `tool_call_id` 会被拒绝。
- `MMB.Demo.WebAPI` 可以加载 AI Web 模块，并在真实 MergeBlock 运行时宿主中暴露 AI 路由。

阶段 3 完成条件：

- `Materal.MergeBlock.AI.Web` 通过抽象调用业务注册的 Agent 运行时。
- Runtime 输出可以转换为 `message.delta`、`tool_call.requested`、`run.paused`、`run.completed` 和 `error` 等 SSE 事件。
- 远程工具请求可以被持久化为 pending tool call，并通过 resume 接口把工具结果交回业务运行时。
- MMB.AI 不引用任何 Provider 包，也不包含具体业务模块逻辑。

## 用户确认后的提交检查点

如果用户明确要求提交，使用以下检查点：

```powershell
git add .\Materal.MergeBlock\Materal.MergeBlock.AI.Abstractions .\Materal.MergeBlock\Materal.MergeBlock.AI .\Materal.MergeBlock\Materal.MergeBlock.AI.Test
git commit -m "feat: 添加AI核心抽象与模块"
```

```powershell
git add .\Materal.MergeBlock\Materal.MergeBlock.AI.Web .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test
git commit -m "feat: 添加AI远程工具主机"
```
