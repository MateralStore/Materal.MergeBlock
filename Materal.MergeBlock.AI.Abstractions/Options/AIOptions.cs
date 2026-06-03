namespace Materal.MergeBlock.AI.Abstractions.Options;

/// <summary>
/// AI配置
/// </summary>
public class AIOptions : IOptions
{
    /// <summary>
    /// 配置节点
    /// </summary>
    public const string ConfigKey = "MergeBlock:AI";
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; } = true;
    /// <summary>
    /// 默认Agent名称
    /// </summary>
    public string DefaultAgentName { get; set; } = "default";
    /// <summary>
    /// 是否扫描工具
    /// </summary>
    public bool ScanTools { get; set; } = true;
    /// <summary>
    /// 是否要求工具授权
    /// </summary>
    public bool RequireToolAuthorization { get; set; } = true;
    /// <summary>
    /// 是否审计工具调用
    /// </summary>
    public bool AuditToolCalls { get; set; } = true;
}
