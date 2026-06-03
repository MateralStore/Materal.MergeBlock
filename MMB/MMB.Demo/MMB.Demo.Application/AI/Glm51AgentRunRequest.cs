using Materal.MergeBlock.AI.Abstractions.Context;

namespace MMB.Demo.Application.AI;

/// <summary>
/// GLM5.1 Agent运行请求
/// </summary>
public class Glm51AgentRunRequest
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; init; } = string.Empty;
    /// <summary>
    /// AI上下文
    /// </summary>
    public IReadOnlyAIContext AIContext { get; init; } = default!;
    /// <summary>
    /// 系统提示词
    /// </summary>
    public IReadOnlyList<string> SystemMessages { get; init; } = [];
    /// <summary>
    /// 取消令牌
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}
