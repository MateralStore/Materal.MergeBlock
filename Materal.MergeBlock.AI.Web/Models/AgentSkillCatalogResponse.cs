namespace Materal.MergeBlock.AI.Web.Models;

/// <summary>
/// Agent Skill目录响应
/// </summary>
public class AgentSkillCatalogResponse
{
    /// <summary>
    /// 契约版本
    /// </summary>
    public string SchemaVersion { get; init; } = "agent-skill-catalog-v1";
    /// <summary>
    /// Skill列表
    /// </summary>
    public IReadOnlyList<AgentSkillCatalogItem> Skills { get; init; } = [];
}

/// <summary>
/// Agent Skill目录项
/// </summary>
public class AgentSkillCatalogItem
{
    /// <summary>
    /// Skill ID
    /// </summary>
    public string Id { get; init; } = string.Empty;
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// AI Agent Skill目录提供器
/// </summary>
public interface IAIAgentSkillCatalogProvider
{
    /// <summary>
    /// 获取Skill目录
    /// </summary>
    /// <returns>Skill目录项</returns>
    IReadOnlyList<AgentSkillCatalogItem> GetSkills();
}
