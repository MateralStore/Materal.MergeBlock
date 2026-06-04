namespace Materal.MergeBlock.AI.Web.RemoteTools;

/// <summary>
/// 远程工具网关
/// </summary>
public class RemoteToolGateway(IAIAgentStateStore stateStore)
{
    /// <summary>
    /// 校验恢复请求
    /// </summary>
    /// <param name="request">远程工具结果请求</param>
    public async Task ValidateResumeAsync(RemoteToolResultsRequest request)
    {
        AgentRunTrace trace = await stateStore.GetRunTraceAsync(request.RunId);
        if (IsTerminalRun(trace.Run.Status))
        {
            throw new InvalidOperationException($"运行已进入终态，不能恢复远程工具调用: {trace.Run.Status}");
        }
        if (!string.Equals(trace.Run.ThreadId, request.ThreadId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"会话ID不匹配: Run={trace.Run.ThreadId}, Request={request.ThreadId}");
        }
        string[] duplicateIds = request.ToolResults
            .GroupBy(m => m.ToolCallId, StringComparer.Ordinal)
            .Where(m => m.Count() > 1)
            .Select(m => m.Key)
            .ToArray();
        if (duplicateIds.Length > 0)
        {
            throw new InvalidOperationException($"工具结果包含重复的工具调用ID: {string.Join(",", duplicateIds)}");
        }
        RemoteToolResultItem? invalidStatusItem = request.ToolResults.FirstOrDefault(m => !IsValidToolResultStatus(m.Status));
        if (invalidStatusItem is not null)
        {
            throw new InvalidOperationException($"工具结果状态无效: ToolCallId={invalidStatusItem.ToolCallId}, Status={invalidStatusItem.Status}");
        }
        string[] pendingIds = trace.ToolCalls
            .Where(m => string.Equals(m.Status, AIToolCallStatus.Requested, StringComparison.OrdinalIgnoreCase))
            .Select(m => m.ToolCallId)
            .OrderBy(m => m, StringComparer.Ordinal)
            .ToArray();
        string[] requestIds = request.ToolResults
            .Select(m => m.ToolCallId)
            .OrderBy(m => m, StringComparer.Ordinal)
            .ToArray();
        if (!pendingIds.SequenceEqual(requestIds))
        {
            throw new InvalidOperationException($"工具调用ID不匹配: Pending=[{string.Join(",", pendingIds)}], Request=[{string.Join(",", requestIds)}]");
        }
    }
    private static bool IsTerminalRun(string status)
    {
        return string.Equals(status, AgentRunStatus.Completed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, AgentRunStatus.Failed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, AgentRunStatus.Cancelled, StringComparison.OrdinalIgnoreCase);
    }
    private static bool IsValidToolResultStatus(string status)
    {
        return string.Equals(status, AIToolCallStatus.Completed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, AIToolCallStatus.Failed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, AIToolCallStatus.Rejected, StringComparison.OrdinalIgnoreCase);
    }
}
