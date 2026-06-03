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
        if (!string.Equals(trace.Run.ThreadId, request.ThreadId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"会话ID不匹配: Run={trace.Run.ThreadId}, Request={request.ThreadId}");
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
}
