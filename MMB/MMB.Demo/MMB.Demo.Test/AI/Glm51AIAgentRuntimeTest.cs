using MMB.Demo.Application.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace MMB.Demo.Test.AI;

[TestClass]
public class Glm51AIAgentRuntimeTest
{
    [TestMethod]
    public void Glm51AgentOptions_ShouldIncludePromptInjectionGuard()
    {
        Glm51AgentOptions options = new();

        StringAssert.Contains(options.Instructions, "不要泄露");
        StringAssert.Contains(options.Instructions, "系统提示词");
        StringAssert.Contains(options.Instructions, "密钥");
        StringAssert.Contains(options.Instructions, "忽略");
        StringAssert.Contains(options.Instructions, "工具");
    }

    [TestMethod]
    public async Task DemoLocalAITools_ShouldExposeServerTimeTool()
    {
        AITool tool = DemoLocalAITools.CreateTools().Single(m => m.Name == "getCurrentServerTime");

        object? result = await ((AIFunction)tool).InvokeAsync(new AIFunctionArguments());

        Assert.IsNotNull(result);
        StringAssert.Contains(result.ToString(), "server_time");
        StringAssert.Contains(result.ToString(), "thread_id");
    }

    [TestMethod]
    public async Task RunAsync_ShouldReturnError_WhenApiKeyIsMissing()
    {
        Glm51AIAgentRuntime runtime = new(
            Options.Create(new Glm51AIOptions { ApiKey = string.Empty }),
            new RecordingAgentRunner());

        List<AIAgentRunOutput> outputs = await CollectAsync(runtime.RunAsync(CreateRunRequest("hello")));

        Assert.AreEqual(1, outputs.Count);
        Assert.AreEqual(AIAgentRunOutputType.Error, outputs[0].Type);
        StringAssert.Contains(outputs[0].ErrorMessage, "MergeBlock:AI:GLM51:ApiKey");
    }

    [TestMethod]
    public async Task RunAsync_ShouldStreamMafAgentOutput_WhenConfigured()
    {
        RecordingAgentRunner runner = new("你好", "，世界");
        Glm51AIAgentRuntime runtime = new(
            Options.Create(new Glm51AIOptions { ApiKey = "key" }),
            runner);

        List<AIAgentRunOutput> outputs = await CollectAsync(runtime.RunAsync(CreateRunRequest("hello")));

        Assert.AreEqual(3, outputs.Count);
        Assert.AreEqual("你好", outputs[0].Text);
        Assert.AreEqual("，世界", outputs[1].Text);
        Assert.AreEqual(AIAgentRunOutputType.RunCompleted, outputs[2].Type);
        Assert.AreEqual("hello", runner.LastMessage);
    }

    [TestMethod]
    public async Task RunAsync_ShouldStreamDeterministicBasicDemo_WhenApiKeyIsMissing()
    {
        Glm51AIAgentRuntime runtime = new(
            Options.Create(new Glm51AIOptions { ApiKey = string.Empty }),
            new RecordingAgentRunner());

        List<AIAgentRunOutput> outputs = await CollectAsync(runtime.RunAsync(CreateRunRequest("demo-basic-chat")));

        Assert.AreEqual(2, outputs.Count);
        Assert.AreEqual(AIAgentRunOutputType.MessageDelta, outputs[0].Type);
        Assert.AreEqual("Demo response", outputs[0].Text);
        Assert.AreEqual(AIAgentRunOutputType.RunCompleted, outputs[1].Type);
    }

    [TestMethod]
    public async Task RunAsync_ShouldStreamDeterministicSlowDemo_WhenApiKeyIsMissing()
    {
        Glm51AIAgentRuntime runtime = new(
            Options.Create(new Glm51AIOptions { ApiKey = string.Empty }),
            new RecordingAgentRunner());

        List<AIAgentRunOutput> outputs = await CollectAsync(runtime.RunAsync(CreateRunRequest("slow-stream")));

        Assert.IsTrue(outputs.Count >= 3);
        Assert.IsTrue(outputs.Take(outputs.Count - 1).All(m => m.Type == AIAgentRunOutputType.MessageDelta));
        Assert.AreEqual(AIAgentRunOutputType.RunCompleted, outputs[^1].Type);
    }

    [TestMethod]
    public async Task RunAsync_ShouldRequestRemoteTool_WhenTriggerIsPresent()
    {
        Glm51AIAgentRuntime runtime = new(
            Options.Create(new Glm51AIOptions { ApiKey = string.Empty }),
            new RecordingAgentRunner());

        List<AIAgentRunOutput> outputs = await CollectAsync(runtime.RunAsync(CreateRunRequest("use-remote-tool")));

        Assert.AreEqual(AIAgentRunOutputType.ToolCallRequested, outputs[0].Type);
        Assert.AreEqual("runClientAction", outputs[0].ToolName);
        Assert.AreEqual(AIAgentRunOutputType.RunPaused, outputs[1].Type);
        Assert.AreEqual("tool_result_required", outputs[1].Reason);
        Assert.IsNotNull(outputs[1].Metadata);
        CollectionAssert.Contains(((IEnumerable<object>)outputs[1].Metadata!["tool_call_ids"]!).ToList(), outputs[0].ToolCallId);
    }

    [TestMethod]
    public async Task ResumeAsync_ShouldCompleteAfterRemoteToolResult()
    {
        Glm51AIAgentRuntime runtime = new(
            Options.Create(new Glm51AIOptions { ApiKey = "key" }),
            new RecordingAgentRunner());

        List<AIAgentRunOutput> outputs = await CollectAsync(runtime.ResumeAsync(new AIAgentResumeRequest
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            ToolResults =
            [
                new AIAgentRemoteToolResult
                {
                    ToolCallId = "call_001",
                    Status = AIToolCallStatus.Completed,
                    Result = new Dictionary<string, object?> { ["ok"] = true }
                }
            ],
            AIContext = new AIContextSnapshot(new Dictionary<string, object?>()),
            SystemMessages = [],
            CancellationToken = CancellationToken.None
        }));

        Assert.AreEqual("Remote tool completed", outputs[0].Text);
        Assert.AreEqual(AIAgentRunOutputType.RunCompleted, outputs[1].Type);
    }

    private static AIAgentRunRequest CreateRunRequest(string message) => new()
    {
        ThreadId = "thread_001",
        RunId = "run_001",
        Message = message,
        AIContext = new AIContextSnapshot(new Dictionary<string, object?>()),
        SystemMessages = [],
        CancellationToken = CancellationToken.None
    };

    private static async Task<List<AIAgentRunOutput>> CollectAsync(IAsyncEnumerable<AIAgentRunOutput> outputs)
    {
        List<AIAgentRunOutput> result = [];
        await foreach (AIAgentRunOutput output in outputs)
        {
            result.Add(output);
        }
        return result;
    }

    private sealed class RecordingAgentRunner(params string[] deltas) : IGlm51AgentRunner
    {
        public string? LastMessage { get; private set; }

        public async IAsyncEnumerable<string> RunStreamingAsync(Glm51AgentRunRequest request)
        {
            LastMessage = request.Message;
            foreach (string delta in deltas)
            {
                yield return delta;
                await Task.Yield();
            }
        }
    }
}
