namespace Materal.MergeBlock.AI.Test.Auditing;

[TestClass]
public class LoggingAIToolCallAuditorTest
{
    [TestMethod]
    public async Task AuditAsync_ShouldAcceptRemoteContext()
    {
        LoggingAIToolCallAuditor auditor = new(NullLogger<LoggingAIToolCallAuditor>.Instance);

        await auditor.AuditAsync(new AIToolCallAuditContext
        {
            ToolName = "runClientAction",
            ExecutionMode = AIToolExecutionMode.Remote,
            ThreadId = "thread_001",
            RunId = "run_001",
            Status = AIToolCallStatus.Requested
        });
    }
}
