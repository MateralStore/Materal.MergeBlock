using Materal.MergeBlock.AI.Web.Persistence;

namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent运行状态映射器
/// </summary>
public static class AgentRunStatusMapper
{
    /// <summary>
    /// 映射为公开状态
    /// </summary>
    public static string ToPublicStatus(string status)
    {
        return string.Equals(status, AgentRunStatus.WaitingToolResult, StringComparison.OrdinalIgnoreCase)
            ? AgentRunStatus.Paused
            : status;
    }

    /// <summary>
    /// 映射为公开运行记录
    /// </summary>
    public static AgentRunRecord ToPublicRun(AgentRunRecord run)
    {
        return new AgentRunRecord
        {
            RunId = run.RunId,
            ThreadId = run.ThreadId,
            Status = ToPublicStatus(run.Status),
            ErrorMessage = run.ErrorMessage
        };
    }

    /// <summary>
    /// 映射为公开会话追踪
    /// </summary>
    public static AgentSessionTrace ToPublicSession(AgentSessionTrace session)
    {
        return new AgentSessionTrace
        {
            ThreadId = session.ThreadId,
            Runs = [.. session.Runs.Select(ToPublicRun)]
        };
    }

    /// <summary>
    /// 映射为公开调试追踪摘要
    /// </summary>
    public static AgentDebugTraceSummary ToPublicDebugTrace(AgentDebugTraceSummary trace)
    {
        return new AgentDebugTraceSummary
        {
            TraceId = trace.TraceId,
            RunId = trace.RunId,
            ThreadId = trace.ThreadId,
            Status = ToPublicStatus(trace.Status),
            ErrorMessage = trace.ErrorMessage
        };
    }

    /// <summary>
    /// 映射为公开运行追踪
    /// </summary>
    public static AgentRunTrace ToPublicTrace(AgentRunTrace trace)
    {
        return new AgentRunTrace
        {
            Run = ToPublicRun(trace.Run),
            Events = trace.Events,
            ToolCalls = trace.ToolCalls,
            Messages = trace.Messages,
            ScriptReviews = trace.ScriptReviews,
            Timeline = trace.Timeline,
            Checkpoint = trace.Checkpoint
        };
    }
}
