# Agent Service Migration Compatibility Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `Materal.MergeBlock.AI.Web` compatible with the existing Agent service contract and provide the missing generic runtime hooks needed for remote tools, model settings, server-executed tools, pre-execution review, checkpointing, watchdog, and traceability.

**Architecture:** Keep MMB AI framework provider-neutral and business-neutral. Web DTOs preserve existing front-end wire contracts, then map into generic runtime request models. Business modules provide concrete `IAIAgentRuntime`, remote tool contracts, server-executed tools, prompt contributors, Provider adapters, and pre-execution review gates through DI.

**Tech Stack:** C#/.NET `net8.0;net9.0;net10.0`, ASP.NET Core controllers/SSE, `System.Text.Json`, `Microsoft.Data.Sqlite`, MergeBlock modules, MSTest, Microsoft Agent Framework in business/demo modules only.

---

## Source Defect Coverage

This plan resolves the generic defect list in the parent `002-AI插件` plan directory.

| Defect | Covered by |
| --- | --- |
| B1 HTTP DTO 与前端 JSON 契约不兼容 | Tasks 1, 2 |
| B2 缺少真实业务 Agent Runtime | Tasks 3, 8 |
| B3 远程客户端工具契约未迁移 | Task 4 |
| B4 暂停/恢复协议与前端预期不完全一致 | Tasks 2, 5 |
| B5 模型配置与 Provider 选择未兼容 | Tasks 1, 3, 8 |
| M1 server-executed 工具缺失 | Task 6 |
| M2 系统提示词和安全边界未迁移 | Task 7 |
| M3 执行前审查 gate 未实现 | Task 7 |
| M4 runtime watchdog / recovery 未实现 | Task 9 |
| M5 checkpoint 语义不足 | Task 10 |
| N1 debug trace 结构需要兼容前端 | Task 10 |
| N2 取消接口语义需要补事件 | Task 5 |
| N3 工具审计需要脱敏策略 | Task 11 |
| T1 测试计划和 Demo 页面能力不足 | Task 12, `testing-plan.md` |

## File Structure

Create or modify these files.

### Core Runtime Abstractions

- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRunRequest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentResumeRequest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRunOutput.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRunOutputType.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentModelConfig.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentReasoningConfig.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentThinkingConfig.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentSkillRequest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentPreExecutionReviewConfig.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRemoteToolCall.cs`

### Tool Contracts And Server Tools

- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolDescriptor.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolContractSchema.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolPermissionLevel.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/IAIServerTool.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIServerToolContext.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIServerToolResult.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI/Tools/AIToolRegistry.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI/Tools/AIServerToolRegistry.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI/Extensions/DIExtensions.cs`

### Web Contract Compatibility

- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentChatRequest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/RemoteToolResultsRequest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/CancelAgentRunRequest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentStreamEvent.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentSkillCatalogResponse.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentWireContractJsonContext.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentRunStatusMapper.cs`

### Web Runtime Host

- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Controllers/AIAgentController.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentStreamAdapter.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentRuntimeRequestFactory.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentRuntimeWatchdog.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentWatchdogOptions.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/RemoteTools/RemoteToolGateway.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/AIWebModule.cs`

### Persistence And Trace

- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/IAIAgentStateStore.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/SqliteAIAgentStateStore.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/AgentRunTrace.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/AgentTimelineItem.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/AgentToolResultRecord.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/AgentModelConfigSummary.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/AgentTraceRedactor.cs`

### Pre-Execution Review

- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Review/IAIAgentPreExecutionReviewer.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Review/AIAgentPreExecutionReviewRequest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Review/AIAgentPreExecutionReviewResult.cs`

### Tests

- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Runtime/AIAgentModelConfigTest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Tools/AIToolContractSchemaTest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Tools/AIServerToolRegistryTest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Models/AgentWireContractJsonTest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentRuntimeRequestFactoryTest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentRuntimeWatchdogTest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Persistence/AgentTraceRedactorTest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentControllerRuntimeTest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/RemoteTools/RemoteToolGatewayTest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Persistence/SqliteAIAgentStateStoreTest.cs`

### Demo Runtime Validation

- Modify: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/Glm51AIAgentRuntime.cs`
- Modify: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/MafGlm51AgentRunner.cs`
- Modify: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Test/AI/Glm51AIAgentRuntimeTest.cs`
- Create: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Test/AI/AgentWireCompatibilityTest.cs`
- Modify when needed: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.WebAPI/wwwroot/ai-chat-test.html`

### Test Planning

- Create: `Materal.MergeBlock/docs/plans/002-AI插件/agent-service-migration/testing-plan.md`

## Implementation Rules

- Do not add Provider-specific packages to `Materal.MergeBlock.AI` or `Materal.MergeBlock.AI.Web`.
- Do not expose Provider SDK types in `Materal.MergeBlock.AI.Abstractions`.
- Keep API key and raw credentials out of stream events, checkpoint metadata, debug trace, and audit metadata.
- Treat `testing-plan.md` as the release test matrix. Automated tests are required, and Demo page manual P0 scenarios must pass before migration sign-off.
- If `MMB.Demo.WebAPI/wwwroot/ai-chat-test.html` cannot execute a P0 manual scenario from `testing-plan.md`, upgrade that page before marking the scenario complete.
- Preserve existing external event names: `run.started`, `message.delta`, `thinking.delta`, `tool_call.delta`, `tool_call.requested`, `tool_result.completed`, `script_review.completed`, `agent.heartbeat`, `agent.recovery.started`, `agent.recovery.completed`, `agent.recovery.failed`, `run.paused`, `run.cancelled`, `run.completed`, `error`.
- Preserve `message.delta.payload.text`.
- Preserve `tool_result.completed` for completed, failed, and rejected tool results. Do not add `tool_result.failed` or `tool_call.resumed`.
- Before modifying shared symbols, run GitNexus impact analysis as required by `AGENTS.md`.
- Do not commit unless the user explicitly requests it. If committing later, use a Chinese commit message.

## Task 0: Preparation And Impact Analysis

**Files:**
- Read: the generic defect list in `Materal.MergeBlock/docs/plans/002-AI插件`
- Read: `Materal.MergeBlock/docs/plans/002-AI插件/design.md`
- Read: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Controllers/AIAgentController.cs`
- Read: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/IAIAgentRuntime.cs`

- [ ] **Step 1: Check worktree status**

Run:

```powershell
git status --short
git -C .\Materal.MergeBlock status --short
```

Expected: Existing unrelated user changes may be present. Do not revert them.

- [ ] **Step 2: Run GitNexus impact for shared runtime symbols**

Run GitNexus impact analysis before modifying these symbols:

```text
impact target=AIAgentController direction=upstream repo=Materal
impact target=AIAgentRunRequest direction=upstream repo=Materal
impact target=AIAgentResumeRequest direction=upstream repo=Materal
impact target=AIAgentRunOutput direction=upstream repo=Materal
impact target=AIToolDescriptor direction=upstream repo=Materal
impact target=IAIAgentStateStore direction=upstream repo=Materal
impact target=RemoteToolGateway direction=upstream repo=Materal
```

Expected: Risk should be LOW or MEDIUM because these are new AI module symbols. If any result is HIGH or CRITICAL, stop and report before editing.

- [ ] **Step 3: Confirm baseline tests pass before code changes**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj -p:UseSharedCompilation=false
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj -p:UseSharedCompilation=false
dotnet test --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.Test\MMB.Demo.Test.csproj --filter Glm51AIAgentRuntimeTest -p:UseSharedCompilation=false
```

Expected: All tests pass. If file locks occur, run `dotnet build-server shutdown` and retry sequentially.

## Task 1: Add Provider-Neutral Runtime Request Models

**Files:**
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentModelConfig.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentReasoningConfig.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentThinkingConfig.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentSkillRequest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentPreExecutionReviewConfig.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRunRequest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentResumeRequest.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Runtime/AIAgentModelConfigTest.cs`

- [ ] **Step 1: Write runtime model tests**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Runtime/AIAgentModelConfigTest.cs`:

```csharp
namespace Materal.MergeBlock.AI.Test.Runtime;

[TestClass]
public class AIAgentModelConfigTest
{
    [TestMethod]
    public void NewModelConfig_ShouldKeepProviderNeutralFields()
    {
        AIAgentModelConfig config = new()
        {
            Provider = "openai_compatible",
            Adapter = "deepseek_openai",
            Model = "model-a",
            BaseUrl = "https://example.test/v1",
            ApiKey = "secret",
            Temperature = 0.2f,
            MaxTokens = 2048,
            Reasoning = new AIAgentReasoningConfig
            {
                Enabled = true,
                Effort = "high",
                BudgetTokens = 8192,
                Summary = "auto"
            },
            Thinking = new AIAgentThinkingConfig
            {
                Enabled = true,
                BudgetTokens = 4096
            }
        };

        Assert.AreEqual("openai_compatible", config.Provider);
        Assert.AreEqual("deepseek_openai", config.Adapter);
        Assert.AreEqual("model-a", config.Model);
        Assert.AreEqual("https://example.test/v1", config.BaseUrl);
        Assert.AreEqual("secret", config.ApiKey);
        Assert.AreEqual(0.2f, config.Temperature);
        Assert.AreEqual(2048, config.MaxTokens);
        Assert.IsNotNull(config.Reasoning);
        Assert.IsNotNull(config.Thinking);
    }

