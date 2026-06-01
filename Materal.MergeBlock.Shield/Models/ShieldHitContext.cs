namespace Materal.MergeBlock.Shield.Models;

/// <summary>
/// Shield命中上下文
/// </summary>
/// <param name="Path">请求路径</param>
/// <param name="Method">HTTP方法</param>
/// <param name="UserAgent">User-Agent</param>
/// <param name="Timestamp">命中时间</param>
public readonly record struct ShieldHitContext(string Path, string Method, string? UserAgent, DateTimeOffset Timestamp);
