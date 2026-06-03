using Microsoft.Extensions.AI;

namespace MMB.Demo.Application.AI;

/// <summary>
/// Demo本地AI工具
/// </summary>
public static class DemoLocalAITools
{
    /// <summary>
    /// 获取本地工具列表
    /// </summary>
    public static IList<AITool> CreateTools()
    {
        return
        [
            AIFunctionFactory.Create(
                GetCurrentServerTime,
                new AIFunctionFactoryOptions
                {
                    Name = "getCurrentServerTime",
                    Description = "获取MMB.Demo.WebAPI服务器当前时间和托管环境信息。"
                })
        ];
    }

    private static IReadOnlyDictionary<string, object?> GetCurrentServerTime()
    {
        return new Dictionary<string, object?>
        {
            ["server_time"] = DateTimeOffset.Now.ToString("O"),
            ["utc_time"] = DateTimeOffset.UtcNow.ToString("O"),
            ["thread_id"] = Environment.CurrentManagedThreadId,
            ["machine_name"] = Environment.MachineName
        };
    }
}
