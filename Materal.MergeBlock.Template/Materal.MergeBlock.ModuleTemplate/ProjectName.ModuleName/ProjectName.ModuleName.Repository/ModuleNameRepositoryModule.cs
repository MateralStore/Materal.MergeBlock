namespace ProjectName.ModuleName.Repository;

/// <summary>
/// ModuleName仓储模块
/// </summary>
public class ModuleNameRepositoryModule() : ProjectNameRepositoryModule<ModuleNameDBContext>("ProjectName.ModuleName仓储模块")
{
    /// <summary>
    /// 配置键
    /// </summary>
    protected override string ConfigKey => "ModuleName:DBConfig";
}