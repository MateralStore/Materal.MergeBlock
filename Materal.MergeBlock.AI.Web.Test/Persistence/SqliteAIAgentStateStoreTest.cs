using Materal.MergeBlock.AI.Web.Persistence;
using Materal.MergeBlock.AI.Web.RemoteTools;

namespace Materal.MergeBlock.AI.Web.Test.Persistence;

[TestClass]
public class SqliteAIAgentStateStoreTest
{
    [TestMethod]
    public async Task GetRunTraceAsync_ShouldReturnRunEventsAndToolCalls()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sqlite3");
        SqliteAIAgentStateStore store = new(databasePath);
        await store.InitializeAsync();

        await store.UpsertSessionAsync("thread_001");
        await store.StartRunAsync("run_001", "thread_001");
        await store.RecordStreamEventAsync(new AgentStreamEvent
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Seq = 1,
            Event = "run.started",
            Payload = new Dictionary<string, object?> { ["message"] = "started" }
        });
        await store.RecordToolCallAsync(new RemoteToolPendingCall
        {
            ToolCallId = "call_001",
            ThreadId = "thread_001",
            RunId = "run_001",
            ToolName = "runWordScript",
            Status = "requested",
            Arguments = new Dictionary<string, object?> { ["script"] = "return 1;" }
        });
        await store.CompleteRunAsync("run_001", "waiting_tool_result");

        AgentRunTrace trace = await store.GetRunTraceAsync("run_001");

        Assert.AreEqual("run_001", trace.Run.RunId);
        Assert.AreEqual("thread_001", trace.Run.ThreadId);
        Assert.AreEqual("waiting_tool_result", trace.Run.Status);
        Assert.AreEqual(1, trace.Events.Count);
        Assert.AreEqual("run.started", trace.Events[0].Event);
        Assert.AreEqual(1, trace.ToolCalls.Count);
        Assert.AreEqual("call_001", trace.ToolCalls[0].ToolCallId);
    }

    [TestMethod]
    public async Task RecordToolResultAsync_ShouldUpdateToolCallResult()
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
            ToolName = "runClientAction",
            Status = AIToolCallStatus.Requested,
            Arguments = new Dictionary<string, object?> { ["kind"] = "demo" }
        });

        await store.RecordToolResultAsync(new RemoteToolResultItem
        {
            ToolCallId = "call_001",
            Status = AIToolCallStatus.Completed,
            Result = new Dictionary<string, object?> { ["ok"] = true }
        });

        AgentRunTrace trace = await store.GetRunTraceAsync("run_001");

        Assert.AreEqual(AIToolCallStatus.Completed, trace.ToolCalls[0].Status);
        Assert.IsNotNull(trace.ToolCalls[0].Result);
        Assert.AreEqual("True", trace.ToolCalls[0].Result!["ok"]?.ToString());
    }
}
