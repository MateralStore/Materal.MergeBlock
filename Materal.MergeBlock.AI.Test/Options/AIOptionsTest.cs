namespace Materal.MergeBlock.AI.Test.Options;

[TestClass]
public class AIOptionsTest
{
    [TestMethod]
    public void NewOptions_ShouldUseSafeDefaults()
    {
        AIOptions options = new();

        bool enable = ReadEnable(options);
        bool scanTools = ReadScanTools(options);
        bool requireToolAuthorization = ReadRequireToolAuthorization(options);
        bool auditToolCalls = ReadAuditToolCalls(options);

        Assert.AreEqual(true, enable);
        Assert.AreEqual("default", options.DefaultAgentName);
        Assert.AreEqual(true, scanTools);
        Assert.AreEqual(true, requireToolAuthorization);
        Assert.AreEqual(true, auditToolCalls);
    }

    private static bool ReadEnable(AIOptions options) => options.Enable;
    private static bool ReadScanTools(AIOptions options) => options.ScanTools;
    private static bool ReadRequireToolAuthorization(AIOptions options) => options.RequireToolAuthorization;
    private static bool ReadAuditToolCalls(AIOptions options) => options.AuditToolCalls;
}
