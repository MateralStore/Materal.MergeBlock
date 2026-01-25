namespace ProjectName.ModuleName.Abstractions;

/// <summary>
/// ModuleName缓存仓储接口
/// </summary>
/// <typeparam name="TDomain"></typeparam>
public interface IModuleNameCacheRepository<TDomain> : IProjectNameCacheRepository<TDomain>
    where TDomain : BaseDomain, IDomain, IEntity<Guid>, new()
{
}