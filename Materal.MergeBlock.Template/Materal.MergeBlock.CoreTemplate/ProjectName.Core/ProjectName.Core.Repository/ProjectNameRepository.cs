namespace ProjectName.Core.Repository;

/// <summary>
/// ProjectName仓储实现
/// </summary>
/// <typeparam name="TDomain"></typeparam>
/// <typeparam name="TDBContext"></typeparam>
public abstract class ProjectNameRepositoryImpl<TDomain, TDBContext>(TDBContext dbContext) : SqlServerEFRepositoryImpl<TDomain, Guid, TDBContext>(dbContext), IProjectNameRepository<TDomain>
    where TDomain : BaseDomain, IDomain, IEntity<Guid>, new()
    where TDBContext : DbContext
{
}