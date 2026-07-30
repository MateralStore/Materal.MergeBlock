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
        return new AIAgentRunRequest
        {
            ThreadId = threadId,
            RunId = runId,
            Message = request.Message,
            ModelConfig = ApplyTopLevelReasoningAndThinking(request),
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
