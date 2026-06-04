namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent运行输出类型
/// </summary>
public enum AIAgentRunOutputType
{
    /// <summary>
    /// 消息增量
    /// </summary>
    MessageDelta,
    /// <summary>
    /// 思考增量
    /// </summary>
    ThinkingDelta,
    /// <summary>
    /// 工具调用增量
    /// </summary>
    ToolCallDelta,
    /// <summary>
    /// 工具调用请求
    /// </summary>
    ToolCallRequested,
    /// <summary>
    /// 工具结果完成
    /// </summary>
    ToolResultCompleted,
    /// <summary>
    /// 脚本审查完成
    /// </summary>
    ScriptReviewCompleted,
    /// <summary>
    /// 心跳
    /// </summary>
    Heartbeat,
    /// <summary>
    /// 恢复开始
    /// </summary>
    RecoveryStarted,
    /// <summary>
    /// 恢复完成
    /// </summary>
    RecoveryCompleted,
    /// <summary>
    /// 恢复失败
    /// </summary>
    RecoveryFailed,
    /// <summary>
    /// 运行暂停
    /// </summary>
    RunPaused,
    /// <summary>
    /// 运行完成
    /// </summary>
    RunCompleted,
    /// <summary>
    /// 错误
    /// </summary>
    Error
}
