using Materal.MergeBlock.AI.Abstractions.Runtime;

namespace Materal.MergeBlock.AI.Web.Runtime;

/// <summary>
/// AI Agent流式事件适配器
/// </summary>
public class AIAgentStreamAdapter
{
    /// <summary>
    /// 转换为流式事件
    /// </summary>
    public AgentStreamEvent ToStreamEvent(string threadId, string runId, int seq, AIAgentRunOutput output)
    {
        return new AgentStreamEvent
        {
            ThreadId = threadId,
            RunId = runId,
            Seq = seq,
            Event = GetEventName(output.Type),
            Payload = BuildPayload(output)
        };
    }

    private static string GetEventName(AIAgentRunOutputType type)
    {
        return type switch
        {
            AIAgentRunOutputType.MessageDelta => "message.delta",
            AIAgentRunOutputType.ToolCallRequested => "tool_call.requested",
            AIAgentRunOutputType.RunPaused => "run.paused",
            AIAgentRunOutputType.RunCompleted => "run.completed",
            AIAgentRunOutputType.Error => "error",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "未知AI Agent输出类型。")
        };
    }

    private static IReadOnlyDictionary<string, object?> BuildPayload(AIAgentRunOutput output)
    {
        Dictionary<string, object?> result = new(StringComparer.Ordinal);
        if (!string.IsNullOrEmpty(output.Text))
        {
            result["delta"] = output.Text;
        }
        if (!string.IsNullOrEmpty(output.ToolCallId))
        {
            result["tool_call_id"] = output.ToolCallId;
        }
        if (!string.IsNullOrEmpty(output.ToolName))
        {
            result["name"] = output.ToolName;
        }
        if (output.ToolArguments is not null)
        {
            result["arguments"] = output.ToolArguments;
        }
        if (!string.IsNullOrEmpty(output.ErrorMessage))
        {
            result["message"] = output.ErrorMessage;
        }
        if (!string.IsNullOrEmpty(output.ErrorCode))
        {
            result["code"] = output.ErrorCode;
        }
        if (output.Metadata is not null)
        {
            foreach (KeyValuePair<string, object?> item in output.Metadata)
            {
                result[item.Key] = item.Value;
            }
        }
        return result;
    }
}
