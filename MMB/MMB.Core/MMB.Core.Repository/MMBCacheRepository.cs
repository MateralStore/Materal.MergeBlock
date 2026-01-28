using Materal.Utils.Caching;

namespace MMB.Core.Repository;

/// <summary>
/// MMB缓存仓储实现
/// </summary>
/// <typeparam name="TDomain"></typeparam>
/// <typeparam name="TDBContext"></typeparam>
public abstract class MMBCacheRepositoryImpl<TDomain, TDBContext>(TDBContext dbContext, ICacheHelper cacheHelper) : SqlServerCacheEFRepositoryImpl<TDomain, Guid, TDBContext>(dbContext, cacheHelper), IMMBCacheRepository<TDomain>
    where TDomain : BaseDomain, IDomain, IEntity<Guid>, new()
    where TDBContext : DbContext
{
}