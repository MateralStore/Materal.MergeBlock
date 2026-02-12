namespace ProjectName.Core.Abstractions;

/// <summary>
/// ProjectName仓储接口
/// </summary>
/// <typeparam name="TDomain"></typeparam>
public interface IProjectNameRepository<TDomain> : IEFRepository<TDomain, Guid>
    where TDomain : BaseDomain, IDomain, IEntity<Guid>, new()
{
}
