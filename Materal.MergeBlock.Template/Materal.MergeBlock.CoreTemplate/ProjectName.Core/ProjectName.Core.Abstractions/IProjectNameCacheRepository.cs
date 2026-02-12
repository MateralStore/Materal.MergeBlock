namespace ProjectName.Core.Abstractions;

/// <summary>
/// ProjectName缓存仓储接口
/// </summary>
/// <typeparam name="TDomain"></typeparam>
public interface IProjectNameCacheRepository<TDomain> : ICacheEFRepository<TDomain, Guid>
    where TDomain : BaseDomain, IDomain, IEntity<Guid>, new()
{
}
