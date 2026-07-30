namespace Materal.MergeBlock.AI.Abstractions.Runtime;

/// <summary>
/// AI Agent运行输出
/// </summary>
public class AIAgentRunOutput
{
    /// <summary>
    /// 输出类型
    /// </summary>
    public AIAgentRunOutputType Type { get; init; }
    /// <summary>
    /// 文本内容
    /// </summary>
    public string? Text { get; init; }
    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string? ToolCallId { get; init; }
    /// <summary>
    /// 工具名称
    /// </summary>
    public string? ToolName { get; init; }
    /// <summary>
    /// 工具参数
    /// </summary>
    public IReadOnlyDictionary<string, object?>? ToolArguments { get; init; }
    /// <summary>
    /// 工具参数增量
    /// </summary>
    public string? ToolArgumentsDelta { get; init; }
    /// <summary>
    /// 状态
    /// </summary>
    public string? Status { get; init; }
    /// <summary>
    /// 工具结果
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Result { get; init; }
    /// <summary>
    /// 工具错误
    /// </summary>
    public IReadOnlyDictionary<string, object?>? ToolError { get; init; }
    /// <summary>
    /// 是否通过审查
    /// </summary>
    public bool? Approved { get; init; }
    /// <summary>
    /// 原因
    /// </summary>
    public string? Reason { get; init; }
    /// <summary>
    /// 风险等级
    /// </summary>
    public string? RiskLevel { get; init; }
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; init; }
    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; init; }
    /// <summary>
    /// 扩展元数据
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
    /// <summary>
    /// 创建文本增量
    /// </summary>
    public static AIAgentRunOutput MessageDelta(string text) => new()
    {
        Type = AIAgentRunOutputType.MessageDelta,
        Text = text
    };
    /// <summary>
    /// 创建思考增量
    /// </summary>
    public static AIAgentRunOutput ThinkingDelta(string text) => new()
    {
        Type = AIAgentRunOutputType.ThinkingDelta,
        Text = text
    };
    /// <summary>
    /// 创建工具调用增量
    /// </summary>
    public static AIAgentRunOutput ToolCallDelta(string toolCallId, string toolName, string argumentsDelta) => new()
    {
        Type = AIAgentRunOutputType.ToolCallDelta,
        ToolCallId = toolCallId,
        ToolName = toolName,
        ToolArgumentsDelta = argumentsDelta
    };
    /// <summary>
    /// 创建工具调用请求
    /// </summary>
    public static AIAgentRunOutput ToolCallRequested(string toolCallId, string toolName, IReadOnlyDictionary<string, object?>? arguments = null) => new()
    {
        Type = AIAgentRunOutputType.ToolCallRequested,
        ToolCallId = toolCallId,
        ToolName = toolName,
        ToolArguments = arguments
    };
    /// <summary>
    /// 创建工具结果完成
    /// </summary>
    public static AIAgentRunOutput ToolResultCompleted(string toolCallId, string status, IReadOnlyDictionary<string, object?>? result = null, IReadOnlyDictionary<string, object?>? error = null) => new()
    {
        Type = AIAgentRunOutputType.ToolResultCompleted,
        ToolCallId = toolCallId,
        Status = status,
        Result = result,
        ToolError = error
    };
    /// <summary>
    /// 创建脚本审查完成
    /// </summary>
    public static AIAgentRunOutput ScriptReviewCompleted(
        string toolCallId,
        bool approved,
        string reason,
        string? riskLevel = null,
        IReadOnlyDictionary<string, object?>? metadata = null) => new()
    {
        Type = AIAgentRunOutputType.ScriptReviewCompleted,
        ToolCallId = toolCallId,
        Approved = approved,
        Reason = reason,
        RiskLevel = riskLevel,
        Metadata = metadata
    };
    /// <summary>
    /// 创建心跳
    /// </summary>
    public static AIAgentRunOutput Heartbeat() => new()
    {
        Type = AIAgentRunOutputType.Heartbeat
    };
    /// <summary>
    /// 创建恢复开始
    /// </summary>
    public static AIAgentRunOutput RecoveryStarted() => new()
    {
        Type = AIAgentRunOutputType.RecoveryStarted
    };
    /// <summary>
    /// 创建恢复完成
    /// </summary>
    public static AIAgentRunOutput RecoveryCompleted() => new()
    {
        Type = AIAgentRunOutputType.RecoveryCompleted
    };
    /// <summary>
    /// 创建恢复失败
    /// </summary>
    public static AIAgentRunOutput RecoveryFailed(string message) => new()
    {
        Type = AIAgentRunOutputType.RecoveryFailed,
        ErrorMessage = message
    };
    /// <summary>
    /// 创建运行暂停
    /// </summary>
    public static AIAgentRunOutput RunPaused(string reason = "tool_result_required", IReadOnlyList<string>? toolCallIds = null) => new()
    {
        Type = AIAgentRunOutputType.RunPaused,
        Reason = reason,
        Metadata = toolCallIds is { Count: > 0 }
            ? new Dictionary<string, object?>
            {
                ["tool_call_ids"] = toolCallIds
            }
            : null
    };
    /// <summary>
    /// 创建运行取消
    /// </summary>
    public static AIAgentRunOutput RunCancelled(string reason = "user_requested", string source = "agent_chat_ui") => new()
    {
        Type = AIAgentRunOutputType.RunCancelled,
        Reason = reason,
        Metadata = new Dictionary<string, object?>
        {
            ["source"] = source
        }
    };
    /// <summary>
    /// 创建运行完成
    /// </summary>
    public static AIAgentRunOutput RunCompleted() => new()
    {
        Type = AIAgentRunOutputType.RunCompleted
    };
    /// <summary>
    /// 创建错误
    /// </summary>
    public static AIAgentRunOutput Error(string message, string code = "runtime_error") => new()
    {
        Type = AIAgentRunOutputType.Error,
        ErrorMessage = message,
        ErrorCode = code
    };
}
