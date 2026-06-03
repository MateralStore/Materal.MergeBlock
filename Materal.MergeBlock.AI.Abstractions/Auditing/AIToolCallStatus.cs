namespace Materal.MergeBlock.AI.Abstractions.Auditing;

/// <summary>
/// AI工具调用状态
/// </summary>
public static class AIToolCallStatus
{
    /// <summary>
    /// 已请求
    /// </summary>
    public const string Requested = "requested";
    /// <summary>
    /// 已开始
    /// </summary>
    public const string Started = "started";
    /// <summary>
    /// 已完成
    /// </summary>
    public const string Completed = "completed";
    /// <summary>
    /// 失败
    /// </summary>
    public const string Failed = "failed";
    /// <summary>
    /// 已拒绝
    /// </summary>
    public const string Rejected = "rejected";
    /// <summary>
    /// 已取消
    /// </summary>
    public const string Cancelled = "cancelled";
}
