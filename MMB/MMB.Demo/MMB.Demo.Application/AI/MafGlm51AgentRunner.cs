using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace MMB.Demo.Application.AI;

/// <summary>
/// 基于Microsoft Agent Framework的GLM5.1 Agent运行器
/// </summary>
public class MafGlm51AgentRunner(
    IOptions<Glm51AIOptions> aiOptions,
    IOptions<Glm51AgentOptions> agentOptions,
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider) : IGlm51AgentRunner
{
    /// <inheritdoc />
    public async IAsyncEnumerable<string> RunStreamingAsync(Glm51AgentRunRequest request)
    {
        Glm51AIOptions currentAIOptions = aiOptions.Value;
        ChatClientAgent agent = CreateAgent(currentAIOptions, agentOptions.Value);
        ChatClientAgentRunOptions runOptions = new()
        {
            ChatOptions = new ChatOptions
            {
                ConversationId = request.ThreadId,
                Instructions = BuildInstructions(agentOptions.Value, request.SystemMessages),
                ModelId = currentAIOptions.Model,
                Temperature = currentAIOptions.Temperature,
                MaxOutputTokens = currentAIOptions.MaxOutputTokens
            }
        };
        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, request.Message), null, runOptions, request.CancellationToken))
        {
            if (string.IsNullOrEmpty(update.Text)) continue;
            yield return update.Text;
        }
    }

    private ChatClientAgent CreateAgent(Glm51AIOptions aiOptionsValue, Glm51AgentOptions agentOptionsValue)
    {
        OpenAIClientOptions clientOptions = new()
        {
            Endpoint = new Uri(aiOptionsValue.BaseUrl)
        };
        OpenAIClient client = new(new ApiKeyCredential(aiOptionsValue.ApiKey), clientOptions);
        ChatClient chatClient = client.GetChatClient(aiOptionsValue.Model);
        return chatClient.AsAIAgent(
            agentOptionsValue.Name,
            agentOptionsValue.Description,
            agentOptionsValue.Instructions,
            DemoLocalAITools.CreateTools(),
            null,
            loggerFactory,
            serviceProvider);
    }

    private static string BuildInstructions(Glm51AgentOptions agentOptionsValue, IReadOnlyList<string> systemMessages)
    {
        if (systemMessages.Count == 0) return agentOptionsValue.Instructions;
        return $"{agentOptionsValue.Instructions}\n\n{string.Join("\n\n", systemMessages)}";
    }
}