    [TestMethod]
    public void NewRunRequest_ShouldCarryModelAndReviewSettings()
    {
        AIAgentRunRequest request = new()
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Message = "hello",
            ModelConfig = new AIAgentModelConfig
            {
                Provider = "openai",
                Model = "gpt-test",
                ApiKey = "secret"
            },
            SkillRequest = new AIAgentSkillRequest
            {
                Name = "analysis",
                Description = "Use analysis capability"
            },
            PreExecutionReview = new AIAgentPreExecutionReviewConfig
            {
                Enabled = true
            }
        };

        Assert.AreEqual("openai", request.ModelConfig.Provider);
        Assert.AreEqual("analysis", request.SkillRequest.Name);
        Assert.IsTrue(request.PreExecutionReview.Enabled);
    }
}
```

- [ ] **Step 2: Run runtime model tests and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIAgentModelConfigTest -p:UseSharedCompilation=false
```

Expected: FAIL because the runtime model types do not exist.

- [ ] **Step 3: Add model config types**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentReasoningConfig.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent推理配置
/// </summary>
public class AIAgentReasoningConfig
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; init; }
    /// <summary>
    /// 推理强度
    /// </summary>
    public string Effort { get; init; } = "medium";
    /// <summary>
    /// 推理预算Token数
    /// </summary>
    public int? BudgetTokens { get; init; }
    /// <summary>
    /// 摘要策略
    /// </summary>
    public string Summary { get; init; } = "auto";
}
```

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentThinkingConfig.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent思考配置
/// </summary>
public class AIAgentThinkingConfig
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; init; }
    /// <summary>
    /// 预算Token数
    /// </summary>
    public int BudgetTokens { get; init; } = 1024;
}
```

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentModelConfig.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent模型配置
/// </summary>
public class AIAgentModelConfig
{
    /// <summary>
    /// Provider名称
    /// </summary>
    public string Provider { get; init; } = string.Empty;
    /// <summary>
    /// Provider适配器
    /// </summary>
    public string? Adapter { get; init; }
    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; init; } = string.Empty;
    /// <summary>
    /// API地址
    /// </summary>
    public string? BaseUrl { get; init; }
    /// <summary>
    /// API密钥
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;
    /// <summary>
    /// 温度
    /// </summary>
    public float Temperature { get; init; } = 0.2f;
    /// <summary>
    /// 最大输出Token数
    /// </summary>
    public int MaxTokens { get; init; } = 1200;
    /// <summary>
    /// 推理配置
    /// </summary>
    public AIAgentReasoningConfig? Reasoning { get; init; }
    /// <summary>
    /// 思考配置
    /// </summary>
    public AIAgentThinkingConfig? Thinking { get; init; }
}
```

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentSkillRequest.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent能力请求
/// </summary>
public class AIAgentSkillRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
```

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentPreExecutionReviewConfig.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent执行前审查配置
/// </summary>
public class AIAgentPreExecutionReviewConfig
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; init; }
}
```

- [ ] **Step 4: Extend run and resume requests**

Modify `AIAgentRunRequest` by adding these properties:

```csharp
/// <summary>
/// 模型配置
/// </summary>
public AIAgentModelConfig ModelConfig { get; init; } = new();
/// <summary>
/// 能力请求
/// </summary>
public AIAgentSkillRequest? SkillRequest { get; init; }
/// <summary>
/// 执行前审查配置
/// </summary>
public AIAgentPreExecutionReviewConfig PreExecutionReview { get; init; } = new();
```

Modify `AIAgentResumeRequest` by adding these properties:

```csharp
/// <summary>
/// 模型配置
/// </summary>
public AIAgentModelConfig ModelConfig { get; init; } = new();
/// <summary>
/// 能力请求
/// </summary>
public AIAgentSkillRequest? SkillRequest { get; init; }
/// <summary>
/// 执行前审查配置
/// </summary>
public AIAgentPreExecutionReviewConfig PreExecutionReview { get; init; } = new();
```

- [ ] **Step 5: Run runtime model tests and verify pass**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIAgentModelConfigTest -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 2: Make Web DTOs Wire-Compatible

**Files:**
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentChatRequest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/RemoteToolResultsRequest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/CancelAgentRunRequest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentStreamEvent.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentSkillCatalogResponse.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentRunStatusMapper.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Models/AgentWireContractJsonTest.cs`

- [ ] **Step 1: Write wire contract JSON tests**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Models/AgentWireContractJsonTest.cs`:

```csharp
using System.Text.Json;

namespace Materal.MergeBlock.AI.Web.Test.Models;

[TestClass]
public class AgentWireContractJsonTest
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [TestMethod]
    public void AgentChatRequest_ShouldDeserializeSnakeCaseContract()
    {
        const string json = """
        {
          "schema_version": "agent-chat-request-v1",
          "thread_id": "thread_001",
          "run_id": "run_001",
          "message": "hello",
          "model_config": {
            "provider": "openai_compatible",
            "adapter": "deepseek_openai",
            "model": "model-a",
            "base_url": "https://example.test/v1",
            "api_key": "secret",
            "temperature": 0.3,
            "max_tokens": 2048,
            "reasoning": {
              "enabled": true,
              "effort": "high",
              "budget_tokens": 8192,
              "summary": "auto"
            },
            "thinking": {
              "enabled": true,
              "budget_tokens": 4096
            }
          },
          "skill_request": {
            "name": "analysis",
            "description": "Use analysis capability"
          },
          "script_review": {
            "enabled": true
          }
        }
        """;

        AgentChatRequest request = JsonSerializer.Deserialize<AgentChatRequest>(json, Options)!;

        Assert.AreEqual("agent-chat-request-v1", request.SchemaVersion);
        Assert.AreEqual("thread_001", request.ThreadId);
        Assert.AreEqual("run_001", request.RunId);
        Assert.AreEqual("hello", request.Message);
        Assert.AreEqual("openai_compatible", request.ModelConfig.Provider);
        Assert.AreEqual("deepseek_openai", request.ModelConfig.Adapter);
        Assert.AreEqual("model-a", request.ModelConfig.Model);
        Assert.AreEqual("https://example.test/v1", request.ModelConfig.BaseUrl);
        Assert.AreEqual("secret", request.ModelConfig.ApiKey);
        Assert.AreEqual(0.3f, request.ModelConfig.Temperature);
        Assert.AreEqual(2048, request.ModelConfig.MaxTokens);
        Assert.AreEqual("high", request.ModelConfig.Reasoning!.Effort);
        Assert.AreEqual(4096, request.ModelConfig.Thinking!.BudgetTokens);
        Assert.AreEqual("analysis", request.SkillRequest!.Name);
        Assert.IsTrue(request.PreExecutionReview.Enabled);
    }

    [TestMethod]
    public void RemoteToolResultsRequest_ShouldDeserializeFrontendToolResultsContract()
    {
        const string json = """
        {
          "schema_version": "frontend-tool-results-v1",
          "thread_id": "thread_001",
          "run_id": "run_001",
          "tool_results": [
            {
              "tool_call_id": "call_001",
              "status": "completed",
              "result": { "ok": true }
            }
          ]
        }
        """;

        RemoteToolResultsRequest request = JsonSerializer.Deserialize<RemoteToolResultsRequest>(json, Options)!;

        Assert.AreEqual("frontend-tool-results-v1", request.SchemaVersion);
        Assert.AreEqual("thread_001", request.ThreadId);
        Assert.AreEqual("run_001", request.RunId);
        Assert.AreEqual("call_001", request.ToolResults[0].ToolCallId);
        Assert.AreEqual("completed", request.ToolResults[0].Status);
    }

    [TestMethod]
    public void AgentStreamEvent_ShouldSerializeSnakeCaseContract()
    {
        AgentStreamEvent streamEvent = new()
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Seq = 1,
            Event = "message.delta",
            Payload = new Dictionary<string, object?> { ["text"] = "hello" }
        };

        string json = JsonSerializer.Serialize(streamEvent, Options);

        StringAssert.Contains(json, "\"schema_version\":\"agent-stream-v1\"");
        StringAssert.Contains(json, "\"thread_id\":\"thread_001\"");
        StringAssert.Contains(json, "\"run_id\":\"run_001\"");
    }
}
```

- [ ] **Step 2: Run wire contract tests and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter AgentWireContractJsonTest -p:UseSharedCompilation=false
```

Expected: FAIL because DTOs do not yet define snake_case JSON mappings or full request fields.

- [ ] **Step 3: Add JSON property mappings to DTOs**

Modify `AgentChatRequest` to include `JsonPropertyName` attributes and runtime config properties:

