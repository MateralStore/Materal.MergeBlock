using Materal.MergeBlock.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Materal.MergeBlock.Shield;

/// <summary>
/// Shield配置
/// </summary>
[Options(ConfigKey)]
public class ShieldOptions : IOptions
{
    /// <summary>
    /// 配置节点
    /// </summary>
    public const string ConfigKey = "MergeBlock:Shield";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// 监控路径前缀
    /// </summary>
    public string[] MonitorPathPrefixes { get; set; } = ["/api"];

    /// <summary>
    /// 排除路径前缀
    /// </summary>
    public string[] ExcludePathPrefixes { get; set; } = [];

    /// <summary>
    /// IP白名单
    /// </summary>
    public string[] WhiteListIPs { get; set; } = [];

    /// <summary>
    /// 404统计时间窗口秒数
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// 时间窗口内允许的404次数
    /// </summary>
    public int NotFoundLimit { get; set; } = 10;

    /// <summary>
    /// 封禁秒数
    /// </summary>
    public int BlockedSeconds { get; set; } = 600;

    /// <summary>
    /// 被拦截时返回的HTTP状态码
    /// </summary>
    public int BlockedStatusCode { get; set; } = StatusCodes.Status429TooManyRequests;

    /// <summary>
    /// 是否统计已认证请求
    /// </summary>
    public bool TrackAuthenticatedRequests { get; set; }
}
