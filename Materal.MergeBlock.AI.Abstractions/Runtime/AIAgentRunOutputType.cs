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
    /// 工具调用请求
    /// </summary>
    ToolCallRequested,
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
