using Materal.MergeBlock.AI.Abstractions.Runtime;
using Materal.MergeBlock.AI.Web.Runtime;

namespace Materal.MergeBlock.AI.Web.Test.Runtime;

[TestClass]
public class AIAgentStreamAdapterTest
{
    [TestMethod]
    public void ToStreamEvent_ShouldMapRuntimeOutputs()
    {
        AIAgentStreamAdapter adapter = new();

        AgentStreamEvent message = adapter.ToStreamEvent("thread_001", "run_001", 1, AIAgentRunOutput.MessageDelta("hello"));
        AgentStreamEvent toolCall = adapter.ToStreamEvent("thread_001", "run_001", 2, AIAgentRunOutput.ToolCallRequested("call_001", "runClientAction", new Dictionary<string, object?> { ["value"] = 1 }));
        AgentStreamEvent paused = adapter.ToStreamEvent("thread_001", "run_001", 3, AIAgentRunOutput.RunPaused());
        AgentStreamEvent completed = adapter.ToStreamEvent("thread_001", "run_001", 4, AIAgentRunOutput.RunCompleted());
        AgentStreamEvent error = adapter.ToStreamEvent("thread_001", "run_001", 5, AIAgentRunOutput.Error("boom", "provider_error"));

        Assert.AreEqual("message.delta", message.Event);
        Assert.AreEqual("hello", message.Payload["delta"]);
        Assert.AreEqual("tool_call.requested", toolCall.Event);
        Assert.AreEqual("call_001", toolCall.Payload["tool_call_id"]);
        Assert.AreEqual("run.paused", paused.Event);
        Assert.AreEqual("run.completed", completed.Event);
        Assert.AreEqual("error", error.Event);
        Assert.AreEqual("provider_error", error.Payload["code"]);
    }
}
