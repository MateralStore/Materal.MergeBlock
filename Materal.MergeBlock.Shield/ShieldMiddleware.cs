using System.Diagnostics.CodeAnalysis;
using Materal.MergeBlock.Shield.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Materal.MergeBlock.Shield;

/// <summary>
/// Shield中间件
/// </summary>
/// <param name="optionsMonitor">Shield配置</param>
/// <param name="store">Shield状态存储</param>
/// <param name="logger">日志记录器</param>
public class ShieldMiddleware(IOptionsMonitor<ShieldOptions> optionsMonitor, IShieldStore store, ILogger<ShieldMiddleware> logger) : IMiddleware
{
    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ShieldOptions options = optionsMonitor.CurrentValue;
        if (!options.Enable || !TryGetSource(context, out string? sourceValue) || ShouldSkip(context, options))
        {
            await next(context);
            return;
        }
        string source = sourceValue;

        DateTimeOffset now = DateTimeOffset.UtcNow;
        try
        {
            ShieldBlockState blockState = await store.GetBlockStateAsync(source, now, context.RequestAborted);
            if (blockState.IsBlocked)
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = NormalizeStatusCode(options.BlockedStatusCode);
                }
                logger.LogWarning("Shield blocked request. Source: {Source}, Path: {Path}, BlockedUntil: {BlockedUntil}", source, context.Request.Path.Value, blockState.BlockedUntil);
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Shield failed to read block state. Source: {Source}, Path: {Path}", source, context.Request.Path.Value);
            await next(context);
            return;
        }

        await next(context);
        if (context.Response.StatusCode != StatusCodes.Status404NotFound) return;

        try
        {
            ShieldHitContext hitContext = new(context.Request.Path.Value ?? string.Empty, context.Request.Method, context.Request.Headers.UserAgent.ToString(), DateTimeOffset.UtcNow);
            ShieldHitResult result = await store.RecordNotFoundAsync(source, hitContext, options, context.RequestAborted);
            logger.LogInformation("Shield recorded 404. Source: {Source}, Path: {Path}, HitCount: {HitCount}", source, hitContext.Path, result.HitCount);
            if (result.IsNewlyBlocked)
            {
                logger.LogWarning("Shield blocked source. Source: {Source}, HitCount: {HitCount}, BlockedUntil: {BlockedUntil}, FirstHitAt: {FirstHitAt}, LastHitAt: {LastHitAt}", source, result.HitCount, result.BlockedUntil, result.FirstHitAt, result.LastHitAt);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Shield failed to record 404. Source: {Source}, Path: {Path}", source, context.Request.Path.Value);
        }
    }

    private static bool TryGetSource(HttpContext context, [NotNullWhen(true)] out string? source)
    {
        source = context.Connection.RemoteIpAddress?.ToString();
        return !string.IsNullOrWhiteSpace(source);
    }

    private static bool ShouldSkip(HttpContext context, ShieldOptions options)
    {
        if (HttpMethods.IsOptions(context.Request.Method)) return true;
        if (!options.TrackAuthenticatedRequests && context.User.Identity?.IsAuthenticated == true) return true;
        string path = context.Request.Path.Value ?? string.Empty;
        if (IsWhiteListed(context, options)) return true;
        if (MatchesAnyPrefix(path, options.ExcludePathPrefixes)) return true;
        return !MatchesAnyPrefix(path, options.MonitorPathPrefixes);
    }

    private static bool IsWhiteListed(HttpContext context, ShieldOptions options)
    {
        string? ip = context.Connection.RemoteIpAddress?.ToString();
        return !string.IsNullOrWhiteSpace(ip) && options.WhiteListIPs.Any(m => string.Equals(m, ip, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesAnyPrefix(string path, IEnumerable<string> prefixes)
        => prefixes.Where(m => !string.IsNullOrWhiteSpace(m)).Select(NormalizePrefix).Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static string NormalizePrefix(string prefix)
    {
        prefix = prefix.Trim();
        return prefix.StartsWith('/') ? prefix : $"/{prefix}";
    }

    private static int NormalizeStatusCode(int statusCode)
        => statusCode is >= 100 and <= 599 ? statusCode : StatusCodes.Status429TooManyRequests;
}
