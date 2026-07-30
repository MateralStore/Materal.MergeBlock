namespace Materal.MergeBlock.AI.Web.Runtime;

/// <summary>
/// AI Agent运行时看门狗配置
/// </summary>
public class AIAgentWatchdogOptions
{
    /// <summary>
    /// 心跳间隔
    /// </summary>
    public TimeSpan HeartbeatInterval { get; init; } = TimeSpan.FromSeconds(5);
    /// <summary>
    /// 空闲超时
    /// </summary>
    public TimeSpan IdleTimeout { get; init; } = TimeSpan.FromSeconds(60);
    /// <summary>
    /// 仅思考输出超时
    /// </summary>
    public TimeSpan ThinkingOnlyTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
