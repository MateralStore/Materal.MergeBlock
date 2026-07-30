using Materal.MergeBlock.AI.Web.Cancellation;
using Microsoft.AspNetCore.Http.Features;

namespace Materal.MergeBlock.AI.Web.Controllers;

/// <summary>
/// AI Agent控制器
/// </summary>
[ApiController]
[Route("agent")]
public class AIAgentController(
    IAIAgentStateStore stateStore,
    RemoteToolGateway remoteToolGateway,
    AIAgentCancellationRegistry cancellationRegistry,
    AIAgentRuntimeRequestFactory runtimeRequestFactory,
    AIAgentStreamAdapter streamAdapter,
    IEnumerable<IAIToolCallAuditor> toolCallAuditors,
    IServiceProvider serviceProvider) : ControllerBase
{
    /// <summary>
    /// 流式对话
    /// </summary>
    [HttpPost("chat/stream")]
    public async Task StreamAsync(AgentChatRequest request)
    {
        string threadId = string.IsNullOrWhiteSpace(request.ThreadId) ? Guid.NewGuid().ToString("N") : request.ThreadId;
        string runId = string.IsNullOrWhiteSpace(request.RunId) ? Guid.NewGuid().ToString("N") : request.RunId;
        await stateStore.InitializeAsync();
        await stateStore.UpsertSessionAsync(threadId);
        await stateStore.StartRunAsync(runId, threadId);
        await stateStore.RecordMessageAsync(new AgentMessageRecord
        {
            Id = $"message_{runId}_user",
            ThreadId = threadId,
            RunId = runId,
            Role = "user",
            Content = new Dictionary<string, object?> { ["text"] = request.Message }
        });
        await stateStore.RecordCheckpointAsync(runId, new Dictionary<string, object?>
        {
            ["thread_id"] = threadId,
            ["run_id"] = runId,
            ["phase"] = "started"
        });
        CancellationToken cancellationToken = cancellationRegistry.Register(runId);
        int seq = 1;
        AgentStreamEvent streamEvent = new()
        {
            ThreadId = threadId,
            RunId = runId,
            Seq = seq,
            Event = "run.started",
            Payload = new Dictionary<string, object?>
            {
                ["message"] = request.Message
            }
        };
        await stateStore.RecordStreamEventAsync(streamEvent);
        await WriteStreamEventAsync(streamEvent);
        IAIAgentRuntime? runtime = serviceProvider.GetService<IAIAgentRuntime>();
        if (runtime is null)
        {
            AgentStreamEvent errorEvent = streamAdapter.ToStreamEvent(threadId, runId, ++seq, AIAgentRunOutput.Error("未注册IAIAgentRuntime，无法调用真实Agent运行时。", "runtime_not_registered"));
            await stateStore.RecordStreamEventAsync(errorEvent);
            await WriteStreamEventAsync(errorEvent);
            await stateStore.CompleteRunAsync(runId, AgentRunStatus.Failed, "未注册IAIAgentRuntime");
            cancellationRegistry.Complete(runId);
            return;
        }
        try
        {
            AIAgentRunRequest runRequest = await runtimeRequestFactory.CreateRunRequestAsync(request, threadId, runId, cancellationToken);
            IAsyncEnumerable<AIAgentRunOutput> outputs = WatchRuntime(runtime.RunAsync(runRequest), cancellationToken);
            await foreach (AIAgentRunOutput output in outputs.WithCancellation(cancellationToken))
            {
                AgentStreamEvent runtimeEvent = streamAdapter.ToStreamEvent(threadId, runId, ++seq, output);
                await PersistRuntimeOutputAsync(runtimeEvent, output);
                await WriteStreamEventAsync(runtimeEvent);
                if (output.Type is AIAgentRunOutputType.RunCompleted)
                {
                    await stateStore.CompleteRunAsync(runId, AgentRunStatus.Completed);
                }
                else if (output.Type is AIAgentRunOutputType.RunPaused)
                {
                    await stateStore.CompleteRunAsync(runId, AgentRunStatus.WaitingToolResult);
                }
                else if (output.Type is AIAgentRunOutputType.Error)
                {
                    await stateStore.CompleteRunAsync(runId, AgentRunStatus.Failed, output.ErrorMessage);
                }
            }
        }
        catch (Exception exception)
        {
            AgentStreamEvent errorEvent = streamAdapter.ToStreamEvent(threadId, runId, ++seq, AIAgentRunOutput.Error(exception.Message, "runtime_exception"));
            await stateStore.RecordStreamEventAsync(errorEvent);
            await WriteStreamEventAsync(errorEvent);
            await stateStore.CompleteRunAsync(runId, AgentRunStatus.Failed, exception.Message);
        }
        finally
        {
            cancellationRegistry.Complete(runId);
        }
    }
    /// <summary>
    /// 恢复远程工具调用
    /// </summary>
    [HttpPost("chat/resume/stream")]
    public async Task ResumeAsync(RemoteToolResultsRequest request)
    {
        await stateStore.InitializeAsync();
        await remoteToolGateway.ValidateResumeAsync(request);
        foreach (RemoteToolResultItem toolResult in request.ToolResults)
        {
            await stateStore.RecordToolResultAsync(toolResult);
        }
        AgentRunTrace trace = await stateStore.GetRunTraceAsync(request.RunId);
        await AuditToolResultsAsync(request, trace);
        IAIAgentRuntime? runtime = serviceProvider.GetService<IAIAgentRuntime>();
        int seq = trace.Events.Count;
        foreach (RemoteToolResultItem toolResult in request.ToolResults)
        {
            AgentStreamEvent streamEvent = streamAdapter.ToStreamEvent(request.ThreadId, request.RunId, ++seq, AIAgentRunOutput.ToolResultCompleted(toolResult.ToolCallId, toolResult.Status, toolResult.Result, toolResult.Error));
            await stateStore.RecordStreamEventAsync(streamEvent);
            await WriteStreamEventAsync(streamEvent);
        }
        if (runtime is null)
        {
            AgentStreamEvent errorEvent = streamAdapter.ToStreamEvent(request.ThreadId, request.RunId, ++seq, AIAgentRunOutput.Error("未注册IAIAgentRuntime，无法恢复Agent运行时。", "runtime_not_registered"));
            await stateStore.RecordStreamEventAsync(errorEvent);
            await WriteStreamEventAsync(errorEvent);
            await stateStore.CompleteRunAsync(request.RunId, AgentRunStatus.Failed, "未注册IAIAgentRuntime");
            return;
        }
        try
        {
            AgentChatRequest baseRequest = new()
            {
                ThreadId = request.ThreadId,
                RunId = request.RunId,
            };
            AIAgentResumeRequest resumeRequest = await runtimeRequestFactory.CreateResumeRequestAsync(baseRequest, request, HttpContext.RequestAborted);
            IAsyncEnumerable<AIAgentRunOutput> outputs = WatchRuntime(runtime.ResumeAsync(resumeRequest), HttpContext.RequestAborted);
            await foreach (AIAgentRunOutput output in outputs.WithCancellation(HttpContext.RequestAborted))
            {
                AgentStreamEvent runtimeEvent = streamAdapter.ToStreamEvent(request.ThreadId, request.RunId, ++seq, output);
                await PersistRuntimeOutputAsync(runtimeEvent, output);
                await WriteStreamEventAsync(runtimeEvent);
                if (output.Type is AIAgentRunOutputType.RunCompleted)
                {
                    await stateStore.CompleteRunAsync(request.RunId, AgentRunStatus.Completed);
                }
                else if (output.Type is AIAgentRunOutputType.RunPaused)
                {
                    await stateStore.CompleteRunAsync(request.RunId, AgentRunStatus.WaitingToolResult);
                }
                else if (output.Type is AIAgentRunOutputType.Error)
                {
                    await stateStore.CompleteRunAsync(request.RunId, AgentRunStatus.Failed, output.ErrorMessage);
                }
            }
        }
        catch (Exception exception)
        {
            AgentStreamEvent errorEvent = streamAdapter.ToStreamEvent(request.ThreadId, request.RunId, ++seq, AIAgentRunOutput.Error(exception.Message, "runtime_exception"));
            await stateStore.RecordStreamEventAsync(errorEvent);
            await WriteStreamEventAsync(errorEvent);
            await stateStore.CompleteRunAsync(request.RunId, AgentRunStatus.Failed, exception.Message);
        }
    }
    /// <summary>
    /// 取消运行
    /// </summary>
    [HttpPost("runs/{runId}/cancel")]
    public async Task<IActionResult> CancelAsync(string runId, CancelAgentRunRequest request)
    {
        await stateStore.InitializeAsync();
        cancellationRegistry.Cancel(runId);
        await RecordCancelledEventAsync(runId, request);
        await stateStore.CompleteRunAsync(runId, AgentRunStatus.Cancelled, $"{request.Source}:{request.Reason}");
        return Ok();
    }
    /// <summary>
    /// 获取会话
    /// </summary>
    [HttpGet("sessions/{threadId}")]
    public async Task<AgentSessionTrace> GetSessionAsync(string threadId)
    {
        await stateStore.InitializeAsync();
        AgentSessionTrace session = await stateStore.GetSessionTraceAsync(threadId);
        return AgentRunStatusMapper.ToPublicSession(session);
    }
    /// <summary>
    /// 获取运行
    /// </summary>
    [HttpGet("runs/{runId}")]
    public async Task<AgentRunRecord> GetRunAsync(string runId)
    {
        await stateStore.InitializeAsync();
        AgentRunRecord run = await stateStore.GetRunAsync(runId);
        return AgentRunStatusMapper.ToPublicRun(run);
    }
    /// <summary>
    /// 获取调试追踪列表
    /// </summary>
    [HttpGet("debug-traces")]
    public async Task<IReadOnlyList<AgentDebugTraceSummary>> GetDebugTracesAsync()
    {
        await stateStore.InitializeAsync();
        IReadOnlyList<AgentDebugTraceSummary> traces = await stateStore.ListDebugTracesAsync();
        return [.. traces.Select(AgentRunStatusMapper.ToPublicDebugTrace)];
    }
    /// <summary>
    /// 获取调试追踪
    /// </summary>
    [HttpGet("debug-traces/{traceId}")]
    public async Task<AgentRunTrace> GetDebugTraceAsync(string traceId)
    {
        await stateStore.InitializeAsync();
        AgentRunTrace trace = await stateStore.GetRunTraceAsync(traceId);
        return AgentRunStatusMapper.ToPublicTrace(trace);
    }
    /// <summary>
    /// 获取Skill目录
    /// </summary>
    [HttpGet("skills")]
    public AgentSkillCatalogResponse GetSkills()
    {
        AgentSkillCatalogItem[] skills = [.. serviceProvider.GetServices<IAIAgentSkillCatalogProvider>().SelectMany(m => m.GetSkills())];
        return new AgentSkillCatalogResponse
        {
            Skills = skills
        };
    }
    private async Task WriteStreamEventAsync(AgentStreamEvent streamEvent)
    {
        if (!Response.HasStarted)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
            await Response.StartAsync(HttpContext.RequestAborted);
        }
        byte[] bytes = Encoding.UTF8.GetBytes(SseEventWriter.Format(streamEvent));
        await Response.Body.WriteAsync(bytes, HttpContext.RequestAborted);
        await Response.Body.FlushAsync(HttpContext.RequestAborted);
    }
    private IAsyncEnumerable<AIAgentRunOutput> WatchRuntime(IAsyncEnumerable<AIAgentRunOutput> source, CancellationToken cancellationToken)
    {
        AIAgentRuntimeWatchdog? watchdog = serviceProvider.GetService<AIAgentRuntimeWatchdog>();
        return watchdog is null ? source : watchdog.WatchAsync(source, cancellationToken);
    }
    private async Task RecordCancelledEventAsync(string runId, CancelAgentRunRequest request)
    {
        string threadId = request.ThreadId;
        int seq = 1;
        try
        {
            AgentRunTrace trace = await stateStore.GetRunTraceAsync(runId);
            if (string.IsNullOrWhiteSpace(threadId))
            {
                threadId = trace.Run.ThreadId;
            }
            seq = trace.Events.Count + 1;
        }
        catch (KeyNotFoundException)
        {
            threadId = string.IsNullOrWhiteSpace(threadId) ? string.Empty : threadId;
        }
        AgentStreamEvent streamEvent = streamAdapter.ToStreamEvent(threadId, runId, seq, AIAgentRunOutput.RunCancelled(request.Reason, request.Source));
        await stateStore.RecordStreamEventAsync(streamEvent);
    }
    private async Task PersistRuntimeOutputAsync(AgentStreamEvent streamEvent, AIAgentRunOutput output)
    {
        await stateStore.RecordStreamEventAsync(streamEvent);
        if (output.Type is AIAgentRunOutputType.MessageDelta && !string.IsNullOrEmpty(output.Text))
        {
            await stateStore.RecordMessageAsync(new AgentMessageRecord
            {
                Id = $"message_{streamEvent.RunId}_{streamEvent.Seq}",
                ThreadId = streamEvent.ThreadId,
                RunId = streamEvent.RunId,
                Role = "assistant",
                Content = new Dictionary<string, object?> { ["text"] = output.Text }
            });
        }
        if (output.Type is AIAgentRunOutputType.ScriptReviewCompleted)
        {
            await stateStore.RecordScriptReviewAsync(new ScriptReviewResult
            {
                Id = $"script_review_{streamEvent.RunId}_{streamEvent.Seq}",
                ThreadId = streamEvent.ThreadId,
                RunId = streamEvent.RunId,
                ToolCallId = output.ToolCallId ?? string.Empty,
                Approved = output.Approved ?? false,
                Reason = output.Reason,
                RiskLevel = output.RiskLevel
            });
        }
        if (output.Type is AIAgentRunOutputType.RunPaused)
        {
            await stateStore.RecordCheckpointAsync(streamEvent.RunId, new Dictionary<string, object?>
            {
                ["thread_id"] = streamEvent.ThreadId,
                ["run_id"] = streamEvent.RunId,
                ["phase"] = "paused",
                ["seq"] = streamEvent.Seq
            });
        }
        if (output.Type is not AIAgentRunOutputType.ToolCallRequested) return;
        await stateStore.RecordToolCallAsync(new RemoteToolPendingCall
        {
            ToolCallId = output.ToolCallId ?? Guid.NewGuid().ToString("N"),
            ThreadId = streamEvent.ThreadId,
            RunId = streamEvent.RunId,
            ToolName = output.ToolName ?? string.Empty,
            Status = AIToolCallStatus.Requested,
            Arguments = output.ToolArguments
        });
        await AuditAsync(new AIToolCallAuditContext
        {
            ToolName = output.ToolName ?? string.Empty,
            ExecutionMode = AIToolExecutionMode.Remote,
            ThreadId = streamEvent.ThreadId,
            RunId = streamEvent.RunId,
            Status = AIToolCallStatus.Requested,
            Metadata = AgentTraceRedactor.Redact(output.ToolArguments ?? new Dictionary<string, object?>())
        });
    }
    private async Task AuditToolResultsAsync(RemoteToolResultsRequest request, AgentRunTrace trace)
    {
        Dictionary<string, RemoteToolPendingCall> pendingCalls = trace.ToolCalls.ToDictionary(m => m.ToolCallId, StringComparer.Ordinal);
        foreach (RemoteToolResultItem toolResult in request.ToolResults)
        {
            pendingCalls.TryGetValue(toolResult.ToolCallId, out RemoteToolPendingCall? pendingCall);
            await AuditAsync(new AIToolCallAuditContext
            {
                ToolName = pendingCall?.ToolName ?? string.Empty,
                ExecutionMode = AIToolExecutionMode.Remote,
                ThreadId = request.ThreadId,
                RunId = request.RunId,
                Status = toolResult.Status,
                Metadata = AgentTraceRedactor.Redact(toolResult.Result ?? toolResult.Error ?? new Dictionary<string, object?>())
            });
        }
    }
    private async Task AuditAsync(AIToolCallAuditContext context)
    {
        foreach (IAIToolCallAuditor auditor in toolCallAuditors)
        {
            await auditor.AuditAsync(context);
        }
    }
}