```csharp
using System.Text.Json.Serialization;

namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent对话请求
/// </summary>
public class AgentChatRequest
{
    [JsonPropertyName("schema_version")]
    public string SchemaVersion { get; init; } = "agent-chat-request-v1";
    [JsonPropertyName("thread_id")]
    public string ThreadId { get; init; } = string.Empty;
    [JsonPropertyName("run_id")]
    public string? RunId { get; init; }
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
    [JsonPropertyName("model_config")]
    public AIAgentModelConfig ModelConfig { get; init; } = new();
    [JsonPropertyName("reasoning")]
    public AIAgentReasoningConfig? Reasoning { get; init; }
    [JsonPropertyName("thinking")]
    public AIAgentThinkingConfig? Thinking { get; init; }
    [JsonPropertyName("skill_request")]
    public AIAgentSkillRequest? SkillRequest { get; init; }
    [JsonPropertyName("script_review")]
    public AIAgentPreExecutionReviewConfig PreExecutionReview { get; init; } = new();
}
```

Modify all Web model properties that are part of the wire contract with explicit `JsonPropertyName`. Use these exact wire names:

- `schema_version`
- `thread_id`
- `run_id`
- `tool_results`
- `tool_call_id`
- `status`
- `result`
- `error`
- `seq`
- `event`
- `payload`
- `skills`
- `id`
- `name`
- `description`

- [ ] **Step 4: Add JSON mappings to runtime config models**

Modify the runtime config models from Task 1 to include `JsonPropertyName` attributes. Use these wire names:

- `provider`
- `adapter`
- `model`
- `base_url`
- `api_key`
- `temperature`
- `max_tokens`
- `reasoning`
- `thinking`
- `enabled`
- `effort`
- `budget_tokens`
- `summary`
- `name`
- `description`

- [ ] **Step 5: Add public run status mapper**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Models/AgentRunStatusMapper.cs`:

```csharp
namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent运行状态映射器
/// </summary>
public static class AgentRunStatusMapper
{
    /// <summary>
    /// 转换为对外状态
    /// </summary>
    public static string ToPublicStatus(string status)
    {
        return string.Equals(status, AgentRunStatus.WaitingToolResult, StringComparison.OrdinalIgnoreCase)
            ? "paused"
            : status;
    }
}
```

- [ ] **Step 6: Run wire contract tests and verify pass**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter AgentWireContractJsonTest -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 3: Map Web Requests Into Runtime Requests

**Files:**
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentRuntimeRequestFactory.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Controllers/AIAgentController.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentRuntimeRequestFactoryTest.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentControllerRuntimeTest.cs`

- [ ] **Step 1: Write request factory tests**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentRuntimeRequestFactoryTest.cs`:

```csharp
namespace Materal.MergeBlock.AI.Web.Test.Runtime;

[TestClass]
public class AIAgentRuntimeRequestFactoryTest
{
    [TestMethod]
    public async Task CreateRunRequestAsync_ShouldCarryModelSettingsAndContext()
    {
        ServiceCollection services = new();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        AIAgentRuntimeRequestFactory factory = new(
            new AIContextBuilder(serviceProvider, []),
            new AIPromptBuilder([]));
        AgentChatRequest request = new()
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Message = "hello",
            ModelConfig = new AIAgentModelConfig
            {
                Provider = "openai",
                Model = "gpt-test",
                ApiKey = "secret"
            },
            SkillRequest = new AIAgentSkillRequest
            {
                Name = "analysis",
                Description = "Use analysis capability"
            },
            PreExecutionReview = new AIAgentPreExecutionReviewConfig
            {
                Enabled = true
            }
        };

        AIAgentRunRequest runtimeRequest = await factory.CreateRunRequestAsync(
            request,
            "thread_001",
            "run_001",
            CancellationToken.None);

        Assert.AreEqual("thread_001", runtimeRequest.ThreadId);
        Assert.AreEqual("run_001", runtimeRequest.RunId);
        Assert.AreEqual("hello", runtimeRequest.Message);
        Assert.AreEqual("openai", runtimeRequest.ModelConfig.Provider);
        Assert.AreEqual("analysis", runtimeRequest.SkillRequest!.Name);
        Assert.IsTrue(runtimeRequest.PreExecutionReview.Enabled);
    }
}
```

- [ ] **Step 2: Run request factory tests and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter AIAgentRuntimeRequestFactoryTest -p:UseSharedCompilation=false
```

Expected: FAIL because the request factory does not exist.

- [ ] **Step 3: Implement request factory**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentRuntimeRequestFactory.cs`:

```csharp
namespace Materal.MergeBlock.AI.Web.Runtime;

/// <summary>
/// AI Agent运行时请求工厂
/// </summary>
public class AIAgentRuntimeRequestFactory(
    AIContextBuilder contextBuilder,
    AIPromptBuilder promptBuilder)
{
    /// <summary>
    /// 创建运行请求
    /// </summary>
    public async Task<AIAgentRunRequest> CreateRunRequestAsync(
        AgentChatRequest request,
        string threadId,
        string runId,
        CancellationToken cancellationToken)
    {
        IReadOnlyAIContext aiContext = await contextBuilder.BuildAsync();
        IReadOnlyList<string> systemMessages = await promptBuilder.BuildSystemMessagesAsync(aiContext);
        AIAgentModelConfig modelConfig = ApplyTopLevelReasoningAndThinking(request);
        return new AIAgentRunRequest
        {
            ThreadId = threadId,
            RunId = runId,
            Message = request.Message,
            ModelConfig = modelConfig,
            SkillRequest = request.SkillRequest,
            PreExecutionReview = request.PreExecutionReview,
            AIContext = aiContext,
            SystemMessages = systemMessages,
            CancellationToken = cancellationToken
        };
    }

    /// <summary>
    /// 创建恢复请求
    /// </summary>
    public async Task<AIAgentResumeRequest> CreateResumeRequestAsync(
        AgentChatRequest baseRequest,
        RemoteToolResultsRequest request,
        CancellationToken cancellationToken)
    {
        IReadOnlyAIContext aiContext = await contextBuilder.BuildAsync();
        IReadOnlyList<string> systemMessages = await promptBuilder.BuildSystemMessagesAsync(aiContext);
        return new AIAgentResumeRequest
        {
            ThreadId = request.ThreadId,
            RunId = request.RunId,
            ToolResults = request.ToolResults.Select(ToRuntimeToolResult).ToArray(),
            ModelConfig = ApplyTopLevelReasoningAndThinking(baseRequest),
            SkillRequest = baseRequest.SkillRequest,
            PreExecutionReview = baseRequest.PreExecutionReview,
            AIContext = aiContext,
            SystemMessages = systemMessages,
            CancellationToken = cancellationToken
        };
    }

    private static AIAgentModelConfig ApplyTopLevelReasoningAndThinking(AgentChatRequest request)
    {
        AIAgentModelConfig modelConfig = request.ModelConfig;
        return new AIAgentModelConfig
        {
            Provider = modelConfig.Provider,
            Adapter = modelConfig.Adapter,
            Model = modelConfig.Model,
            BaseUrl = modelConfig.BaseUrl,
            ApiKey = modelConfig.ApiKey,
            Temperature = modelConfig.Temperature,
            MaxTokens = modelConfig.MaxTokens,
            Reasoning = modelConfig.Reasoning ?? request.Reasoning,
            Thinking = modelConfig.Thinking ?? request.Thinking
        };
    }

    private static AIAgentRemoteToolResult ToRuntimeToolResult(RemoteToolResultItem item)
    {
        return new AIAgentRemoteToolResult
        {
            ToolCallId = item.ToolCallId,
            Status = item.Status,
            Result = item.Result,
            Error = item.Error
        };
    }
}
```

- [ ] **Step 4: Register request factory**

Modify `AIWebModule.OnConfigureServices`:

```csharp
context.Services.AddSingleton<AIAgentRuntimeRequestFactory>();
```

- [ ] **Step 5: Use request factory in controller**

Modify `AIAgentController` constructor to replace direct `AIContextBuilder` and `AIPromptBuilder` parameters with `AIAgentRuntimeRequestFactory`.

In `StreamAsync`, replace manual context/prompt construction with:

```csharp
AIAgentRunRequest runRequest = await runtimeRequestFactory.CreateRunRequestAsync(
    request,
    threadId,
    runId,
    cancellationToken);
```

For resume, use a persisted base request or checkpoint metadata once Task 10 is complete. Until Task 10, create a base request with the run id and default model config, and add a failing test in Task 10 that proves persisted model config is restored.

- [ ] **Step 6: Run request factory tests and controller runtime tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter "AIAgentRuntimeRequestFactoryTest|AIAgentControllerRuntimeTest" -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 4: Add Generic Remote Tool Contract Registry

**Files:**
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolDescriptor.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolContractSchema.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolPermissionLevel.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI/Tools/AIToolRegistry.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Tools/AIToolContractSchemaTest.cs`

- [ ] **Step 1: Write remote tool contract tests**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Tools/AIToolContractSchemaTest.cs`:

```csharp
namespace Materal.MergeBlock.AI.Test.Tools;

