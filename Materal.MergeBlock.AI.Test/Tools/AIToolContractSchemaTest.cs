namespace Materal.MergeBlock.AI.Test.Tools;

[TestClass]
public class AIToolContractSchemaTest
{
    [TestMethod]
    public void Descriptor_ShouldStoreSchemaAndPermission()
    {
        AIToolDescriptor descriptor = new()
        {
            Name = "readClientState",
            Description = "Read client state",
            ExecutionMode = AIToolExecutionMode.Remote,
            PermissionLevel = AIToolPermissionLevel.Read,
            InputSchema = AIToolContractSchema.Object(
                new Dictionary<string, object?>
                {
                    ["maxItems"] = new Dictionary<string, object?>
                    {
                        ["type"] = "integer",
                        ["minimum"] = 1,
                        ["maximum"] = 100
                    }
                },
                ["maxItems"]),
            ResultSchema = AIToolContractSchema.GenericObject()
        };

        Assert.AreEqual(AIToolPermissionLevel.Read, descriptor.PermissionLevel);
        Assert.AreEqual("object", descriptor.InputSchema!.Schema["type"]);
        Assert.AreEqual("object", descriptor.ResultSchema!.Schema["type"]);
    }
}
