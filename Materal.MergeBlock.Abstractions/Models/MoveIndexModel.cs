namespace Materal.MergeBlock.Abstractions.Models;

/// <summary>
/// 移动索引模型
/// </summary>
public class MoveIndexModel
{
    /// <summary>
    /// 被移动实体的唯一标识
    /// </summary>
    [Required(ErrorMessage = "唯一标识为空")]
    public Guid SourceID { get; set; }
    /// <summary>
    /// 参照实体的唯一标识
    /// </summary>
    [Required(ErrorMessage = "目标唯一标识为空")]
    public Guid TargetID { get; set; }
    /// <summary>
    /// true=移动到目标之前，false=移动到目标之后
    /// </summary>
    public bool Before { get; set; } = false;
}
