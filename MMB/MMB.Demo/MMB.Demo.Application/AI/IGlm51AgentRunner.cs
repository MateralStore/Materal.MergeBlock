namespace MMB.Demo.Application.AI;

/// <summary>
/// GLM5.1 Agent运行器
/// </summary>
public interface IGlm51AgentRunner
{
    /// <summary>
    /// 流式运行
    /// </summary>
    IAsyncEnumerable<string> RunStreamingAsync(Glm51AgentRunRequest request);
}
