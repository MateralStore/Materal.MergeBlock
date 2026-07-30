namespace Materal.MergeBlock.AI.Test.Tools;

[TestClass]
public class AIToolScannerTest
{
    [TestMethod]
    public void Scan_ShouldDiscoverLocalAndRemoteTools()
    {
        AIToolScanner scanner = new();

        IReadOnlyList<AIToolDescriptor> descriptors = scanner.Scan(typeof(LocalOrderTool).Assembly);

        AIToolDescriptor local = descriptors.Single(m => m.Name == nameof(LocalOrderTool));
        AIToolDescriptor remote = descriptors.Single(m => m.Name == "runClientAction");

        Assert.AreEqual(AIToolExecutionMode.Local, local.ExecutionMode);
        Assert.AreEqual(AIToolExecutionMode.Remote, remote.ExecutionMode);
    }

    [TestMethod]
    public void Scan_ShouldDiscoverMethodTools()
    {
        AIToolScanner scanner = new();

        IReadOnlyList<AIToolDescriptor> descriptors = scanner.Scan(typeof(MethodToolContainer).Assembly);

        AIToolDescriptor descriptor = descriptors.Single(m => m.Name == "getOrder");
        Assert.AreEqual("查询订单明细", descriptor.Description);
        Assert.AreEqual(AIToolExecutionMode.Local, descriptor.ExecutionMode);
        Assert.AreEqual(typeof(MethodToolContainer), descriptor.ImplementationType);
    }

    [MergeBlockAITool("查询订单")]
    private sealed class LocalOrderTool;

    [MergeBlockAITool("执行客户端动作", Name = "runClientAction", ExecutionMode = AIToolExecutionMode.Remote)]
    private sealed class RemoteClientTool;

    private sealed class MethodToolContainer
    {
        [MergeBlockAITool("查询订单明细", Name = "getOrder")]
        public string GetOrder() => "order";
    }
}
