namespace Materal.MergeBlock.Shield.Models;

/// <summary>
/// Shield命中结果
/// </summary>
/// <param name="HitCount">当前窗口命中次数</param>
/// <param name="IsBlocked">是否已封禁</param>
/// <param name="IsNewlyBlocked">是否本次新触发封禁</param>
/// <param name="FirstHitAt">当前窗口首次命中时间</param>
/// <param name="LastHitAt">当前窗口最后命中时间</param>
/// <param name="BlockedUntil">封禁结束时间</param>
public readonly record struct ShieldHitResult(int HitCount, bool IsBlocked, bool IsNewlyBlocked, DateTimeOffset? FirstHitAt, DateTimeOffset? LastHitAt, DateTimeOffset? BlockedUntil);
