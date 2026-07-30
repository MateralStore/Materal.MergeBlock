namespace Materal.MergeBlock.AI.Test.Context;

[TestClass]
public class AIContextSnapshotTest
{
    [TestMethod]
    public void Snapshot_ShouldNotChange_WhenSourceDictionaryChanges()
    {
        Dictionary<string, object?> source = new()
        {
            ["permissions"] = new[] { "content.read" }
        };

        AIContextSnapshot snapshot = new(source);
        source["permissions"] = new[] { "content.edit" };

        string[] permissions = snapshot.GetRequired<string[]>("permissions");

        CollectionAssert.AreEqual(new[] { "content.read" }, permissions);
    }

    [TestMethod]
    public void Items_ShouldBeReadOnly()
    {
        AIContextSnapshot snapshot = new(new Dictionary<string, object?>
        {
            ["tenantId"] = "tenant-001"
        });

        Assert.AreEqual("tenant-001", snapshot.GetRequired<string>("tenantId"));
        Assert.IsTrue(snapshot.Items is not IDictionary<string, object?> writable || writable.IsReadOnly);
    }
}
