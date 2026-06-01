namespace Materal.MergeBlock.Shield.Models;

/// <summary>
/// Shield封禁状态
/// </summary>
/// <param name="IsBlocked">是否已封禁</param>
/// <param name="BlockedUntil">封禁结束时间</param>
public readonly record struct ShieldBlockState(bool IsBlocked, DateTimeOffset? BlockedUntil);
