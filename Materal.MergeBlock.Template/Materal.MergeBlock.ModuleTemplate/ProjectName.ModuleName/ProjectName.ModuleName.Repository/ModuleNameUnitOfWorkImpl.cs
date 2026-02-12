namespace ProjectName.ModuleName.Repository;

/// <summary>
/// ModuleName工作单元实现
/// </summary>
/// <param name="context"></param>
/// <param name="serviceProvider"></param>
public class ModuleNameUnitOfWorkImpl(ModuleNameDBContext context, IServiceProvider serviceProvider) : ProjectNameUnitOfWorkImpl<ModuleNameDBContext>(context, serviceProvider), IModuleNameUnitOfWork, IScopedDependency<IModuleNameUnitOfWork>
{
}
