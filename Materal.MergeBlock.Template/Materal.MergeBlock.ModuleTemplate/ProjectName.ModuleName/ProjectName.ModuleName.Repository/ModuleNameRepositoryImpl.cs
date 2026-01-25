namespace ProjectName.ModuleName.Repository;

/// <summary>
/// ModuleName仓储实现
/// </summary>
/// <typeparam name="TDomain"></typeparam>
public abstract class ModuleNameRepositoryImpl<TDomain>(ModuleNameDBContext dbContext) : ProjectNameRepositoryImpl<TDomain, ModuleNameDBContext>(dbContext), IModuleNameRepository<TDomain>
    where TDomain : BaseDomain, IDomain, IEntity<Guid>, new()
{
}