[TestClass]
public class AIToolContractSchemaTest
{
    [TestMethod]
    public void Descriptor_ShouldStoreSchemaAndPermission()
    {
        AIToolDescriptor descriptor = new()
        {
            Name = "readClientState",
            Description = "Read client state",
            ExecutionMode = AIToolExecutionMode.Remote,
            PermissionLevel = AIToolPermissionLevel.Read,
            InputSchema = AIToolContractSchema.Object(
                new Dictionary<string, object?>
                {
                    ["maxItems"] = new Dictionary<string, object?>
                    {
                        ["type"] = "integer",
                        ["minimum"] = 1,
                        ["maximum"] = 100
                    }
                },
                ["maxItems"]),
            ResultSchema = AIToolContractSchema.GenericObject()
        };

        Assert.AreEqual(AIToolPermissionLevel.Read, descriptor.PermissionLevel);
        Assert.AreEqual("object", descriptor.InputSchema!.Schema["type"]);
        Assert.AreEqual("object", descriptor.ResultSchema!.Schema["type"]);
    }
}
```

- [ ] **Step 2: Run contract tests and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIToolContractSchemaTest -p:UseSharedCompilation=false
```

Expected: FAIL because schema and permission types do not exist.

- [ ] **Step 3: Add schema and permission types**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolPermissionLevel.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI工具权限级别
/// </summary>
public enum AIToolPermissionLevel
{
    /// <summary>
    /// 读取
    /// </summary>
    Read,
    /// <summary>
    /// 修改
    /// </summary>
    Edit,
    /// <summary>
    /// 高风险
    /// </summary>
    HighRisk
}
```

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIToolContractSchema.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI工具契约Schema
/// </summary>
public class AIToolContractSchema
{
    /// <summary>
    /// Schema内容
    /// </summary>
    public IReadOnlyDictionary<string, object?> Schema { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// 创建对象Schema
    /// </summary>
    public static AIToolContractSchema Object(
        IReadOnlyDictionary<string, object?> properties,
        IReadOnlyList<string>? required = null)
    {
        return new AIToolContractSchema
        {
            Schema = new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = required ?? [],
                ["additionalProperties"] = false
            }
        };
    }

    /// <summary>
    /// 创建通用对象Schema
    /// </summary>
    public static AIToolContractSchema GenericObject()
    {
        return new AIToolContractSchema
        {
            Schema = new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["additionalProperties"] = true
            }
        };
    }
}
```

- [ ] **Step 4: Extend AIToolDescriptor**

Add these properties to `AIToolDescriptor`:

```csharp
/// <summary>
/// 权限级别
/// </summary>
public AIToolPermissionLevel PermissionLevel { get; init; } = AIToolPermissionLevel.Read;
/// <summary>
/// 输入Schema
/// </summary>
public AIToolContractSchema? InputSchema { get; init; }
/// <summary>
/// 结果Schema
/// </summary>
public AIToolContractSchema? ResultSchema { get; init; }
/// <summary>
/// 是否需要执行前审查
/// </summary>
public bool RequirePreExecutionReview { get; init; }
```

- [ ] **Step 5: Run contract tests and existing scanner tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter "AIToolContractSchemaTest|AIToolScannerTest|AIToolDescriptorTest" -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 5: Align Pause, Resume, Cancel, And Public Run Status

**Files:**
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRunOutput.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentStreamAdapter.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Controllers/AIAgentController.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/RemoteTools/RemoteToolGateway.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/SqliteAIAgentStateStore.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentStreamAdapterTest.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentControllerRuntimeTest.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/RemoteTools/RemoteToolGatewayTest.cs`

- [ ] **Step 1: Add stream adapter test for pause payload**

Modify `AIAgentStreamAdapterTest` with:

```csharp
[TestMethod]
public void ToStreamEvent_ShouldWritePauseReasonAndToolCallIds()
{
    AIAgentStreamAdapter adapter = new();
    AgentStreamEvent streamEvent = adapter.ToStreamEvent(
        "thread_001",
        "run_001",
        10,
        AIAgentRunOutput.RunPaused(["call_001", "call_002"]));

    Assert.AreEqual("run.paused", streamEvent.Event);
    Assert.AreEqual("waiting_for_frontend_tool", streamEvent.Payload["reason"]);
    CollectionAssert.AreEqual(
        new[] { "call_001", "call_002" },
        ((IEnumerable<string>)streamEvent.Payload["tool_call_ids"]!).ToArray());
}
```

- [ ] **Step 2: Run stream adapter test and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter ToStreamEvent_ShouldWritePauseReasonAndToolCallIds -p:UseSharedCompilation=false
```

Expected: FAIL because `RunPaused` does not carry tool call ids.

- [ ] **Step 3: Extend RunPaused output**

Add properties to `AIAgentRunOutput`:

```csharp
/// <summary>
/// 工具调用ID集合
/// </summary>
public IReadOnlyList<string> ToolCallIds { get; init; } = [];
```

Replace `RunPaused()` with:

```csharp
/// <summary>
/// 创建运行暂停
/// </summary>
public static AIAgentRunOutput RunPaused(IReadOnlyList<string>? toolCallIds = null, string reason = "waiting_for_frontend_tool") => new()
{
    Type = AIAgentRunOutputType.RunPaused,
    Reason = reason,
    ToolCallIds = toolCallIds ?? []
};
```

- [ ] **Step 4: Update stream adapter payload**

In `AIAgentStreamAdapter.BuildPayload`, add:

```csharp
if (output.ToolCallIds.Count > 0)
{
    result["tool_call_ids"] = output.ToolCallIds;
}
```

- [ ] **Step 5: Record cancel event**

Modify `CancelAsync` to record and return a compatible cancel payload:

```csharp
AgentRunTrace trace = await stateStore.GetRunTraceAsync(runId);
if (!string.Equals(trace.Run.ThreadId, request.ThreadId, StringComparison.Ordinal))
{
    return Conflict();
}
int seq = trace.Events.Count + 1;
AgentStreamEvent streamEvent = new()
{
    ThreadId = request.ThreadId,
    RunId = runId,
    Seq = seq,
    Event = "run.cancelled",
    Payload = new Dictionary<string, object?>
    {
        ["status"] = "cancelled",
        ["reason"] = request.Reason,
        ["source"] = request.Source
    }
};
await stateStore.RecordStreamEventAsync(streamEvent);
cancellationRegistry.Cancel(runId);
await stateStore.CompleteRunAsync(runId, AgentRunStatus.Cancelled, $"{request.Source}:{request.Reason}");
return Ok(new Dictionary<string, object?>
{
    ["run_id"] = runId,
    ["thread_id"] = request.ThreadId,
    ["status"] = AgentRunStatus.Cancelled
});
```

- [ ] **Step 6: Run pause/resume/cancel tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter "AIAgentStreamAdapterTest|AIAgentControllerRuntimeTest|RemoteToolGatewayTest" -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 6: Add Server-Executed Tool Infrastructure

**Files:**
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/IAIServerTool.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIServerToolContext.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Tools/AIServerToolResult.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI/Tools/AIServerToolRegistry.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI/Extensions/DIExtensions.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Tools/AIServerToolRegistryTest.cs`

- [ ] **Step 1: Write server tool registry tests**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Tools/AIServerToolRegistryTest.cs`:

```csharp
namespace Materal.MergeBlock.AI.Test.Tools;

[TestClass]
public class AIServerToolRegistryTest
{
    [TestMethod]
    public async Task ExecuteAsync_ShouldRunRegisteredTool()
    {
        AIServerToolRegistry registry = new([new EchoServerTool()]);

        AIServerToolResult result = await registry.ExecuteAsync(
            "echo",
            new Dictionary<string, object?> { ["text"] = "hello" },
            CancellationToken.None);

        Assert.AreEqual("completed", result.Status);
        Assert.AreEqual("hello", result.Result!["text"]);
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldFailUnknownTool()
    {
        AIServerToolRegistry registry = new([]);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
            registry.ExecuteAsync("missing", new Dictionary<string, object?>(), CancellationToken.None));
    }

    private sealed class EchoServerTool : IAIServerTool
    {
        public AIToolDescriptor Descriptor { get; } = new()
        {
            Name = "echo",
            Description = "Echo input",
            ExecutionMode = AIToolExecutionMode.Local,
            PermissionLevel = AIToolPermissionLevel.Read
        };

        public Task<AIServerToolResult> ExecuteAsync(AIServerToolContext context)
        {
            return Task.FromResult(AIServerToolResult.Completed(context.Arguments));
        }
    }
}
```

