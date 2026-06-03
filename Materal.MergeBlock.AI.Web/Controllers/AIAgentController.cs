using Materal.MergeBlock.AI.Web.Cancellation;
using Microsoft.AspNetCore.Http.Features;

namespace Materal.MergeBlock.AI.Web.Controllers;

/// <summary>
/// AI Agent控制器
/// </summary>
[ApiController]
[Route("agent/chat")]
public class AIAgentController(
    IAIAgentStateStore stateStore,
    RemoteToolGateway remoteToolGateway,
    AIAgentCancellationRegistry cancellationRegistry,
    AIContextBuilder contextBuilder,
    AIPromptBuilder promptBuilder,
    AIAgentStreamAdapter streamAdapter,
    IEnumerable<IAIToolCallAuditor> toolCallAuditors,
    IServiceProvider serviceProvider) : ControllerBase
{
    /// <summary>
    /// 流式对话
    /// </summary>
    [HttpPost("stream")]
    public async Task StreamAsync(AgentChatRequest request)
    {
        string threadId = string.IsNullOrWhiteSpace(request.ThreadId) ? Guid.NewGuid().ToString("N") : request.ThreadId;
        string runId = string.IsNullOrWhiteSpace(request.RunId) ? Guid.NewGuid().ToString("N") : request.RunId;
        await stateStore.InitializeAsync();
        await stateStore.UpsertSessionAsync(threadId);
        await stateStore.StartRunAsync(runId, threadId);
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
            IReadOnlyAIContext aiContext = await contextBuilder.BuildAsync();
            IReadOnlyList<string> systemMessages = await promptBuilder.BuildSystemMessagesAsync(aiContext);
            AIAgentRunRequest runRequest = new()
            {
                ThreadId = threadId,
                RunId = runId,
                Message = request.Message,
                AIContext = aiContext,
                SystemMessages = systemMessages,
                CancellationToken = cancellationToken
            };
            await foreach (AIAgentRunOutput output in runtime.RunAsync(runRequest).WithCancellation(cancellationToken))
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
    [HttpPost("resume/stream")]
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
        int seq = trace.Events.Count + 1;
        AgentStreamEvent streamEvent = new()
        {
            ThreadId = request.ThreadId,
            RunId = request.RunId,
            Seq = seq,
            Event = "tool_call.resumed",
            Payload = new Dictionary<string, object?>
            {
                ["tool_results_count"] = request.ToolResults.Count
            }
        };
        await stateStore.RecordStreamEventAsync(streamEvent);
        await WriteStreamEventAsync(streamEvent);
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
            IReadOnlyAIContext aiContext = await contextBuilder.BuildAsync();
            IReadOnlyList<string> systemMessages = await promptBuilder.BuildSystemMessagesAsync(aiContext);
            AIAgentResumeRequest resumeRequest = new()
            {
                ThreadId = request.ThreadId,
                RunId = request.RunId,
                ToolResults = request.ToolResults.Select(ToRuntimeToolResult).ToArray(),
                AIContext = aiContext,
                SystemMessages = systemMessages,
                CancellationToken = HttpContext.RequestAborted
            };
            await foreach (AIAgentRunOutput output in runtime.ResumeAsync(resumeRequest).WithCancellation(HttpContext.RequestAborted))
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
    [HttpPost("{runId}/cancel")]
    public async Task<IActionResult> CancelAsync(string runId, CancelAgentRunRequest request)
    {
        await stateStore.InitializeAsync();
        cancellationRegistry.Cancel(runId);
        await stateStore.CompleteRunAsync(runId, AgentRunStatus.Cancelled, $"{request.Source}:{request.Reason}");
        return Ok();
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
    private async Task PersistRuntimeOutputAsync(AgentStreamEvent streamEvent, AIAgentRunOutput output)
    {
        await stateStore.RecordStreamEventAsync(streamEvent);
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
            Metadata = output.ToolArguments ?? new Dictionary<string, object?>()
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
                Metadata = toolResult.Result ?? toolResult.Error ?? new Dictionary<string, object?>()
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
