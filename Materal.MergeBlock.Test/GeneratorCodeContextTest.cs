using Materal.MergeBlock.GeneratorCode;

namespace Materal.MergeBlock.Test;

[TestClass]
public sealed class GeneratorCodeContextTest
{
    /// <summary>
    /// 生成代码测试
    /// </summary>
    /// <param name="projectPath"></param>
    [DataRow("E:\\Project\\Demo\\MMB\\MMB.Demo")]
    [TestMethod]
    public async Task GeneratorCodeTest(string projectPath)
    {
        await PlugHost.RunAsync(projectPath);
    }
    /// <summary>
    /// 测试GeneratorCodeContext初始化
    /// </summary>
    /// <param name="projectPath"></param>
    [DataRow("E:\\Project\\Demo\\MMB\\MMB.Demo")]
    [TestMethod]
    public void GeneratorCodeContextInitTest(string projectPath)
    {
        GeneratorCodeContext context = new(projectPath);
        context.Refresh();
    }

    /// <summary>
    /// 测试获取插件组
    /// </summary>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    [DataRow("E:\\Project\\Demo\\MMB\\MMB.Core\\MMB.Core.Abstractions")]
    [TestMethod]
    public void GetPlugsTest(string projectPath)
    {
        List<IMergeBlockGeneratorCodePlug> plugs = GeneratorCodeContext.GetPlugs(projectPath);
        Assert.IsNotEmpty(plugs);
    }

    /// <summary>
    /// 测试获取插件
    /// </summary>
    /// <param name="csharpCode"></param>
    [TestMethod]
    public void BuildPlugTest()
    {
        const string csharpCode = @"
using Materal.MergeBlock.GeneratorCode;

namespace MMB.Core.Abstractions.Code;

/// <summary>
/// 测试插件
/// </summary>
public class TestPlug : IMergeBlockGeneratorCodePlug
{
    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task ExcuteAsync(GeneratorCodeContext context)
    {
        throw new NotImplementedException();
    }
}
";
        IMergeBlockGeneratorCodePlug plug = GeneratorCodeContext.BuildPlug(csharpCode);
        Assert.IsNotNull(plug);
    }
}
