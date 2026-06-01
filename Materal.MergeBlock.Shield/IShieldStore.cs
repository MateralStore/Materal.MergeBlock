using Materal.MergeBlock.Shield.Models;

namespace Materal.MergeBlock.Shield;

/// <summary>
/// Shield状态存储
/// </summary>
public interface IShieldStore
{
    /// <summary>
    /// 获取来源封禁状态
    /// </summary>
    /// <param name="source">来源标识</param>
    /// <param name="now">当前时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>封禁状态</returns>
    ValueTask<ShieldBlockState> GetBlockStateAsync(string source, DateTimeOffset now, CancellationToken cancellationToken = default);

    /// <summary>
    /// 原子记录一次404命中，并在达到阈值时写入封禁状态
    /// </summary>
    /// <param name="source">来源标识</param>
    /// <param name="hitContext">命中上下文</param>
    /// <param name="options">Shield配置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>命中结果</returns>
    ValueTask<ShieldHitResult> RecordNotFoundAsync(string source, ShieldHitContext hitContext, ShieldOptions options, CancellationToken cancellationToken = default);
}
