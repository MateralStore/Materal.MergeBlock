using System.Diagnostics;
using System.Reflection;

namespace Materal.MergeBlock.GeneratorCode;

/// <summary>
/// 生成代码插件上下文
/// </summary>
public partial class GeneratorCodeContext
{
    /// <summary>
    /// MGC文件夹名称
    /// </summary>
    public const string MGCDirectoryName = "MGC";
    /// <summary>
    /// 核心抽象层路径
    /// </summary>
    public string CoreAbstractionsPath { get; }
    /// <summary>
    /// 核心抽象层MGC路径
    /// </summary>
    public string CoreAbstractionsMGCPath { get; }
    /// <summary>
    /// 核心仓储路径
    /// </summary>
    public string CoreRepositoryPath { get; }
    /// <summary>
    /// 核心仓储MGC路径
    /// </summary>
    public string CoreRepositoryMGCPath { get; }
    /// <summary>
    /// 核心应用层路径
    /// </summary>
    public string CoreApplicationPath { get; }
    /// <summary>
    /// 核心应用层MGC路径
    /// </summary>
    public string CoreApplicationMGCPath { get; }
    /// <summary>
    /// 模块抽象层路径
    /// </summary>
    public string ModuleAbstractionsPath { get; }
    /// <summary>
    /// 模块抽象层MGC路径
    /// </summary>
    public string ModuleAbstractionsMGCPath { get; }
    /// <summary>
    /// 模块应用层路径
    /// </summary>
    public string ModuleApplicationPath { get; }
    /// <summary>
    /// 模块应用层MGC路径
    /// </summary>
    public string ModuleApplicationMGCPath { get; }
    /// <summary>
    /// 模块仓储路径
    /// </summary>
    public string ModuleRepositoryPath { get; }
    /// <summary>
    /// 模块仓储MGC路径
    /// </summary>
    public string ModuleRepositoryMGCPath { get; }
    /// <summary>
    /// 模块WebAPI路径
    /// </summary>
    public string ModuleWebAPIPath { get; }
    /// <summary>
    /// 模块WebAPIMGC路径
    /// </summary>
    public string ModuleWebAPIMGCPath { get; }
    /// <summary>
    /// 项目名称
    /// </summary>
    public string ProjectName { get; }
    /// <summary>
    /// 模块名称
    /// </summary>
    public string ModuleName { get; }
    /// <summary>
    /// 生成代码插件
    /// </summary>
    public List<IMergeBlockGeneratorCodePlug> GeneratorCodePlugs { get; }
    /// <summary>
    /// 领域
    /// </summary>
    public List<DomainModel> Domains { get; set; } = [];
    /// <summary>
    /// 服务
    /// </summary>
    public List<IServiceModel> Services { get; set; } = [];
    /// <summary>
    /// 控制器
    /// </summary>
    public List<IControllerModel> Controllers { get; set; } = [];
    /// <summary>
    /// 枚举
    /// </summary>
    public List<EnumModel> Enums { get; set; } = [];

    /// <summary>
    /// 构造方法
    /// </summary>
    public GeneratorCodeContext(string projectPath)
    {
        DirectoryInfo projectDir = new(projectPath);
        int lastDotIndex = projectDir.Name.LastIndexOf('.');
        if (lastDotIndex == -1) throw new ArgumentException("项目路径格式不正确，应为 {ProjectName}.{ModuleName}", nameof(projectPath));
        string projectName = projectDir.Name[..lastDotIndex];
        string moduleName = projectDir.Name[(lastDotIndex + 1)..];
        string projectRootPath = projectDir.Parent?.FullName ?? throw new ArgumentException("无效的项目路径", nameof(projectPath));
        CoreAbstractionsPath = Path.Combine(projectRootPath, $"{projectName}.Core", $"{projectName}.Core.Abstractions");
        CoreAbstractionsMGCPath = Path.Combine(CoreAbstractionsPath, MGCDirectoryName);
        CoreRepositoryPath = Path.Combine(projectRootPath, $"{projectName}.Core", $"{projectName}.Core.Repository");
        CoreRepositoryMGCPath = Path.Combine(CoreRepositoryPath, MGCDirectoryName);
        CoreApplicationPath = Path.Combine(projectRootPath, $"{projectName}.Core", $"{projectName}.Core.Application");
        CoreApplicationMGCPath = Path.Combine(CoreApplicationPath, MGCDirectoryName);
        ModuleAbstractionsPath = Path.Combine(projectPath, $"{projectDir.Name}.Abstractions");
        ModuleAbstractionsMGCPath = Path.Combine(ModuleAbstractionsPath, MGCDirectoryName);
        ModuleRepositoryPath = Path.Combine(projectPath, $"{projectDir.Name}.Repository");
        ModuleRepositoryMGCPath = Path.Combine(ModuleRepositoryPath, MGCDirectoryName);
        ModuleApplicationPath = Path.Combine(projectPath, $"{projectDir.Name}.Application");
        ModuleApplicationMGCPath = Path.Combine(ModuleApplicationPath, MGCDirectoryName);
        ModuleWebAPIPath = Path.Combine(projectPath, $"{projectDir.Name}.WebAPI");
        ModuleWebAPIMGCPath = Path.Combine(ModuleWebAPIPath, MGCDirectoryName);
        ProjectName = projectName;
        ModuleName = moduleName;
        GeneratorCodePlugs = GetAllPlugs();
    }

