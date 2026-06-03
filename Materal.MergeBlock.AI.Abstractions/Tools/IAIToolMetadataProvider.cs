namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI工具元数据提供器
/// </summary>
public interface IAIToolMetadataProvider
{
    /// <summary>
    /// 获取工具描述
    /// </summary>
    /// <returns>工具描述</returns>
    IEnumerable<AIToolDescriptor> GetToolDescriptors();
}
