namespace Materal.MergeBlock.AI.Tools;

/// <summary>
/// AI工具注册表
/// </summary>
public class AIToolRegistry
{
    private readonly Dictionary<string, AIToolDescriptor> _tools = new(StringComparer.Ordinal);
    /// <summary>
    /// 工具列表
    /// </summary>
    public IReadOnlyCollection<AIToolDescriptor> Tools => _tools.Values;
    /// <summary>
    /// 注册工具
    /// </summary>
    /// <param name="descriptor">工具描述</param>
    public void Register(AIToolDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Name))
        {
            throw new ArgumentException("AI工具名称不能为空", nameof(descriptor));
        }
        _tools[descriptor.Name] = descriptor;
    }
    /// <summary>
    /// 获取工具
    /// </summary>
    /// <param name="name">工具名称</param>
    /// <returns>工具描述</returns>
    public AIToolDescriptor GetRequired(string name)
    {
        return _tools.TryGetValue(name, out AIToolDescriptor? descriptor)
            ? descriptor
            : throw new KeyNotFoundException($"未找到AI工具: {name}");
    }
}
