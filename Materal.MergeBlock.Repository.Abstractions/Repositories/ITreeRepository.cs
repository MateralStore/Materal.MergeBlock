namespace Materal.MergeBlock.Repository.Abstractions.Repositories;

/// <summary>
/// 树仓储
/// </summary>
public interface ITreeRepository<TDomain>
    where TDomain : class, IEntity<Guid>, ITreeDomain
{
    /// <summary>
    /// 获取指定节点的递归子级
    /// </summary>
    /// <param name="parentID">父节点ID</param>
    /// <returns>所有递归子级</returns>
    List<TDomain> GetAllRecursiveChildren(Guid parentID);

    /// <summary>
    /// 获取指定节点的递归子级唯一标识
    /// </summary>
    /// <param name="parentID">父节点ID</param>
    /// <returns>所有递归子级ID</returns>
    List<Guid> GetAllRecursiveChildrenID(Guid parentID);
}