using Materal.MergeBlock.AI.Abstractions.Runtime;
using Microsoft.Extensions.Options;

namespace MMB.Demo.Application.AI;

/// <summary>
/// GLM5.1 AI Agent运行时
/// </summary>
public class Glm51AIAgentRuntime(IOptions<Glm51AIOptions> options, IGlm51AgentRunner agentRunner) : IAIAgentRuntime
{
    private const string RemoteToolTrigger = "use-remote-tool";

    /// <inheritdoc />
    public async IAsyncEnumerable<AIAgentRunOutput> RunAsync(AIAgentRunRequest request)
    {
        if (request.Message.Contains(RemoteToolTrigger, StringComparison.OrdinalIgnoreCase))
        {
            yield return AIAgentRunOutput.ToolCallRequested(
                Guid.NewGuid().ToString("N"),
                "runClientAction",
                new Dictionary<string, object?>
                {
                    ["action"] = "demo",
                    ["message"] = request.Message
                });
            yield return AIAgentRunOutput.RunPaused();
            yield break;
        }
        Glm51AIOptions currentOptions = options.Value;
        if (string.IsNullOrWhiteSpace(currentOptions.ApiKey))
        {
            yield return AIAgentRunOutput.Error("未配置MergeBlock:AI:GLM51:ApiKey，无法调用GLM5.1。", "glm51_api_key_missing");
            yield break;
        }
        Glm51AgentRunRequest runRequest = new()
        {
            ThreadId = request.ThreadId,
            Message = request.Message,
            AIContext = request.AIContext,
            SystemMessages = request.SystemMessages,
            CancellationToken = request.CancellationToken
        };
        await foreach (string delta in agentRunner.RunStreamingAsync(runRequest).WithCancellation(request.CancellationToken))
        {
            if (string.IsNullOrEmpty(delta)) continue;
            yield return AIAgentRunOutput.MessageDelta(delta);
        }
        yield return AIAgentRunOutput.RunCompleted();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<AIAgentRunOutput> ResumeAsync(AIAgentResumeRequest request)
    {
        await Task.Yield();
        yield return AIAgentRunOutput.MessageDelta("Remote tool completed");
        yield return AIAgentRunOutput.RunCompleted();
    }
}
