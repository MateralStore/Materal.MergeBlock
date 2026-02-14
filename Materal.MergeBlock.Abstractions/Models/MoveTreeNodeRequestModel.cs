namespace Materal.MergeBlock.Abstractions.Models;

/// <summary>
/// 移动节点请求模型
/// </summary>
public class MoveTreeNodeRequestModel
{
    /// <summary>
    /// 被移动实体的唯一标识
    /// </summary>
    [Required(ErrorMessage = "唯一标识为空")]
    public Guid SourceID { get; set; }
    /// <summary>
    /// 参照实体的唯一标识
    /// </summary>
    public Guid? TargetID { get; set; }
}