- [ ] **Step 2: Run server tool tests and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIServerToolRegistryTest -p:UseSharedCompilation=false
```

Expected: FAIL because server tool infrastructure does not exist.

- [ ] **Step 3: Add server tool abstractions**

Create `IAIServerTool.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI服务端工具
/// </summary>
public interface IAIServerTool
{
    /// <summary>
    /// 工具描述
    /// </summary>
    AIToolDescriptor Descriptor { get; }
    /// <summary>
    /// 执行工具
    /// </summary>
    Task<AIServerToolResult> ExecuteAsync(AIServerToolContext context);
}
```

Create `AIServerToolContext.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI服务端工具上下文
/// </summary>
public class AIServerToolContext
{
    public string ThreadId { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public string ToolCallId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();
    public IReadOnlyAIContext AIContext { get; init; } = default!;
    public CancellationToken CancellationToken { get; init; }
}
```

Create `AIServerToolResult.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI服务端工具结果
/// </summary>
public class AIServerToolResult
{
    public string Status { get; init; } = AIToolCallStatus.Completed;
    public IReadOnlyDictionary<string, object?>? Result { get; init; }
    public IReadOnlyDictionary<string, object?>? Error { get; init; }

    public static AIServerToolResult Completed(IReadOnlyDictionary<string, object?> result) => new()
    {
        Status = AIToolCallStatus.Completed,
        Result = result
    };

    public static AIServerToolResult Failed(string code, string message) => new()
    {
        Status = AIToolCallStatus.Failed,
        Error = new Dictionary<string, object?>
        {
            ["code"] = code,
            ["message"] = message
        }
    };
}
```

- [ ] **Step 4: Implement server tool registry**

Create `Materal.MergeBlock/Materal.MergeBlock.AI/Tools/AIServerToolRegistry.cs`:

```csharp
namespace Materal.MergeBlock.AI.Tools;

/// <summary>
/// AI服务端工具注册表
/// </summary>
public class AIServerToolRegistry(IEnumerable<IAIServerTool> tools)
{
    private readonly Dictionary<string, IAIServerTool> _tools = tools.ToDictionary(m => m.Descriptor.Name, StringComparer.Ordinal);

    public IReadOnlyCollection<AIToolDescriptor> Tools => _tools.Values.Select(m => m.Descriptor).ToArray();

    public async Task<AIServerToolResult> ExecuteAsync(
        string name,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        if (!_tools.TryGetValue(name, out IAIServerTool? tool))
        {
            throw new KeyNotFoundException($"未找到AI服务端工具: {name}");
        }
        AIServerToolContext context = new()
        {
            ToolCallId = Guid.NewGuid().ToString("N"),
            Arguments = arguments,
            CancellationToken = cancellationToken
        };
        return await tool.ExecuteAsync(context);
    }
}
```

- [ ] **Step 5: Register server tool registry**

Modify `DIExtensions.AddMergeBlockAI`:

```csharp
services.AddSingleton<AIServerToolRegistry>();
```

- [ ] **Step 6: Run server tool tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIServerToolRegistryTest -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 7: Add Prompt Contributor And Pre-Execution Review Hooks

**Files:**
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Review/IAIAgentPreExecutionReviewer.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Review/AIAgentPreExecutionReviewRequest.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Review/AIAgentPreExecutionReviewResult.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Abstractions/Runtime/AIAgentRunOutput.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Review/AIAgentPreExecutionReviewerTest.cs`

- [ ] **Step 1: Write review result tests**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Test/Review/AIAgentPreExecutionReviewerTest.cs`:

```csharp
namespace Materal.MergeBlock.AI.Test.Review;

[TestClass]
public class AIAgentPreExecutionReviewerTest
{
    [TestMethod]
    public void Approved_ShouldCreateApprovedResult()
    {
        AIAgentPreExecutionReviewResult result = AIAgentPreExecutionReviewResult.Approve("safe");

        Assert.IsTrue(result.Approved);
        Assert.AreEqual("approve", result.Decision);
        Assert.AreEqual("safe", result.Reason);
    }

