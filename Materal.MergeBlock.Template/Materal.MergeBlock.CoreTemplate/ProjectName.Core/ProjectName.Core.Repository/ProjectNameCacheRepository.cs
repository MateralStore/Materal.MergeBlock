using Materal.Utils.Caching;

namespace ProjectName.Core.Repository;

/// <summary>
/// ProjectName缓存仓储实现
/// </summary>
/// <typeparam name="TDomain"></typeparam>
/// <typeparam name="TDBContext"></typeparam>
public abstract class ProjectNameCacheRepositoryImpl<TDomain, TDBContext>(TDBContext dbContext, ICacheHelper cacheHelper) : SqlServerCacheEFRepositoryImpl<TDomain, Guid, TDBContext>(dbContext, cacheHelper), IProjectNameCacheRepository<TDomain>
    where TDomain : BaseDomain, IDomain, IEntity<Guid>, new()
    where TDBContext : DbContext
{
}