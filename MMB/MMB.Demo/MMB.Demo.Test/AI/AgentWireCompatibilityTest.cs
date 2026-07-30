using System.Text.Json;
using Materal.MergeBlock.AI.Web.Models;

namespace MMB.Demo.Test.AI;

[TestClass]
public class AgentWireCompatibilityTest
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [TestMethod]
    public void Demo_ShouldDeserializeExistingChatContract()
    {
        const string json = """
        {
          "schema_version": "agent-chat-request-v1",
          "thread_id": "demo-thread",
          "message": "hello",
          "model_config": {
            "provider": "openai_compatible",
            "model": "glm-5.1",
            "base_url": "https://api.z.ai/api/paas/v4/",
            "api_key": "secret",
            "temperature": 0.6,
            "max_tokens": 2048,
            "reasoning": {
              "enabled": true,
              "effort": "medium",
              "budget_tokens": 4096
            }
          },
          "skill_request": {
            "name": "client_action",
            "description": "Run a generic client-side action."
          },
          "metadata": {
            "source": "demo-page"
          }
        }
        """;

        AgentChatRequest request = JsonSerializer.Deserialize<AgentChatRequest>(json, JsonOptions)!;

        Assert.AreEqual("agent-chat-request-v1", request.SchemaVersion);
        Assert.AreEqual("demo-thread", request.ThreadId);
        Assert.AreEqual("hello", request.Message);
        Assert.AreEqual("openai_compatible", request.ModelConfig.Provider);
        Assert.AreEqual("glm-5.1", request.ModelConfig.Model);
        Assert.AreEqual("https://api.z.ai/api/paas/v4/", request.ModelConfig.BaseUrl);
        Assert.AreEqual("secret", request.ModelConfig.ApiKey);
        Assert.AreEqual(0.6f, request.ModelConfig.Temperature);
        Assert.AreEqual(2048, request.ModelConfig.MaxTokens);
        Assert.IsNotNull(request.ModelConfig.Reasoning);
        Assert.IsTrue(request.ModelConfig.Reasoning.Enabled);
        Assert.AreEqual("medium", request.ModelConfig.Reasoning.Effort);
        Assert.AreEqual(4096, request.ModelConfig.Reasoning.BudgetTokens);
        Assert.AreEqual("client_action", request.SkillRequest?.Name);
    }

    [TestMethod]
    public void Demo_ShouldDeserializeRemoteToolResumeContract()
    {
        const string json = """
        {
          "schema_version": "remote-tool-results-v1",
          "thread_id": "demo-thread",
          "run_id": "run-001",
          "tool_results": [
            {
              "tool_call_id": "call-001",
              "status": "completed",
              "result": {
                "ok": true,
                "value": "done"
              }
            }
          ]
        }
        """;

        RemoteToolResultsRequest request = JsonSerializer.Deserialize<RemoteToolResultsRequest>(json, JsonOptions)!;

        Assert.AreEqual("remote-tool-results-v1", request.SchemaVersion);
        Assert.AreEqual("demo-thread", request.ThreadId);
        Assert.AreEqual("run-001", request.RunId);
        Assert.AreEqual(1, request.ToolResults.Count);
        Assert.AreEqual("call-001", request.ToolResults[0].ToolCallId);
        Assert.AreEqual("completed", request.ToolResults[0].Status);
        Assert.IsNotNull(request.ToolResults[0].Result);
        Assert.IsTrue(((JsonElement)request.ToolResults[0].Result!["ok"]!).GetBoolean());
    }

    [TestMethod]
    public void Demo_ShouldDeserializeCancelContract()
    {
        const string json = """
        {
          "thread_id": "demo-thread",
          "reason": "manual_cancel",
          "source": "ai-chat-test"
        }
        """;

        CancelAgentRunRequest request = JsonSerializer.Deserialize<CancelAgentRunRequest>(json, JsonOptions)!;

        Assert.AreEqual("demo-thread", request.ThreadId);
        Assert.AreEqual("manual_cancel", request.Reason);
        Assert.AreEqual("ai-chat-test", request.Source);
    }

    [TestMethod]
    public void Demo_ShouldDeserializeStreamEventContract()
    {
        const string json = """
        {
          "schema_version": "agent-stream-v1",
          "thread_id": "demo-thread",
          "run_id": "run-001",
          "seq": 4,
          "event": "run.paused",
          "payload": {
            "reason": "tool_result_required",
            "tool_call_ids": [ "call-001" ]
          }
        }
        """;

        AgentStreamEvent streamEvent = JsonSerializer.Deserialize<AgentStreamEvent>(json, JsonOptions)!;

        Assert.AreEqual("agent-stream-v1", streamEvent.SchemaVersion);
        Assert.AreEqual("demo-thread", streamEvent.ThreadId);
        Assert.AreEqual("run-001", streamEvent.RunId);
        Assert.AreEqual(4, streamEvent.Seq);
        Assert.AreEqual("run.paused", streamEvent.Event);
        Assert.AreEqual("tool_result_required", ((JsonElement)streamEvent.Payload["reason"]!).GetString());
    }
}
