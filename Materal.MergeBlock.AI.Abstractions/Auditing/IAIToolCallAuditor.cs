namespace Materal.MergeBlock.AI.Abstractions.Auditing;

/// <summary>
/// AI工具调用审计器
/// </summary>
public interface IAIToolCallAuditor
{
    /// <summary>
    /// 审计
    /// </summary>
    /// <param name="context">上下文</param>
    /// <returns></returns>
    Task AuditAsync(AIToolCallAuditContext context);
}
