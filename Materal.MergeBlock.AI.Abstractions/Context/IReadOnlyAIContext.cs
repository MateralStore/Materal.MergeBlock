namespace Materal.MergeBlock.AI.Abstractions.Context;

/// <summary>
/// 只读AI上下文
/// </summary>
public interface IReadOnlyAIContext
{
    /// <summary>
    /// 上下文项目
    /// </summary>
    IReadOnlyDictionary<string, object?> Items { get; }
    /// <summary>
    /// 尝试获取值
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns>是否存在</returns>
    bool TryGetValue(string key, out object? value);
    /// <summary>
    /// 获取值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键</param>
    /// <returns>值</returns>
    T? Get<T>(string key);
    /// <summary>
    /// 获取必需值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键</param>
    /// <returns>值</returns>
    T GetRequired<T>(string key);
}
