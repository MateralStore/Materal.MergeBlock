namespace ProjectName.ModuleName.Abstractions;

/// <summary>
/// ModuleName仓储接口
/// </summary>
/// <typeparam name="TDomain"></typeparam>
public interface IModuleNameRepository<TDomain> : IProjectNameRepository<TDomain>
    where TDomain : BaseDomain, IDomain, IEntity<Guid>, new()
{
}