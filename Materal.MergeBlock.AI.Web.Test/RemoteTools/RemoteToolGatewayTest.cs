namespace Materal.MergeBlock.AI.Web.Test.RemoteTools;

[TestClass]
public class RemoteToolGatewayTest
{
    [TestMethod]
    public async Task ValidateResumeAsync_ShouldRejectMismatchedToolCallIds()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sqlite3");
        SqliteAIAgentStateStore store = new(databasePath);
        await store.InitializeAsync();
        await store.UpsertSessionAsync("thread_001");
        await store.StartRunAsync("run_001", "thread_001");
        await store.RecordToolCallAsync(new RemoteToolPendingCall
        {
            ToolCallId = "call_001",
            ThreadId = "thread_001",
            RunId = "run_001",
            ToolName = "runWordScript",
            Status = AIToolCallStatus.Requested
        });
        RemoteToolGateway gateway = new(store);

        RemoteToolResultsRequest request = new()
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            ToolResults =
            [
                new RemoteToolResultItem
                {
                    ToolCallId = "call_002",
                    Status = AIToolCallStatus.Completed
                }
            ]
        };

        InvalidOperationException exception = await ThrowsInvalidOperationExceptionAsync(() => gateway.ValidateResumeAsync(request));

        StringAssert.Contains(exception.Message, "工具调用ID不匹配");
    }
    private static async Task<InvalidOperationException> ThrowsInvalidOperationExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }
        Assert.Fail("应抛出InvalidOperationException。");
        throw new UnreachableException();
    }
}
