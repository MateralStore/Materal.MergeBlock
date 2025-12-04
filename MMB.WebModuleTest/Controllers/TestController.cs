using Materal.Extensions;
using Materal.MergeBlock.Web.Abstractions.Controllers;
using Materal.Utils.Model;
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
    /// 测试流式
    /// </summary>
    [HttpGet]
    public async IAsyncEnumerable<string> TestStreamingAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return $"chunk {i}\n";
            await Task.Delay(500); // 每500ms返回一个
        }
    }
    /// <summary>
    /// 测试流式异常
    /// </summary>
    [HttpGet]
    public async IAsyncEnumerable<string> TestStreamingExceptionAsync()
    {
        int count = 10;
        for (int i = 0; i < count; i++)
        {
            yield return $"chunk {i}\n";
            await Task.Delay(100); // 每500ms返回一个
            if (i == count - 2)
            {
                throw new Exception("测试流式异常");
            }
        }
    }
    /// <summary>
    /// 测试Json
    /// </summary>
    [HttpPost]
    public ResultModel<TestModel> TestJson(TestModel model) => ResultModel<TestModel>.Success(model, "获取成功");

    /// <summary>
    /// 测试ResultModel序列化
    /// </summary>
    [HttpGet]
    public string TestResultModelSerialization()
    {
        ResultModel resultModel = ResultModel.Fail("测试消息");
        string errorJson = resultModel.ToJson();
        return errorJson;
    }
}
