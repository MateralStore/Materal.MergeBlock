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
        Assert.AreEqual("hello", message.Payload["text"]);
        Assert.AreEqual("hello", message.Payload["delta"]);
        Assert.AreEqual("tool_call.requested", toolCall.Event);
        Assert.AreEqual("call_001", toolCall.Payload["tool_call_id"]);
        Assert.AreEqual("run.paused", paused.Event);
        Assert.AreEqual("run.completed", completed.Event);
        Assert.AreEqual("error", error.Event);
        Assert.AreEqual("provider_error", error.Payload["code"]);
    }

    [TestMethod]
    public void ToStreamEvent_ShouldMapExtendedRuntimeOutputs()
    {
        AIAgentStreamAdapter adapter = new();

        AgentStreamEvent thinking = adapter.ToStreamEvent("thread_001", "run_001", 1, AIAgentRunOutput.ThinkingDelta("reasoning"));
        AgentStreamEvent toolDelta = adapter.ToStreamEvent("thread_001", "run_001", 2, AIAgentRunOutput.ToolCallDelta("call_001", "runClientAction", """{"value":"""));
        AgentStreamEvent toolResult = adapter.ToStreamEvent("thread_001", "run_001", 3, AIAgentRunOutput.ToolResultCompleted("call_001", AIToolCallStatus.Failed, error: new Dictionary<string, object?> { ["message"] = "bad script" }));
        AgentStreamEvent review = adapter.ToStreamEvent("thread_001", "run_001", 4, AIAgentRunOutput.ScriptReviewCompleted("call_001", false, "bad script", "high"));
        AgentStreamEvent heartbeat = adapter.ToStreamEvent("thread_001", "run_001", 5, AIAgentRunOutput.Heartbeat());
        AgentStreamEvent recoveryStarted = adapter.ToStreamEvent("thread_001", "run_001", 6, AIAgentRunOutput.RecoveryStarted());
        AgentStreamEvent recoveryCompleted = adapter.ToStreamEvent("thread_001", "run_001", 7, AIAgentRunOutput.RecoveryCompleted());
        AgentStreamEvent recoveryFailed = adapter.ToStreamEvent("thread_001", "run_001", 8, AIAgentRunOutput.RecoveryFailed("timeout"));

        Assert.AreEqual("thinking.delta", thinking.Event);
        Assert.AreEqual("reasoning", thinking.Payload["text"]);
        Assert.AreEqual("tool_call.delta", toolDelta.Event);
        Assert.AreEqual("""{"value":""", toolDelta.Payload["arguments_delta"]);
        Assert.AreEqual("tool_result.completed", toolResult.Event);
        Assert.AreEqual(AIToolCallStatus.Failed, toolResult.Payload["status"]);
        Assert.AreEqual("script_review.completed", review.Event);
        Assert.AreEqual(false, review.Payload["approved"]);
        Assert.AreEqual("agent.heartbeat", heartbeat.Event);
        Assert.AreEqual("agent.recovery.started", recoveryStarted.Event);
        Assert.AreEqual("agent.recovery.completed", recoveryCompleted.Event);
        Assert.AreEqual("agent.recovery.failed", recoveryFailed.Event);
        Assert.AreEqual("timeout", recoveryFailed.Payload["message"]);
    }
}