    [TestMethod]
    public void Rejected_ShouldCreateRejectedResultWithAgentMessage()
    {
        AIAgentPreExecutionReviewResult result = AIAgentPreExecutionReviewResult.Rejected(
            "unsafe",
            "Rewrite the action");

        Assert.IsFalse(result.Approved);
        Assert.AreEqual("reject", result.Decision);
        Assert.AreEqual("unsafe", result.Reason);
        Assert.AreEqual("Rewrite the action", result.AgentErrorMessage);
    }
}
```

- [ ] **Step 2: Run review tests and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIAgentPreExecutionReviewerTest -p:UseSharedCompilation=false
```

Expected: FAIL because review abstractions do not exist.

- [ ] **Step 3: Add review abstractions**

Create `IAIAgentPreExecutionReviewer.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Review;

/// <summary>
/// AI Agent执行前审查器
/// </summary>
public interface IAIAgentPreExecutionReviewer
{
    /// <summary>
    /// 审查工具调用
    /// </summary>
    Task<AIAgentPreExecutionReviewResult> ReviewAsync(AIAgentPreExecutionReviewRequest request);
}
```

Create `AIAgentPreExecutionReviewRequest.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Review;

/// <summary>
/// AI Agent执行前审查请求
/// </summary>
public class AIAgentPreExecutionReviewRequest
{
    public string ThreadId { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public string ToolCallId { get; init; } = string.Empty;
    public string ToolName { get; init; } = string.Empty;
    public string UserMessage { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();
    public AIAgentModelConfig ModelConfig { get; init; } = new();
}
```

Create `AIAgentPreExecutionReviewResult.cs`:

```csharp
namespace Materal.MergeBlock.AI.Abstractions.Review;

/// <summary>
/// AI Agent执行前审查结果
/// </summary>
public class AIAgentPreExecutionReviewResult
{
    public bool Approved { get; init; }
    public string Decision { get; init; } = "reject";
    public string Reason { get; init; } = string.Empty;
    public string AgentErrorMessage { get; init; } = string.Empty;
    public IReadOnlyList<string> Violations { get; init; } = [];

    public static AIAgentPreExecutionReviewResult ApprovedResult(string reason) => Approve(reason);

    public static AIAgentPreExecutionReviewResult Approve(string reason) => new()
    {
        Approved = true,
        Decision = "approve",
        Reason = reason
    };

    public static AIAgentPreExecutionReviewResult Rejected(string reason, string agentErrorMessage, IReadOnlyList<string>? violations = null) => new()
    {
        Approved = false,
        Decision = "reject",
        Reason = reason,
        AgentErrorMessage = agentErrorMessage,
        Violations = violations ?? []
    };
}
```

- [ ] **Step 4: Add review output helper**

Ensure `AIAgentRunOutput.ScriptReviewCompleted` can carry decision, violations, and agent error message through `Metadata`:

```csharp
public static AIAgentRunOutput ScriptReviewCompleted(
    string toolCallId,
    bool approved,
    string reason,
    string? riskLevel = null,
    IReadOnlyDictionary<string, object?>? metadata = null) => new()
{
    Type = AIAgentRunOutputType.ScriptReviewCompleted,
    ToolCallId = toolCallId,
    Approved = approved,
    Reason = reason,
    RiskLevel = riskLevel,
    Metadata = metadata
};
```

- [ ] **Step 5: Run review tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj --filter AIAgentPreExecutionReviewerTest -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 8: Upgrade Demo Runtime To Use Compatible Model Config

**Files:**
- Modify: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/Glm51AIAgentRuntime.cs`
- Modify: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/MafGlm51AgentRunner.cs`
- Modify: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Test/AI/Glm51AIAgentRuntimeTest.cs`
- Create: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Test/AI/AgentWireCompatibilityTest.cs`

- [ ] **Step 1: Add demo runtime test for request-level model config**

Modify `Glm51AIAgentRuntimeTest` with:

```csharp
[TestMethod]
public async Task RunAsync_ShouldUseRequestModelConfig_WhenProvided()
{
    RecordingAgentRunner runner = new("hello");
    Glm51AIAgentRuntime runtime = new(
        Options.Create(new Glm51AIOptions { ApiKey = "config-key" }),
        runner);

    AIAgentRunRequest request = CreateRunRequest("hello") with
    {
        ModelConfig = new AIAgentModelConfig
        {
            Provider = "openai_compatible",
            Model = "request-model",
            ApiKey = "request-key",
            BaseUrl = "https://example.test/v1",
            Temperature = 0.4f,
            MaxTokens = 1024
        }
    };

    List<AIAgentRunOutput> outputs = await CollectAsync(runtime.RunAsync(request));

    Assert.AreEqual(AIAgentRunOutputType.MessageDelta, outputs[0].Type);
    Assert.AreEqual("request-model", runner.LastRequest!.ModelConfig.Model);
    Assert.AreEqual("request-key", runner.LastRequest.ModelConfig.ApiKey);
}
```

Adjust the test helper to create `AIAgentRunRequest` as a mutable object instead of using record `with` if the type is still a class:

```csharp
AIAgentRunRequest request = CreateRunRequest("hello");
request = new AIAgentRunRequest
{
    ThreadId = request.ThreadId,
    RunId = request.RunId,
    Message = request.Message,
    AIContext = request.AIContext,
    SystemMessages = request.SystemMessages,
    CancellationToken = request.CancellationToken,
    ModelConfig = new AIAgentModelConfig
    {
        Provider = "openai_compatible",
        Model = "request-model",
        ApiKey = "request-key",
        BaseUrl = "https://example.test/v1",
        Temperature = 0.4f,
        MaxTokens = 1024
    }
};
```

- [ ] **Step 2: Run demo config test and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.Test\MMB.Demo.Test.csproj --filter RunAsync_ShouldUseRequestModelConfig_WhenProvided -p:UseSharedCompilation=false
```

Expected: FAIL because demo runtime does not use request-level model config.

- [ ] **Step 3: Add model config to Glm51AgentRunRequest**

Modify `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/Glm51AgentRunRequest.cs`:

```csharp
public AIAgentModelConfig ModelConfig { get; init; } = new();
```

- [ ] **Step 4: Pass request model config through Glm51AIAgentRuntime**

When creating `Glm51AgentRunRequest`, set:

```csharp
ModelConfig = request.ModelConfig
```

If `request.ModelConfig.ApiKey` is empty, fall back to existing `Glm51AIOptions` values. If both are empty, return the existing `glm51_api_key_missing` error.

- [ ] **Step 5: Run demo runtime tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.Test\MMB.Demo.Test.csproj --filter Glm51AIAgentRuntimeTest -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 9: Add Runtime Watchdog Wrapper

**Files:**
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentRuntimeWatchdog.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Runtime/AIAgentWatchdogOptions.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Controllers/AIAgentController.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentRuntimeWatchdogTest.cs`

- [ ] **Step 1: Write watchdog test for heartbeat**

Create `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Runtime/AIAgentRuntimeWatchdogTest.cs`:

```csharp
namespace Materal.MergeBlock.AI.Web.Test.Runtime;

[TestClass]
public class AIAgentRuntimeWatchdogTest
{
    [TestMethod]
    public async Task WatchAsync_ShouldEmitHeartbeatWhileWaiting()
    {
        AIAgentRuntimeWatchdog watchdog = new(new AIAgentWatchdogOptions
        {
            HeartbeatInterval = TimeSpan.FromMilliseconds(10),
            IdleTimeout = TimeSpan.FromMilliseconds(100),
            ThinkingOnlyTimeout = TimeSpan.FromMilliseconds(100)
        });

        async IAsyncEnumerable<AIAgentRunOutput> SlowOutputs()
        {
            await Task.Delay(30);
            yield return AIAgentRunOutput.MessageDelta("hello");
            yield return AIAgentRunOutput.RunCompleted();
        }

        List<AIAgentRunOutput> outputs = [];
        await foreach (AIAgentRunOutput output in watchdog.WatchAsync(SlowOutputs(), CancellationToken.None))
        {
            outputs.Add(output);
        }

        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.Heartbeat));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.MessageDelta));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.RunCompleted));
    }

    [TestMethod]
    public async Task WatchAsync_ShouldEmitRecoveryAndError_WhenRuntimeIsIdle()
    {
        AIAgentRuntimeWatchdog watchdog = new(new AIAgentWatchdogOptions
        {
            HeartbeatInterval = TimeSpan.FromMilliseconds(10),
            IdleTimeout = TimeSpan.FromMilliseconds(30),
            ThinkingOnlyTimeout = TimeSpan.FromMilliseconds(100)
        });

        async IAsyncEnumerable<AIAgentRunOutput> IdleOutputs()
        {
            await Task.Delay(200);
            yield return AIAgentRunOutput.MessageDelta("late");
        }

        List<AIAgentRunOutput> outputs = [];
        await foreach (AIAgentRunOutput output in watchdog.WatchAsync(IdleOutputs(), CancellationToken.None))
        {
            outputs.Add(output);
            if (output.Type == AIAgentRunOutputType.Error) break;
        }

        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.Heartbeat));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.RecoveryStarted));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.RecoveryFailed));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.Error));
    }
}
```

- [ ] **Step 2: Run watchdog test and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter AIAgentRuntimeWatchdogTest -p:UseSharedCompilation=false
```

Expected: FAIL because watchdog does not exist.

- [ ] **Step 3: Implement watchdog options**

Create `AIAgentWatchdogOptions.cs`:

```csharp
namespace Materal.MergeBlock.AI.Web.Runtime;

/// <summary>
/// AI Agent运行时看门狗配置
/// </summary>
public class AIAgentWatchdogOptions
{
    public TimeSpan HeartbeatInterval { get; init; } = TimeSpan.FromSeconds(5);
    public TimeSpan IdleTimeout { get; init; } = TimeSpan.FromSeconds(60);
    public TimeSpan ThinkingOnlyTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
```

- [ ] **Step 4: Implement watchdog wrapper**

Create `AIAgentRuntimeWatchdog.cs` with a single responsibility: emit `Heartbeat` while waiting and convert idle timeout to `RecoveryStarted`, `RecoveryFailed`, and `Error` if the wrapped runtime produces no action before timeout.

Implementation requirements:

- Start enumeration of the source `IAsyncEnumerable<AIAgentRunOutput>` on a background task.
- Push runtime outputs into a `Channel<AIAgentRunOutput>`.
- In the public async iterator, wait for either a channel item or `HeartbeatInterval`.
- Emit `Heartbeat` whenever the heartbeat interval elapses before a runtime output arrives.
- If no runtime output arrives before `IdleTimeout`, emit `RecoveryStarted`, then `RecoveryFailed("runtime idle timeout")`, then `Error("Runtime idle timeout.", "runtime_idle_timeout")`, and stop enumeration.
- When a runtime output arrives, reset the idle timer and yield the output.
- If the runtime output type is `ThinkingDelta`, track thinking-only progress separately. If only thinking output arrives for `ThinkingOnlyTimeout`, emit `RecoveryStarted`, then `RecoveryFailed("thinking-only timeout")`, then `Error("Runtime thinking stream timed out.", "runtime_thinking_timeout")`.
- If the source completes normally, complete the iterator without adding an error event.

```csharp
namespace Materal.MergeBlock.AI.Web.Runtime;

/// <summary>
/// AI Agent运行时看门狗
/// </summary>
public class AIAgentRuntimeWatchdog(AIAgentWatchdogOptions options)
{
    public async IAsyncEnumerable<AIAgentRunOutput> WatchAsync(
        IAsyncEnumerable<AIAgentRunOutput> source,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        System.Threading.Channels.Channel<AIAgentRunOutput> channel =
            System.Threading.Channels.Channel.CreateUnbounded<AIAgentRunOutput>();
        Task producer = Task.Run(async () =>
        {
            try
            {
                await foreach (AIAgentRunOutput output in source.WithCancellation(cancellationToken))
                {
                    await channel.Writer.WriteAsync(output, cancellationToken);
                }
                channel.Writer.TryComplete();
            }
            catch (OperationCanceledException)
            {
                channel.Writer.TryComplete();
            }
            catch (Exception exception)
            {
                channel.Writer.TryComplete(exception);
            }
        }, CancellationToken.None);

        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        DateTimeOffset lastOutputAt = startedAt;
        DateTimeOffset lastNonThinkingAt = startedAt;

        while (!cancellationToken.IsCancellationRequested)
        {
            Task<bool> waitForOutputTask = channel.Reader.WaitToReadAsync(cancellationToken).AsTask();
            Task heartbeatTask = Task.Delay(options.HeartbeatInterval, cancellationToken);
            Task completedTask = await Task.WhenAny(waitForOutputTask, heartbeatTask);
            if (completedTask == heartbeatTask)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                yield return AIAgentRunOutput.Heartbeat();
                if (now - lastOutputAt >= options.IdleTimeout)
                {
                    yield return AIAgentRunOutput.RecoveryStarted();
                    yield return AIAgentRunOutput.RecoveryFailed("runtime idle timeout");
                    yield return AIAgentRunOutput.Error("Runtime idle timeout.", "runtime_idle_timeout");
                    yield break;
                }
                if (now - lastNonThinkingAt >= options.ThinkingOnlyTimeout)
                {
                    yield return AIAgentRunOutput.RecoveryStarted();
                    yield return AIAgentRunOutput.RecoveryFailed("thinking-only timeout");
                    yield return AIAgentRunOutput.Error("Runtime thinking stream timed out.", "runtime_thinking_timeout");
                    yield break;
                }
                continue;
            }

            bool hasOutput = await waitForOutputTask;
            if (!hasOutput) break;
            while (channel.Reader.TryRead(out AIAgentRunOutput? output))
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                lastOutputAt = now;
                if (output.Type is not AIAgentRunOutputType.ThinkingDelta)
                {
                    lastNonThinkingAt = now;
                }
                yield return output;
            }
        }

