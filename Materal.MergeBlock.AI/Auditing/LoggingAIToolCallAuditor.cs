namespace Materal.MergeBlock.AI.Auditing;

/// <summary>
/// 日志AI工具调用审计器
/// </summary>
/// <param name="logger">日志记录器</param>
public class LoggingAIToolCallAuditor(ILogger<LoggingAIToolCallAuditor>? logger = null) : IAIToolCallAuditor
{
    private readonly ILogger<LoggingAIToolCallAuditor> _logger = logger ?? NullLogger<LoggingAIToolCallAuditor>.Instance;
    /// <inheritdoc />
    public Task AuditAsync(AIToolCallAuditContext context)
    {
        _logger.LogInformation(
            "AI工具调用: Tool={ToolName}, Mode={ExecutionMode}, Thread={ThreadId}, Run={RunId}, Status={Status}",
            context.ToolName,
            context.ExecutionMode,
            context.ThreadId,
            context.RunId,
            context.Status);
        return Task.CompletedTask;
    }
}
