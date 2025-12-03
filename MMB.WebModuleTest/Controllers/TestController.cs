using Materal.MergeBlock.Web.Abstractions.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MMB.WebModuleTest.Controllers;

/// <summary>
/// 测试控制器
/// </summary>
[AllowAnonymous]
public class TestController : MergeBlockController
{
    /// <summary>
    /// 测试流式（验证 IAsyncEnumerable 是否正常工作）
    /// </summary>
    [HttpGet]
    public async IAsyncEnumerable<string> TestStreaming()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return $"chunk {i}\n";
            await Task.Delay(500); // 每500ms返回一个
        }
    }
    /// <summary>
    /// 测试Json
    /// </summary>
    [HttpPost]
    public TestModel TestJson(TestModel model) => model;
}
