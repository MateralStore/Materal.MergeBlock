namespace Materal.MergeBlock.AI.Test.Tools;

[TestClass]
public class AIToolDescriptorTest
{
    [TestMethod]
    public void Descriptor_ShouldPreserveExecutionMode()
    {
        AIToolDescriptor local = new()
        {
            Name = "queryOrder",
            Description = "查询订单",
            ExecutionMode = AIToolExecutionMode.Local,
            InputType = typeof(QueryOrderInput),
            ResultType = typeof(QueryOrderResult)
        };

        AIToolDescriptor remote = new()
        {
            Name = "runClientAction",
            Description = "执行客户端动作",
            ExecutionMode = AIToolExecutionMode.Remote,
            InputType = typeof(RunClientActionInput),
            ResultType = typeof(Dictionary<string, object?>)
        };

        Assert.AreEqual(AIToolExecutionMode.Local, local.ExecutionMode);
        Assert.AreEqual(AIToolExecutionMode.Remote, remote.ExecutionMode);
    }

    private sealed class QueryOrderInput;
    private sealed class QueryOrderResult;
    private sealed class RunClientActionInput;
}