        await producer;
    }
}
```

- [ ] **Step 5: Register watchdog**

Modify `AIWebModule.OnConfigureServices`:

```csharp
context.Services.TryAddSingleton(new AIAgentWatchdogOptions());
context.Services.AddSingleton<AIAgentRuntimeWatchdog>();
```

- [ ] **Step 6: Run watchdog tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter AIAgentRuntimeWatchdogTest -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 10: Improve Checkpoint And Debug Trace Compatibility

**Files:**
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/IAIAgentStateStore.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/SqliteAIAgentStateStore.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/AgentRunTrace.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/AgentTimelineItem.cs`
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/AgentModelConfigSummary.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Persistence/SqliteAIAgentStateStoreTest.cs`

- [ ] **Step 1: Add persistence test for model config summary and timeline**

Modify `SqliteAIAgentStateStoreTest` with:

```csharp
[TestMethod]
public async Task GetRunTraceAsync_ShouldReturnTimelineAndRedactedModelConfig()
{
    string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sqlite3");
    SqliteAIAgentStateStore store = new(path);
    await store.InitializeAsync();
    await store.UpsertSessionAsync("thread_001");
    await store.StartRunAsync("run_001", "thread_001");
    await store.RecordCheckpointAsync(
        "run_001",
        new Dictionary<string, object?> { ["phase"] = "started" },
        new Dictionary<string, object?>
        {
            ["provider"] = "openai",
            ["model"] = "gpt-test",
            ["api_key"] = "secret"
        });
    await store.RecordMessageAsync(new AgentMessageRecord
    {
        Id = "message_001",
        ThreadId = "thread_001",
        RunId = "run_001",
        Role = "user",
        Content = new Dictionary<string, object?> { ["text"] = "hello" }
    });
    await store.RecordStreamEventAsync(new AgentStreamEvent
    {
        ThreadId = "thread_001",
        RunId = "run_001",
        Seq = 1,
        Event = "run.started",
        Payload = new Dictionary<string, object?>()
    });

    AgentRunTrace trace = await store.GetRunTraceAsync("run_001");

    Assert.IsNotNull(trace.Checkpoint);
    Assert.IsNotNull(trace.Checkpoint.ModelConfigSummary);
    Assert.IsFalse(trace.Checkpoint.ModelConfigSummary.ContainsKey("api_key"));
    Assert.IsTrue(trace.Timeline.Count >= 2);
}
```

- [ ] **Step 2: Run trace test and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter GetRunTraceAsync_ShouldReturnTimelineAndRedactedModelConfig -p:UseSharedCompilation=false
```

Expected: FAIL because timeline is absent and model config summary is not redacted.

- [ ] **Step 3: Add AgentTimelineItem**

Create `AgentTimelineItem.cs`:

```csharp
namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// AI Agent时间线项目
/// </summary>
public class AgentTimelineItem
{
    public string Kind { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public int? Seq { get; init; }
    public string? Role { get; init; }
    public string? Event { get; init; }
    public IReadOnlyDictionary<string, object?> Payload { get; init; } = new Dictionary<string, object?>();
}
```

- [ ] **Step 4: Extend AgentRunTrace**

Add:

```csharp
public IReadOnlyList<AgentTimelineItem> Timeline { get; init; } = [];
```

- [ ] **Step 5: Redact model config before checkpoint persistence**

In `SqliteAIAgentStateStore.RecordCheckpointAsync`, remove sensitive keys before serialization:

```csharp
static IReadOnlyDictionary<string, object?>? RedactModelConfig(IReadOnlyDictionary<string, object?>? value)
{
    if (value is null) return null;
    Dictionary<string, object?> result = new(value, StringComparer.OrdinalIgnoreCase);
    result.Remove("api_key");
    result.Remove("apiKey");
    result.Remove("authorization");
    result.Remove("token");
    return result;
}
```

Use `RedactModelConfig(modelConfigSummary)` for `model_config_summary_json`.

- [ ] **Step 6: Build timeline in GetRunTraceAsync**

After loading messages, events, and tool calls, build:

```csharp
List<AgentTimelineItem> timeline = [];
timeline.AddRange(messages.Select(m => new AgentTimelineItem
{
    Kind = "message",
    RunId = m.RunId,
    Role = m.Role,
    Payload = m.Content
}));
timeline.AddRange(events.Select(m => new AgentTimelineItem
{
    Kind = "event",
    RunId = m.RunId,
    Seq = m.Seq,
    Event = m.Event,
    Payload = m.Payload
}));
```

Assign `Timeline = timeline`.

- [ ] **Step 7: Run persistence tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter SqliteAIAgentStateStoreTest -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 11: Add Audit Redaction

**Files:**
- Create: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Persistence/AgentTraceRedactor.cs`
- Modify: `Materal.MergeBlock/Materal.MergeBlock.AI.Web/Controllers/AIAgentController.cs`
- Test: `Materal.MergeBlock/Materal.MergeBlock.AI.Web.Test/Persistence/AgentTraceRedactorTest.cs`

- [ ] **Step 1: Write redactor tests**

Create `AgentTraceRedactorTest.cs`:

```csharp
namespace Materal.MergeBlock.AI.Web.Test.Persistence;

[TestClass]
public class AgentTraceRedactorTest
{
    [TestMethod]
    public void Redact_ShouldRemoveSecretsAndTrimLongText()
    {
        Dictionary<string, object?> payload = new()
        {
            ["api_key"] = "secret",
            ["token"] = "token-value",
            ["text"] = new string('a', 3000),
            ["normal"] = "ok"
        };

        IReadOnlyDictionary<string, object?> result = AgentTraceRedactor.Redact(payload);

        Assert.IsFalse(result.ContainsKey("api_key"));
        Assert.IsFalse(result.ContainsKey("token"));
        Assert.AreEqual("ok", result["normal"]);
        Assert.IsTrue(((string)result["text"]!).Length <= 1024);
    }
}
```

- [ ] **Step 2: Run redactor test and verify failure**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter AgentTraceRedactorTest -p:UseSharedCompilation=false
```

Expected: FAIL because redactor does not exist.

- [ ] **Step 3: Implement redactor**

Create `AgentTraceRedactor.cs`:

```csharp
namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// AI Agent追踪脱敏器
/// </summary>
public static class AgentTraceRedactor
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "api_key",
        "apiKey",
        "authorization",
        "token",
        "password",
        "secret"
    };

    public static IReadOnlyDictionary<string, object?> Redact(IReadOnlyDictionary<string, object?> payload, int maxTextLength = 1024)
    {
        Dictionary<string, object?> result = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, object?> item in payload)
        {
            if (SensitiveKeys.Contains(item.Key)) continue;
            result[item.Key] = item.Value is string text && text.Length > maxTextLength
                ? text[..maxTextLength]
                : item.Value;
        }
        return result;
    }
}
```

- [ ] **Step 4: Use redactor before audit metadata**

In `AIAgentController.AuditAsync` callers, pass redacted metadata:

```csharp
Metadata = AgentTraceRedactor.Redact(output.ToolArguments ?? new Dictionary<string, object?>())
```

For tool results:

```csharp
Metadata = AgentTraceRedactor.Redact(toolResult.Result ?? toolResult.Error ?? new Dictionary<string, object?>())
```

- [ ] **Step 5: Run redactor tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj --filter AgentTraceRedactorTest -p:UseSharedCompilation=false
```

Expected: PASS.

## Task 12: End-To-End Contract Verification In MMB.Demo

**Files:**
- Read: `Materal.MergeBlock/docs/plans/002-AI插件/agent-service-migration/testing-plan.md`
- Create: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Test/AI/AgentWireCompatibilityTest.cs`
- Modify: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Application/AI/Glm51AIAgentRuntime.cs`
- Modify: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.Test/AI/Glm51AIAgentRuntimeTest.cs`
- Modify when needed: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.WebAPI/wwwroot/ai-chat-test.html`

- [ ] **Step 1: Write wire compatibility demo test**

Create `AgentWireCompatibilityTest.cs`:

```csharp
using System.Text.Json;

namespace MMB.Demo.Test.AI;

[TestClass]
public class AgentWireCompatibilityTest
{
    [TestMethod]
    public void Demo_ShouldDeserializeExistingChatContract()
    {
        const string json = """
        {
          "schema_version": "agent-chat-request-v1",
          "thread_id": "demo-thread",
          "message": "hello",
          "model_config": {
            "provider": "openai_compatible",
            "model": "glm-5.1",
            "base_url": "https://api.z.ai/api/paas/v4/",
            "api_key": "secret",
            "temperature": 0.6,
            "max_tokens": 2048
          }
        }
        """;

        AgentChatRequest request = JsonSerializer.Deserialize<AgentChatRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        Assert.AreEqual("demo-thread", request.ThreadId);
        Assert.AreEqual("glm-5.1", request.ModelConfig.Model);
    }
}
```

- [ ] **Step 2: Run demo wire test**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.Test\MMB.Demo.Test.csproj --filter AgentWireCompatibilityTest -p:UseSharedCompilation=false
```

Expected: PASS after Tasks 1 and 2.

