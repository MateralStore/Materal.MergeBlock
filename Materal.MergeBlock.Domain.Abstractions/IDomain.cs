namespace Materal.MergeBlock.Domain.Abstractions
{
    /// <summary>
    /// Domain
    /// </summary>
    public interface IDomain : IEntity<Guid>
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        DateTime UpdateTime { get; set; }
    }
}
