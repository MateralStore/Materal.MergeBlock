namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent恢复请求
/// </summary>
public class AIAgentResumeRequest
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ThreadId { get; init; } = string.Empty;
    /// <summary>
    /// 运行ID
    /// </summary>
    public string RunId { get; init; } = string.Empty;
    /// <summary>
    /// 工具结果
    /// </summary>
    public IReadOnlyList<AIAgentRemoteToolResult> ToolResults { get; init; } = [];
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
