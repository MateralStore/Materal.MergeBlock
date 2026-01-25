namespace ProjectName.ModuleName.Repository;

/// <summary>
/// ModuleName缓存仓储实现
/// </summary>
/// <typeparam name="TDomain"></typeparam>
public abstract class ModuleNameCacheRepositoryImpl<TDomain>(ModuleNameDBContext dbContext, ICacheHelper cacheHelper) : ProjectNameCacheRepositoryImpl<TDomain, ModuleNameDBContext>(dbContext, cacheHelper), IModuleNameCacheRepository<TDomain>
    where TDomain : BaseDomain, IDomain, IEntity<Guid>, new()
{
}