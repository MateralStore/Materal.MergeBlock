namespace Materal.MergeBlock.AI.Tools;

/// <summary>
/// AI服务端工具注册表
/// </summary>
public class AIServerToolRegistry(IEnumerable<IAIServerTool> tools)
{
    private readonly Dictionary<string, IAIServerTool> _tools = tools.ToDictionary(m => m.Descriptor.Name, StringComparer.Ordinal);

    /// <summary>
    /// 工具列表
    /// </summary>
    public IReadOnlyCollection<AIToolDescriptor> Tools => [.. _tools.Values.Select(m => m.Descriptor)];

    /// <summary>
    /// 执行工具
    /// </summary>
    public async Task<AIServerToolResult> ExecuteAsync(
        string name,
        string threadId,
        string runId,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken,
        string? toolCallId = null,
        IReadOnlyAIContext? aiContext = null)
    {
        if (!_tools.TryGetValue(name, out IAIServerTool? tool))
        {
            return AIServerToolResult.Failed("tool_not_found", $"未找到AI服务端工具: {name}");
        }
        AIServerToolContext context = new()
        {
            ThreadId = threadId,
            RunId = runId,
            ToolCallId = string.IsNullOrWhiteSpace(toolCallId) ? Guid.NewGuid().ToString("N") : toolCallId,
            Arguments = arguments,
            AIContext = aiContext,
            CancellationToken = cancellationToken
        };
        return await tool.ExecuteAsync(context);
    }
}
