namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI服务端工具
/// </summary>
public interface IAIServerTool
{
    /// <summary>
    /// 工具描述
    /// </summary>
    AIToolDescriptor Descriptor { get; }
    /// <summary>
    /// 执行工具
    /// </summary>
    Task<AIServerToolResult> ExecuteAsync(AIServerToolContext context);
}
