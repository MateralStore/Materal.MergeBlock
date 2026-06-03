using Materal.MergeBlock.AI.Web.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Materal.MergeBlock.AI.Web.Test.Controllers;

[TestClass]
public class AIAgentControllerTest
{
    [TestMethod]
    public void Controller_ShouldNotAllowAnonymousAccess()
    {
        object[] attributes = typeof(AIAgentController).GetCustomAttributes(typeof(AllowAnonymousAttribute), false);

        Assert.AreEqual(0, attributes.Length);
    }
}
