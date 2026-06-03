namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent运行时
/// </summary>
public interface IAIAgentRuntime
{
    /// <summary>
    /// 运行
    /// </summary>
    /// <param name="request">运行请求</param>
    /// <returns>运行输出</returns>
    IAsyncEnumerable<AIAgentRunOutput> RunAsync(AIAgentRunRequest request);
    /// <summary>
    /// 恢复运行
    /// </summary>
    /// <param name="request">恢复请求</param>
    /// <returns>运行输出</returns>
    IAsyncEnumerable<AIAgentRunOutput> ResumeAsync(AIAgentResumeRequest request);
}
