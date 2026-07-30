using Materal.MergeBlock.AI.Abstractions.Review;

namespace Materal.MergeBlock.AI.Test.Review;

[TestClass]
public class AIAgentPreExecutionReviewerTest
{
    [TestMethod]
    public void Approve_ShouldCreateApprovedResult()
    {
        AIAgentPreExecutionReviewResult result = AIAgentPreExecutionReviewResult.Approve("safe");

        Assert.IsTrue(result.Approved);
        Assert.AreEqual("approve", result.Decision);
        Assert.AreEqual("safe", result.Reason);
    }

    [TestMethod]
    public void Rejected_ShouldCreateRejectedResultWithAgentMessage()
    {
        AIAgentPreExecutionReviewResult result = AIAgentPreExecutionReviewResult.Rejected(
            "unsafe",
            "Rewrite the action");

        Assert.IsFalse(result.Approved);
        Assert.AreEqual("reject", result.Decision);
        Assert.AreEqual("unsafe", result.Reason);
        Assert.AreEqual("Rewrite the action", result.AgentErrorMessage);
    }
}
