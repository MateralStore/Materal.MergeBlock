using ProjectName.ModuleName.Repository;

namespace ProjectName.ModuleName.Application;

/// <summary>
/// ModuleName模块
/// </summary>
[DependsOn(typeof(ModuleNameRepositoryModule))]
public class ModuleNameModule() : ProjectNameModule("ProjectNameModuleName模块")
{
}
