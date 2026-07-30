namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent运行状态
/// </summary>
public static class AgentRunStatus
{
    /// <summary>
    /// 运行中
    /// </summary>
    public const string Running = "running";
    /// <summary>
    /// 等待工具结果
    /// </summary>
    public const string WaitingToolResult = "waiting_tool_result";
    /// <summary>
    /// 已暂停
    /// </summary>
    public const string Paused = "paused";
    /// <summary>
    /// 已完成
    /// </summary>
    public const string Completed = "completed";
    /// <summary>
    /// 失败
    /// </summary>
    public const string Failed = "failed";
    /// <summary>
    /// 已取消
    /// </summary>
    public const string Cancelled = "cancelled";
}
