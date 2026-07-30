using System.Text.Json;

namespace Materal.MergeBlock.AI.Web.Test.Models;

[TestClass]
public class AgentWireContractJsonTest
{
    [TestMethod]
    public void ChatRequest_ShouldDeserializeSnakeCaseContract()
    {
        const string json = """
        {
          "schema_version": "agent-chat-request-v1",
          "thread_id": "thread_001",
          "run_id": "run_001",
          "message": "hello",
          "model_config": {
            "provider": "openai_compatible",
            "model": "glm-5.1",
            "base_url": "https://api.example.test/v1",
            "api_key": "secret",
            "temperature": 0.6,
            "max_tokens": 2048,
            "reasoning": {
              "enabled": true,
              "effort": "high",
              "budget_tokens": 4096,
              "summary": "auto"
            },
            "thinking": {
              "enabled": true,
              "budget_tokens": 2048
            }
          },
          "context": {
            "client": "demo"
          },
          "metadata": {
            "trace_id": "trace_001"
          }
        }
        """;

        AgentChatRequest request = JsonSerializer.Deserialize<AgentChatRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        Assert.AreEqual("agent-chat-request-v1", request.SchemaVersion);
        Assert.AreEqual("thread_001", request.ThreadId);
        Assert.AreEqual("run_001", request.RunId);
        Assert.AreEqual("hello", request.Message);
        Assert.AreEqual("openai_compatible", request.ModelConfig.Provider);
        Assert.AreEqual("glm-5.1", request.ModelConfig.Model);
        Assert.AreEqual("https://api.example.test/v1", request.ModelConfig.BaseUrl);
        Assert.AreEqual("secret", request.ModelConfig.ApiKey);
        Assert.AreEqual(0.6f, request.ModelConfig.Temperature);
        Assert.AreEqual(2048, request.ModelConfig.MaxTokens);
        Assert.IsTrue(request.ModelConfig.Reasoning!.Enabled);
        Assert.IsTrue(request.ModelConfig.Thinking!.Enabled);
        Assert.IsTrue(request.Context.ContainsKey("client"));
        Assert.IsTrue(request.Metadata.ContainsKey("trace_id"));
    }

    [TestMethod]
    public void RemoteToolResultsRequest_ShouldDeserializeSnakeCaseContract()
    {
        const string json = """
        {
          "schema_version": "remote-tool-results-v1",
          "thread_id": "thread_001",
          "run_id": "run_001",
          "tool_results": [
            {
              "tool_call_id": "call_001",
              "status": "completed",
              "result": { "ok": true }
            }
          ]
        }
        """;

        RemoteToolResultsRequest request = JsonSerializer.Deserialize<RemoteToolResultsRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        Assert.AreEqual("remote-tool-results-v1", request.SchemaVersion);
        Assert.AreEqual("thread_001", request.ThreadId);
        Assert.AreEqual("run_001", request.RunId);
        Assert.AreEqual(1, request.ToolResults.Count);
        Assert.AreEqual("call_001", request.ToolResults[0].ToolCallId);
        Assert.AreEqual("completed", request.ToolResults[0].Status);
        Assert.IsNotNull(request.ToolResults[0].Result);
    }

    [TestMethod]
    public void CancelRequest_ShouldDeserializeSnakeCaseContract()
    {
        const string json = """
        {
          "thread_id": "thread_001",
          "source": "test-page",
          "reason": "manual_cancel"
        }
        """;

        CancelAgentRunRequest request = JsonSerializer.Deserialize<CancelAgentRunRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        Assert.AreEqual("thread_001", request.ThreadId);
        Assert.AreEqual("test-page", request.Source);
        Assert.AreEqual("manual_cancel", request.Reason);
    }

    [TestMethod]
    public void StreamEvent_ShouldSerializeMessageTextPayload()
    {
        AgentStreamEvent streamEvent = new()
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Seq = 1,
            Event = "message.delta",
            Payload = new Dictionary<string, object?>
            {
                ["text"] = "hello"
            }
        };

        string content = SseEventWriter.Format(streamEvent);

        StringAssert.Contains(content, "\"event\":\"message.delta\"");
        StringAssert.Contains(content, "\"payload\":{\"text\":\"hello\"}");
    }
}
