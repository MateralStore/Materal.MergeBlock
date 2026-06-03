namespace Materal.MergeBlock.AI.Abstractions.Context;

/// <summary>
/// AI上下文快照
/// </summary>
public sealed class AIContextSnapshot : IReadOnlyAIContext
{
    private readonly IReadOnlyDictionary<string, object?> _items;
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="items">上下文项目</param>
    public AIContextSnapshot(IDictionary<string, object?> items)
    {
        _items = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(items));
    }
    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Items => _items;
    /// <inheritdoc />
    public bool TryGetValue(string key, out object? value) => _items.TryGetValue(key, out value);
    /// <inheritdoc />
    public T? Get<T>(string key) => TryGetValue(key, out object? value) && value is T result ? result : default;
    /// <inheritdoc />
    public T GetRequired<T>(string key)
    {
        T? value = Get<T>(key);
        return value ?? throw new KeyNotFoundException($"未找到AI上下文项: {key}");
    }
}
