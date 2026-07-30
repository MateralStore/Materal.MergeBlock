using Materal.MergeBlock.AI.Abstractions.Context;
using Materal.MergeBlock.AI.Abstractions.Runtime;
using Materal.MergeBlock.AI.Context;
using Materal.MergeBlock.AI.Prompts;
using Materal.MergeBlock.AI.Web.Cancellation;
using Materal.MergeBlock.AI.Web.Controllers;
using Materal.MergeBlock.AI.Web.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Materal.MergeBlock.AI.Web.Test.Runtime;

[TestClass]
public class AIAgentControllerRuntimeTest
{
    [TestMethod]
    public async Task StreamAsync_ShouldWriteStartedAndRuntimeEvents()
    {
        RecordingStateStore stateStore = new();
        StubRuntime runtime = new([
            AIAgentRunOutput.MessageDelta("hello"),
            AIAgentRunOutput.RunCompleted()
        ]);
        AIAgentController controller = CreateController(stateStore, runtime);

        await controller.StreamAsync(new AgentChatRequest
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Message = "hi"
        });

        string sse = ReadResponse(controller);
        StringAssert.Contains(sse, "event: run.started");
        StringAssert.Contains(sse, "event: message.delta");
        StringAssert.Contains(sse, "event: run.completed");
        Assert.AreEqual(AgentRunStatus.Completed, stateStore.CompletedStatuses["run_001"]);
    }

    [TestMethod]
    public async Task StreamAsync_ShouldPersistToolCallAndPauseRun()
    {
        RecordingStateStore stateStore = new();
        StubRuntime runtime = new([
            AIAgentRunOutput.ToolCallRequested("call_001", "runClientAction", new Dictionary<string, object?> { ["kind"] = "demo" }),
            AIAgentRunOutput.RunPaused()
        ]);
        AIAgentController controller = CreateController(stateStore, runtime);

        await controller.StreamAsync(new AgentChatRequest
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Message = "use-remote-tool"
        });

        Assert.AreEqual(1, stateStore.ToolCalls.Count);
        Assert.AreEqual("call_001", stateStore.ToolCalls[0].ToolCallId);
        Assert.AreEqual(AgentRunStatus.WaitingToolResult, stateStore.CompletedStatuses["run_001"]);
        CollectionAssert.Contains(stateStore.Audits.Select(m => m.Status).ToList(), AIToolCallStatus.Requested);
    }

    [TestMethod]
    public async Task StreamAsync_ShouldPassModelConfigToRuntime()
    {
        RecordingStateStore stateStore = new();
        StubRuntime runtime = new([
            AIAgentRunOutput.RunCompleted()
        ]);
        AIAgentController controller = CreateController(stateStore, runtime);

        await controller.StreamAsync(new AgentChatRequest
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Message = "hi",
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
        });

        Assert.IsNotNull(runtime.LastRunRequest);
        Assert.AreEqual("openai", runtime.LastRunRequest.ModelConfig.Provider);
        Assert.AreEqual("gpt-test", runtime.LastRunRequest.ModelConfig.Model);
        Assert.AreEqual("analysis", runtime.LastRunRequest.SkillRequest!.Name);
        Assert.IsTrue(runtime.LastRunRequest.PreExecutionReview.Enabled);
    }

    [TestMethod]
    public async Task StreamAsync_ShouldReturnError_WhenRuntimeIsMissing()
    {
        RecordingStateStore stateStore = new();
        AIAgentController controller = CreateController(stateStore, null);

        await controller.StreamAsync(new AgentChatRequest
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Message = "hi"
        });

        string sse = ReadResponse(controller);
        StringAssert.Contains(sse, "event: error");
        StringAssert.Contains(sse, "IAIAgentRuntime");
        Assert.AreEqual(AgentRunStatus.Failed, stateStore.CompletedStatuses["run_001"]);
    }

    [TestMethod]
    public async Task ResumeAsync_ShouldRecordToolResultsAndContinueRuntime()
    {
        RecordingStateStore stateStore = new();
        stateStore.Trace = new AgentRunTrace
        {
            Run = new AgentRunRecord { RunId = "run_001", ThreadId = "thread_001", Status = AgentRunStatus.WaitingToolResult },
            ToolCalls =
            [
                new RemoteToolPendingCall
                {
                    ToolCallId = "call_001",
                    ThreadId = "thread_001",
                    RunId = "run_001",
                    ToolName = "runClientAction",
                    Status = AIToolCallStatus.Requested
                }
            ]
        };
        StubRuntime runtime = new([], [
            AIAgentRunOutput.MessageDelta("Remote tool completed"),
            AIAgentRunOutput.RunCompleted()
        ]);
        AIAgentController controller = CreateController(stateStore, runtime);

        await controller.ResumeAsync(new RemoteToolResultsRequest
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            ToolResults =
            [
                new RemoteToolResultItem
                {
                    ToolCallId = "call_001",
                    Status = AIToolCallStatus.Completed,
                    Result = new Dictionary<string, object?> { ["ok"] = true }
                }
            ]
        });

        string sse = ReadResponse(controller);
        StringAssert.Contains(sse, "event: tool_result.completed");
        StringAssert.Contains(sse, "event: message.delta");
        StringAssert.Contains(sse, "event: run.completed");
        Assert.AreEqual(1, stateStore.ToolResults.Count);
        Assert.AreEqual(AIToolCallStatus.Completed, stateStore.Events.Single(m => m.Event == "tool_result.completed").Payload["status"]);
        Assert.AreEqual(AgentRunStatus.Completed, stateStore.CompletedStatuses["run_001"]);
        CollectionAssert.Contains(stateStore.Audits.Select(m => m.Status).ToList(), AIToolCallStatus.Completed);
    }

    [TestMethod]
    public async Task QueryEndpoints_ShouldMapWaitingToolResultToPublicPaused()
    {
        RecordingStateStore stateStore = new()
        {
            Trace = new AgentRunTrace
            {
                Run = new AgentRunRecord
                {
                    RunId = "run_001",
                    ThreadId = "thread_001",
                    Status = AgentRunStatus.WaitingToolResult
                }
            }
        };
        AIAgentController controller = CreateController(stateStore, null);

        AgentRunRecord run = await controller.GetRunAsync("run_001");
        AgentSessionTrace session = await controller.GetSessionAsync("thread_001");
        IReadOnlyList<AgentDebugTraceSummary> debugTraces = await controller.GetDebugTracesAsync();
        AgentRunTrace debugTrace = await controller.GetDebugTraceAsync("run_001");

        Assert.AreEqual(AgentRunStatus.Paused, run.Status);
        Assert.AreEqual(AgentRunStatus.Paused, session.Runs[0].Status);
        Assert.AreEqual(AgentRunStatus.Paused, debugTraces[0].Status);
        Assert.AreEqual(AgentRunStatus.Paused, debugTrace.Run.Status);
    }

    [TestMethod]
    public async Task CancelAsync_ShouldPersistCancelledEvent()
    {
        RecordingStateStore stateStore = new()
        {
            Trace = new AgentRunTrace
            {
                Run = new AgentRunRecord
                {
                    RunId = "run_001",
                    ThreadId = "thread_001",
                    Status = AgentRunStatus.Running
                },
                Events =
                [
                    new AgentStreamEvent
                    {
                        ThreadId = "thread_001",
                        RunId = "run_001",
                        Seq = 1,
                        Event = "run.started",
                        Payload = new Dictionary<string, object?>()
                    }
                ]
            }
        };
        AIAgentController controller = CreateController(stateStore, null);

        await controller.CancelAsync("run_001", new CancelAgentRunRequest
        {
            ThreadId = "thread_001",
            Source = "test",
            Reason = "user_requested"
        });

        AgentStreamEvent cancelledEvent = stateStore.Events.Single(m => m.Event == "run.cancelled");
        Assert.AreEqual(2, cancelledEvent.Seq);
        Assert.AreEqual("user_requested", cancelledEvent.Payload["reason"]);
        Assert.AreEqual("test", cancelledEvent.Payload["source"]);
        Assert.AreEqual(AgentRunStatus.Cancelled, stateStore.CompletedStatuses["run_001"]);
    }

    private static AIAgentController CreateController(RecordingStateStore stateStore, IAIAgentRuntime? runtime)
    {
        ServiceCollection services = new();
        services.AddSingleton<IServiceProvider>(sp => sp);
        services.AddSingleton<IAIToolCallAuditor>(stateStore);
        if (runtime is not null)
        {
            services.AddSingleton(runtime);
        }
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        AIAgentController controller = new(
            stateStore,
            new RemoteToolGateway(stateStore),
            new AIAgentCancellationRegistry(),
            new AIAgentRuntimeRequestFactory(new AIContextBuilder(serviceProvider, []), new AIPromptBuilder([])),
            new AIAgentStreamAdapter(),
            serviceProvider.GetServices<IAIToolCallAuditor>(),
            serviceProvider);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Response =
                {
                    Body = new MemoryStream()
                }
            }
        };
        return controller;
    }

    private static string ReadResponse(AIAgentController controller)
    {
        MemoryStream stream = (MemoryStream)controller.Response.Body;
        stream.Position = 0;
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private sealed class StubRuntime(IReadOnlyList<AIAgentRunOutput> runOutputs, IReadOnlyList<AIAgentRunOutput>? resumeOutputs = null) : IAIAgentRuntime
    {
        public AIAgentRunRequest? LastRunRequest { get; private set; }
        public AIAgentResumeRequest? LastResumeRequest { get; private set; }

        public async IAsyncEnumerable<AIAgentRunOutput> RunAsync(AIAgentRunRequest request)
        {
            LastRunRequest = request;
            foreach (AIAgentRunOutput output in runOutputs)
            {
                yield return output;
                await Task.Yield();
            }
        }

        public async IAsyncEnumerable<AIAgentRunOutput> ResumeAsync(AIAgentResumeRequest request)
        {
            LastResumeRequest = request;
            foreach (AIAgentRunOutput output in resumeOutputs ?? [])
            {
                yield return output;
                await Task.Yield();
            }
        }
    }

    private sealed class RecordingStateStore : IAIAgentStateStore, IAIToolCallAuditor
    {
        public AgentRunTrace Trace { get; set; } = new();
        public List<AgentStreamEvent> Events { get; } = [];
        public List<RemoteToolPendingCall> ToolCalls { get; } = [];
        public List<RemoteToolResultItem> ToolResults { get; } = [];
        public List<AgentMessageRecord> Messages { get; } = [];
        public List<ScriptReviewResult> ScriptReviews { get; } = [];
        public List<AgentCheckpointRecord> Checkpoints { get; } = [];
        public List<AIToolCallAuditContext> Audits { get; } = [];
        public Dictionary<string, string> CompletedStatuses { get; } = new(StringComparer.Ordinal);
        public Task InitializeAsync() => Task.CompletedTask;
        public Task UpsertSessionAsync(string threadId) => Task.CompletedTask;
        public Task StartRunAsync(string runId, string threadId) => Task.CompletedTask;
        public Task CompleteRunAsync(string runId, string status, string? errorMessage = null)
        {
            CompletedStatuses[runId] = status;
            return Task.CompletedTask;
        }
        public Task RecordStreamEventAsync(AgentStreamEvent streamEvent)
        {
            Events.Add(streamEvent);
            return Task.CompletedTask;
        }
        public Task RecordToolCallAsync(RemoteToolPendingCall toolCall)
        {
            ToolCalls.Add(toolCall);
            return Task.CompletedTask;
        }
        public Task RecordToolResultAsync(RemoteToolResultItem toolResult)
        {
            ToolResults.Add(toolResult);
            return Task.CompletedTask;
        }
        public Task RecordMessageAsync(AgentMessageRecord message)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
        public Task RecordScriptReviewAsync(ScriptReviewResult scriptReviewResult)
        {
            ScriptReviews.Add(scriptReviewResult);
            return Task.CompletedTask;
        }
        public Task RecordCheckpointAsync(string runId, IReadOnlyDictionary<string, object?> metadata, IReadOnlyDictionary<string, object?>? modelConfigSummary = null)
        {
            Checkpoints.Add(new AgentCheckpointRecord
            {
                RunId = runId,
                Metadata = metadata,
                ModelConfigSummary = modelConfigSummary
            });
            return Task.CompletedTask;
        }
        public Task<AgentSessionTrace> GetSessionTraceAsync(string threadId) => Task.FromResult(new AgentSessionTrace
        {
            ThreadId = threadId,
            Runs = [Trace.Run]
        });
        public Task<AgentRunRecord> GetRunAsync(string runId) => Task.FromResult(Trace.Run);
        public Task<IReadOnlyList<AgentDebugTraceSummary>> ListDebugTracesAsync() => Task.FromResult<IReadOnlyList<AgentDebugTraceSummary>>(
        [
            new AgentDebugTraceSummary
            {
                TraceId = Trace.Run.RunId,
                RunId = Trace.Run.RunId,
                ThreadId = Trace.Run.ThreadId,
                Status = Trace.Run.Status,
                ErrorMessage = Trace.Run.ErrorMessage
            }
        ]);
        public Task<AgentRunTrace> GetRunTraceAsync(string runId) => Task.FromResult(Trace);
        public Task AuditAsync(AIToolCallAuditContext context)
        {
            Audits.Add(context);
            return Task.CompletedTask;
        }
    }
}
