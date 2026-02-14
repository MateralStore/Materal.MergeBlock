namespace Materal.MergeBlock.Abstractions.Models;

/// <summary>
/// 移动树形节点并排序模型
/// </summary>
public class MoveTreeNodeAndIndexRequestModel
{
    /// <summary>
    /// 被移动实体的唯一标识
    /// </summary>
    [Required(ErrorMessage = "唯一标识为空")]
    public Guid SourceID { get; set; }
    /// <summary>
    /// 目标父级唯一标识（可空，为空时表示不更改父级）
    /// </summary>
    public Guid? ParentID { get; set; }
    /// <summary>
    /// 参照实体的唯一标识（可空）
    /// </summary>
    public Guid? TargetID { get; set; }
    /// <summary>
    /// true=移动到目标之前，false=移动到目标之后
    /// </summary>
    public bool Before { get; set; } = false;
}