    private List<IMergeBlockGeneratorCodePlug> GetAllPlugs()
    {
        List<IMergeBlockGeneratorCodePlug> defaultPlugs = GetDefaultPlugs();
        List<IMergeBlockGeneratorCodePlug> customPlugs = GetCustomPlugs();
        return [.. defaultPlugs, .. customPlugs];
    }

    private List<IMergeBlockGeneratorCodePlug> GetDefaultPlugs()
    {
        IEnumerable<Type> types = typeof(IMergeBlockGeneratorCodePlug).Assembly.GetTypesByFilter(m => m.IsAssignableTo<IMergeBlockGeneratorCodePlug>() && m.IsClass);
        List<IMergeBlockGeneratorCodePlug> result = [];
        foreach (Type type in types)
        {
            IMergeBlockGeneratorCodePlug plug = type.Instantiation<IMergeBlockGeneratorCodePlug>();
            result.Add(plug);
        }
        return result;
    }

    /// <summary>
    /// 保存为文件
    /// </summary>
    /// <param name="stringBuilder"></param>
    /// <param name="directoryPath"></param>
    /// <param name="paths"></param>
    public void SaveAs(StringBuilder stringBuilder, string directoryPath, params string[] paths)
    {
        if (paths.Length < 1) return;
        DirectoryInfo directoryInfo = new(directoryPath);
        string filePath = directoryInfo.FullName;
        for (int i = 0; i < paths.Length - 1; i++)
        {
            filePath = Path.Combine(filePath, paths[i]);
        }
        directoryInfo = new(filePath);
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
            directoryInfo.Refresh();
        }
        filePath = Path.Combine(filePath, paths[^1]);

        StackTrace stackTrace = new(true);
        StackFrame? callerFrame = stackTrace.GetFrame(1);
        if (callerFrame != null)
        {
            MethodBase? method = callerFrame.GetMethod();
            if (method != null)
            {
                string typeName = method.DeclaringType?.FullName ?? "Unknown";
                string methodName = method.Name;
                stringBuilder.Insert(0, $"/* Generated by: {typeName}.{methodName} */\r\n");
            }
        }
        string content = stringBuilder.ToString();
        File.WriteAllText(filePath, content, Encoding.UTF8);
    }

    /// <summary>
    /// 删除所有MGC文件夹
    /// </summary>
    public void DeleteAllMGCDirectorys()
    {
        DeleteMGCDirectorys(CoreAbstractionsMGCPath);
        DeleteMGCDirectorys(CoreRepositoryMGCPath);
        DeleteMGCDirectorys(CoreApplicationMGCPath);
        DeleteMGCDirectorys(ModuleAbstractionsMGCPath);
        DeleteMGCDirectorys(ModuleRepositoryMGCPath);
        DeleteMGCDirectorys(ModuleApplicationMGCPath);
        DeleteMGCDirectorys(ModuleWebAPIMGCPath);
    }

    /// <summary>
    /// 删除MGC文件夹
    /// </summary>
    public void DeleteMGCDirectorys(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 刷新
    /// </summary>
    public void Refresh()
    {
        DirectoryInfo directoryInfo = new(ModuleAbstractionsPath);
        List<CSharpCodeFileModel> models = GetCSharpCodeFileModels(directoryInfo);
        Domains = [.. models.OfType<DomainModel>()];
        Domains = CSharpFileParser.Merge(Domains);
        Services = [.. models.OfType<IServiceModel>()];
        Services = CSharpFileParser.Merge(Services);
        Controllers = [.. models.OfType<IControllerModel>()];
        Controllers = CSharpFileParser.Merge(Controllers);
        Enums = [.. models.OfType<EnumModel>()];
        Enums = CSharpFileParser.Merge(Enums);
    }

    private static List<CSharpCodeFileModel> GetCSharpCodeFileModels(DirectoryInfo directoryInfo)
    {
        List<CSharpCodeFileModel> models = [];
        foreach (DirectoryInfo? item in directoryInfo.GetDirectories())
        {
            if (item is null) continue;
            if (item.Name == "Domain")
            {
                models.AddRange(GetCSharpCodeFileModels(item, fileInfo => fileInfo.Name.EndsWith(".cs")));
            }
            else if (item.Name == "Controllers")
            {
                models.AddRange(GetCSharpCodeFileModels(item, fileInfo => fileInfo.Name.StartsWith('I') && fileInfo.Name.Contains("Controller.") && fileInfo.Name.EndsWith(".cs")));
            }
            else if (item.Name == "Services")
            {
                models.AddRange(GetCSharpCodeFileModels(item, fileInfo => fileInfo.Name.StartsWith('I') && fileInfo.Name.Contains("Service.") && fileInfo.Name.EndsWith(".cs")));
            }
            else if (item.Name == "Enums")
            {
                models.AddRange(GetCSharpCodeFileModels(item, fileInfo => fileInfo.Name.EndsWith(".cs")));
            }
            else if (item.Name == "MGC")
            {
                models.AddRange(GetCSharpCodeFileModels(item));
            }
        }
        return models;
    }

    private static List<CSharpCodeFileModel> GetCSharpCodeFileModels(DirectoryInfo directoryInfo, Func<FileInfo, bool> isTargetFile)
    {
        List<CSharpCodeFileModel> models = [];
        foreach (DirectoryInfo? item in directoryInfo.GetDirectories())
        {
            if (item is null) continue;
            models.AddRange(GetCSharpCodeFileModels(item, isTargetFile));
        }
        foreach (FileInfo? item in directoryInfo.GetFiles())
        {
            if (item is null || !isTargetFile(item)) continue;
            models.AddRange(CSharpFileParser.ParseByFilePath(item.FullName));
        }
        return models;
    }
}
