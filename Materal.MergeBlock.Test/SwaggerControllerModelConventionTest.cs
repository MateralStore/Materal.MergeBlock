using Materal.MergeBlock.Swagger;
using Materal.MergeBlock.AI.Web;

namespace Materal.MergeBlock.Test;

[TestClass]
public class SwaggerControllerModelConventionTest
{
    [TestMethod]
    public void GetGroupName_ShouldUseFullAssemblyName()
    {
        string? groupName = SwaggerControllerModelConvention.GetGroupName(typeof(AIWebModule).Assembly);

        Assert.AreEqual("Materal.MergeBlock.AI.Web", groupName);
    }
}
