using Materal.MergeBlock.AI.Web.Persistence;

namespace Materal.MergeBlock.AI.Web.Test.Persistence;

[TestClass]
public class AgentTraceRedactorTest
{
    [TestMethod]
    public void Redact_ShouldRemoveSecretsAndTrimLongText()
    {
        Dictionary<string, object?> payload = new()
        {
            ["api_key"] = "secret",
            ["token"] = "token-value",
            ["text"] = new string('a', 3000),
            ["normal"] = "ok"
        };

        IReadOnlyDictionary<string, object?> result = AgentTraceRedactor.Redact(payload);

        Assert.IsFalse(result.ContainsKey("api_key"));
        Assert.IsFalse(result.ContainsKey("token"));
        Assert.AreEqual("ok", result["normal"]);
        Assert.IsTrue(((string)result["text"]!).Length <= 1024);
    }
}
