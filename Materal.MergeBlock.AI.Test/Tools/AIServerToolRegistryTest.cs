namespace Materal.MergeBlock.AI.Test.Tools;

[TestClass]
public class AIServerToolRegistryTest
{
    [TestMethod]
    public async Task ExecuteAsync_ShouldInvokeRegisteredServerTool()
    {
        AIServerToolRegistry registry = new([new EchoServerTool()]);

        AIServerToolResult result = await registry.ExecuteAsync(
            "echoServer",
            "thread_001",
            "run_001",
            new Dictionary<string, object?> { ["value"] = "hello" },
            CancellationToken.None);

        Assert.AreEqual(AIToolCallStatus.Completed, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("hello", result.Result["value"]);
    }

    [TestMethod]
    public async Task ExecuteAsync_ShouldReturnFailedResult_WhenToolIsMissing()
    {
        AIServerToolRegistry registry = new([]);

        AIServerToolResult result = await registry.ExecuteAsync(
            "missingTool",
            "thread_001",
            "run_001",
            new Dictionary<string, object?>(),
            CancellationToken.None);

        Assert.AreEqual(AIToolCallStatus.Failed, result.Status);
        Assert.IsNotNull(result.Error);
        Assert.AreEqual("tool_not_found", result.Error["code"]);
    }

    private sealed class EchoServerTool : IAIServerTool
    {
        public AIToolDescriptor Descriptor { get; } = new()
        {
            Name = "echoServer",
            Description = "Echo input",
            ExecutionMode = AIToolExecutionMode.Local,
            PermissionLevel = AIToolPermissionLevel.Edit
        };

        public Task<AIServerToolResult> ExecuteAsync(AIServerToolContext context)
        {
            return Task.FromResult(AIServerToolResult.Completed(context.Arguments));
        }
    }
}
