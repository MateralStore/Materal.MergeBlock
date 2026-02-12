using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;

namespace Materal.MergeBlock.GeneratorCode;

public partial class GeneratorCodeContext
{
    private List<IMergeBlockGeneratorCodePlug> GetCustomPlugs()
    {
        List<IMergeBlockGeneratorCodePlug> coreAbstractionsPlugs = GetPlugs(CoreAbstractionsPath);
        List<IMergeBlockGeneratorCodePlug> coreRepositoryPlugs = GetPlugs(CoreRepositoryPath);
        List<IMergeBlockGeneratorCodePlug> coreApplicationPlugs = GetPlugs(CoreApplicationPath);
        List<IMergeBlockGeneratorCodePlug> moduleAbstractionsPlugs = GetPlugs(ModuleAbstractionsPath);
        List<IMergeBlockGeneratorCodePlug> moduleRepositoryPlugs = GetPlugs(ModuleRepositoryPath);
        List<IMergeBlockGeneratorCodePlug> moduleApplicationPlugs = GetPlugs(ModuleApplicationPath);
        return [
                .. coreAbstractionsPlugs,
                .. coreRepositoryPlugs,
                .. coreApplicationPlugs,
                .. moduleAbstractionsPlugs,
                .. moduleRepositoryPlugs,
                .. moduleApplicationPlugs,
            ];
    }

    /// <summary>
    /// 获得插件组
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    internal static List<IMergeBlockGeneratorCodePlug> GetPlugs(string directoryPath)
    {
        DirectoryInfo directoryInfo = new(directoryPath);
        return GetPlugs(directoryInfo);
    }

    /// <summary>
    /// 获得插件组
    /// </summary>
    /// <param name="directoryInfo"></param>
    /// <returns></returns>
    internal static List<IMergeBlockGeneratorCodePlug> GetPlugs(DirectoryInfo directoryInfo)
    {
        List<IMergeBlockGeneratorCodePlug> result = [];
        if (directoryInfo.Name.Equals("Debug", StringComparison.OrdinalIgnoreCase)) return result;
        if (directoryInfo.Name.Equals("Obj", StringComparison.OrdinalIgnoreCase)) return result;
        foreach (FileInfo file in directoryInfo.GetFiles("*.cs"))
        {
            string fileContent = File.ReadAllText(file.FullName);
            if (!IsGeneratorCodePlugScriptCode(fileContent)) continue;
            IMergeBlockGeneratorCodePlug plug = BuildPlug(fileContent);
            result.Add(plug);
        }
        foreach (DirectoryInfo item in directoryInfo.GetDirectories())
        {
            List<IMergeBlockGeneratorCodePlug> temp = GetPlugs(item);
            result.AddRange(temp);
        }
        return result;
    }
    private readonly static string GeneratorCodePlugName = typeof(IMergeBlockGeneratorCodePlug).Name;

    /// <summary>
    /// 是生成代码插件的脚本代码
    /// </summary>
    /// <param name="csharpCode"></param>
    /// <returns></returns>
    private static bool IsGeneratorCodePlugScriptCode(string csharpCode)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
        CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
        IEnumerable<ClassDeclarationSyntax> classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (ClassDeclarationSyntax classDeclaration in classDeclarations)
        {
            if (classDeclaration.BaseList == null) continue;
            foreach (BaseTypeSyntax baseType in classDeclaration.BaseList.Types)
            {
                string typeName = baseType.Type.ToString();
                if (typeName == GeneratorCodePlugName || typeName == $"global::{typeof(IMergeBlockGeneratorCodePlug).FullName}") return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 构建插件
    /// </summary>
    /// <param name="csharpCode"></param>
    /// <returns></returns>
    internal static IMergeBlockGeneratorCodePlug BuildPlug(string csharpCode)
    {
        csharpCode = $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Materal.MergeBlock.GeneratorCode;
{csharpCode}
";
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
        string assemblyName = $"DynamicPlug_{Guid.NewGuid():N}";
        List<MetadataReference> references = GetMetadataReferences();
        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, [syntaxTree], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using MemoryStream ms = new();
        EmitResult result = compilation.Emit(ms);
        if (!result.Success)
        {
            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
            string errors = string.Join(Environment.NewLine, failures.Select(diagnostic => $"{diagnostic.Id}: {diagnostic.GetMessage()}"));
            throw new InvalidOperationException($"编译失败:{Environment.NewLine}{errors}");
        }
        ms.Seek(0, SeekOrigin.Begin);
        Assembly assembly = Assembly.Load(ms.ToArray());
        Type plugType = assembly.GetTypeByFilter(m => typeof(IMergeBlockGeneratorCodePlug).IsAssignableFrom(m) && !m.IsInterface && !m.IsAbstract) ?? throw new InvalidOperationException($"未找到实现 {GeneratorCodePlugName} 接口的类");
        IMergeBlockGeneratorCodePlug plug = plugType.Instantiation<IMergeBlockGeneratorCodePlug>();
        return plug;
    }

    /// <summary>
    /// 获取元数据引用
    /// </summary>
    /// <returns></returns>
    private static List<MetadataReference> GetMetadataReferences()
    {
        List<MetadataReference> references = [];
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location)) continue;
            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }
        return references;
    }
}