- [ ] **Step 3: Run full AI tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj -p:UseSharedCompilation=false
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj -p:UseSharedCompilation=false
dotnet test --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.Test\MMB.Demo.Test.csproj --filter "Glm51AIAgentRuntimeTest|AgentWireCompatibilityTest" -p:UseSharedCompilation=false
```

Expected: PASS.

- [ ] **Step 4: Assess the Demo test page against P0 manual scenarios**

Read `testing-plan.md` and open `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.WebAPI/wwwroot/ai-chat-test.html`.

Create this checklist in the task notes before manual validation:

```text
P0 Basic chat: page can send request, read SSE, assert run.started/message.delta/run.completed.
P0 Model config compatibility: page can send snake_case model_config and show redacted preview.
P0 Remote tool pause: page can assert tool_call.requested and run.paused payload reason/tool_call_ids.
P0 Remote tool resume completed: page can submit matching tool results and assert completion events.
P0 Invalid remote resume: page can submit missing/extra/duplicate/wrong-run/wrong-thread results.
P0 Cancel: page can cancel active run and confirm run.cancelled/status cancelled.
P0 Debug trace: page can fetch run trace and show timeline/checkpoint/tool/review records.
P0 Redaction: page can prove events, trace, audit, and exported report hide secrets.
```

Expected: every P0 item is marked either `supported by current page` or `requires page upgrade`. Any `requires page upgrade` item must be implemented in Step 5 before sign-off.

- [ ] **Step 5: Upgrade `ai-chat-test.html` when the page cannot run P0 scenarios**

If Step 4 finds gaps, update the page with these minimal generic helpers:

```javascript
const redactionKeys = ["api_key", "authorization", "token", "password", "secret"];

function redactValue(key, value) {
    if (redactionKeys.includes(String(key).toLowerCase())) return "[REDACTED]";
    if (Array.isArray(value)) return value.map(item => redactObject(item));
    if (value && typeof value === "object") return redactObject(value);
    return value;
}

function redactObject(value) {
    return Object.fromEntries(Object.entries(value || {}).map(([key, item]) => [key, redactValue(key, item)]));
}

function assertEventSequence(events, expectedEvents) {
    let cursor = 0;
    const results = [];
    for (const expectedEvent of expectedEvents) {
        const index = events.findIndex((item, itemIndex) => itemIndex >= cursor && item.event === expectedEvent);
        results.push({
            name: `event:${expectedEvent}`,
            status: index >= 0 ? "passed" : "failed",
            eventIndex: index,
            message: index >= 0 ? "found" : "missing"
        });
        if (index >= 0) cursor = index + 1;
    }
    return results;
}

function hasPath(value, path) {
    return path.split(".").every(part => {
        if (value == null || !Object.prototype.hasOwnProperty.call(value, part)) return false;
        value = value[part];
        return true;
    });
}

function assertPayloadFields(events, requiredFields) {
    return requiredFields.map(rule => {
        const event = events.find(item => item.event === rule.event);
        const passed = Boolean(event && hasPath(event, rule.path));
        return {
            name: `${rule.event}:${rule.path}`,
            status: passed ? "passed" : "failed",
            eventIndex: event ? events.indexOf(event) : -1,
            message: passed ? "field found" : "field missing"
        };
    });
}
```

Add scenario definitions for the P0 matrix:

```javascript
const testScenarios = {
    basic_chat: {
        message: "hello",
        expectedEvents: ["run.started", "message.delta", "run.completed"],
        requiredFields: [{ event: "message.delta", path: "payload.text" }]
    },
    remote_tool_pause: {
        message: "use-remote-tool",
        expectedEvents: ["run.started", "tool_call.requested", "run.paused"],
        requiredFields: [
            { event: "run.paused", path: "payload.reason" },
            { event: "run.paused", path: "payload.tool_call_ids" }
        ]
    },
    cancel_run: {
        message: "slow-stream",
        expectedEvents: ["run.started", "run.cancelled"],
        requiredFields: [{ event: "run.cancelled", path: "payload.reason" }]
    }
};
```

Add page actions for:

- Building a snake_case request preview with `schema_version`, `thread_id`, `run_id`, `message`, `model_config`, `context`, and `metadata`.
- Sending multi-tool results with completed, failed, and rejected status.
- Sending invalid resume payload variants: missing, extra, duplicate, wrong run, and wrong thread.
- Fetching run status and debug trace with the same authorization header.
- Exporting a redacted report with `thread_id`, `run_id`, `scenario`, `request_preview`, `events`, `assertions`, `run_status`, and `trace_summary`.

Expected: the page remains a single static HTML file and contains no business-domain or client-host-specific scenario.

- [ ] **Step 6: Run MMB.Demo WebAPI manual P0 scenarios**

Start the Demo WebAPI:

```powershell
dotnet run --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.WebAPI\MMB.Demo.WebAPI.csproj
```

Open:

```text
http://localhost:5000/ai-chat-test.html
```

Run every P0 scenario from `testing-plan.md`:

```text
Basic chat
Model config compatibility
Remote tool pause
Remote tool resume completed
Invalid remote resume
Cancel
Debug trace
Redaction
```

Expected: each scenario passes page assertions, and any exported report contains no raw credential values.

- [ ] **Step 7: Record manual verification evidence**

Create or update task notes with this summary:

```text
Manual P0 scenarios:
- Basic chat: PASS/FAIL, run_id:
- Model config compatibility: PASS/FAIL, run_id:
- Remote tool pause: PASS/FAIL, run_id:
- Remote tool resume completed: PASS/FAIL, run_id:
- Invalid remote resume: PASS/FAIL, run_id:
- Cancel: PASS/FAIL, run_id:
- Debug trace: PASS/FAIL, trace_id/run_id:
- Redaction: PASS/FAIL, report exported:

Page upgrades required:
- none
```

If page upgrades were needed, replace `none` with the exact controls or helpers added to `ai-chat-test.html`.

Expected: manual verification evidence is available before Task 13.

## Task 13: Final Verification And Change Detection

**Files:**
- Verify: all files modified in previous tasks.

- [ ] **Step 1: Check CRLF line endings**

Run:

```powershell
$paths = @(
  '.\Materal.MergeBlock\Materal.MergeBlock.AI.Abstractions',
  '.\Materal.MergeBlock\Materal.MergeBlock.AI',
  '.\Materal.MergeBlock\Materal.MergeBlock.AI.Web',
  '.\Materal.MergeBlock\Materal.MergeBlock.AI.Test',
  '.\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test',
  '.\Materal.MergeBlock\MMB\MMB.Demo'
)
foreach ($path in $paths) {
  Get-ChildItem -LiteralPath $path -Recurse -File | Where-Object Extension -in '.cs','.csproj','.json','.md' | ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    $badLf = 0
    for ($i = 0; $i -lt $bytes.Length; $i++) {
      if ($bytes[$i] -eq 10 -and ($i -eq 0 -or $bytes[$i - 1] -ne 13)) { $badLf++ }
    }
    if ($badLf -gt 0) { throw "发现 LF 行尾: $($_.FullName)" }
  }
}
```

Expected: No exception.

- [ ] **Step 2: Run target tests**

Run:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj -p:UseSharedCompilation=false
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj -p:UseSharedCompilation=false
dotnet test --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.Test\MMB.Demo.Test.csproj --filter "Glm51AIAgentRuntimeTest|AgentWireCompatibilityTest" -p:UseSharedCompilation=false
```

Expected: PASS.

- [ ] **Step 3: Build MMB Demo**

Run:

```powershell
dotnet build .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.slnx -p:UseSharedCompilation=false
```

Expected: Build succeeds.

- [ ] **Step 4: Run GitNexus detect changes**

Run GitNexus change detection for Materal:

```text
detect_changes repo=Materal scope=all
```

Expected: Changed symbols are limited to AI abstractions, AI Web host, AI tests, and MMB.Demo AI validation. Report affected symbols and execution flows before any commit.

- [ ] **Step 5: Report completion summary**

Report:

- Compatibility DTO tests passed.
- Runtime request mapping tests passed.
- Remote tool pause/resume tests passed.
- Server-executed tool tests passed.
- Review gate abstractions tests passed.
- Watchdog tests passed.
- Checkpoint/debug trace tests passed.
- Audit redaction tests passed.
- Demo compatibility tests passed.

## Commit Checkpoints

Do not commit unless the user explicitly asks. If the user asks to commit after implementation, use Chinese commit messages and keep checkpoints small:

```powershell
git add .\Materal.MergeBlock\Materal.MergeBlock.AI.Abstractions .\Materal.MergeBlock\Materal.MergeBlock.AI.Test
git commit -m "feat: 完善AI运行时通用抽象"
```

```powershell
git add .\Materal.MergeBlock\Materal.MergeBlock.AI.Web .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test
git commit -m "feat: 兼容Agent服务Web契约"
```

```powershell
git add .\Materal.MergeBlock\MMB\MMB.Demo
git commit -m "test: 增加AI迁移运行时验证"
```

## Self-Review

- Spec coverage: All blocking items B1-B5 are covered before major gaps M1-M5. Ordinary gaps N1-N3 are covered by trace, cancel, and redaction tasks.
- Completeness scan: The plan avoids unresolved gaps and gives concrete files, commands, and expected results.
- Type consistency: Runtime model names use `AIAgent*`; Web DTOs keep external snake_case wire names through `JsonPropertyName`; persistence models keep existing `Agent*` names.
- Scope check: This plan stays generic. Business-domain tools and client-host-specific behavior belong in separate business module plans.
