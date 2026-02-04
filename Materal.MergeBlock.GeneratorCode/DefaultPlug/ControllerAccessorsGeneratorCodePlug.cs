using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 控制器访问器代码生成插件
/// </summary>
public class ControllerAccessorsGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <summary>
    /// 方法参数视图模型（用于模板渲染）
    /// </summary>
    private class MethodArgumentViewModel
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 参数类型（如：Guid、string、AddUserRequestModel）
        /// </summary>
        public string PredefinedType { get; set; } = string.Empty;
        /// <summary>
        /// 是否为简单类型（需要转换为字符串放入字典）
        /// </summary>
        public bool IsSimpleType { get; set; }
        /// <summary>
        /// 转换代码（如："name" 或 "id.ToString()"）
        /// </summary>
        public string ConvertCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// 方法视图模型（用于模板渲染）
    /// </summary>
    private class MethodViewModel
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 方法注释（XML文档注释中的summary内容）
        /// </summary>
        public string? Annotation { get; set; }
        /// <summary>
        /// 无Task返回类型（如：void、ResultModel、List&lt;UserDTO&gt;）
        /// </summary>
        public string NotTaskReturnType { get; set; } = string.Empty;
        /// <summary>
        /// 是否为异步方法（返回类型以Task开头）
        /// </summary>
        public bool IsAsync { get; set; }
        /// <summary>
        /// 方法是否有返回值（返回类型不是void）
        /// </summary>
        public bool HasReturnValue { get; set; }
        /// <summary>
        /// 参数列表
        /// </summary>
        public List<MethodArgumentViewModel> Arguments { get; set; } = [];
        /// <summary>
        /// 简单类型参数（需要放入字典）
        /// </summary>
        public List<MethodArgumentViewModel> SimpleArguments => [.. Arguments.Where(a => a.IsSimpleType)];
        /// <summary>
        /// 复杂类型参数（需要直接传递）
        /// </summary>
        public List<MethodArgumentViewModel> ComplexArguments => [.. Arguments.Where(a => !a.IsSimpleType)];
        /// <summary>
        /// 所有参数声明（如："Guid id, string name"）
        /// </summary>
        public string ArgumentsDeclaration => string.Join(", ", Arguments.Select(a => $"{a.PredefinedType} {a.Name}"));
    }

    /// <summary>
    /// 控制器视图模型（用于模板渲染）
    /// </summary>
    private class ControllerViewModel
    {
        /// <summary>
        /// 领域名称（如：User）
        /// </summary>
        public string DomainName { get; set; } = string.Empty;
        /// <summary>
        /// 控制器注释（XML文档注释中的summary内容）
        /// </summary>
        public string? Annotation { get; set; }
        /// <summary>
        /// 是否为泛型控制器（继承自IMergeBlockController）
        /// </summary>
        public bool IsGeneric { get; set; }
        /// <summary>
        /// using语句列表
        /// </summary>
        public List<string> Usings { get; set; } = [];
        /// <summary>
        /// 方法列表
        /// </summary>
        public List<MethodViewModel> Methods { get; set; } = [];
    }

    /// <summary>
    /// 服务集合扩展视图模型（用于模板渲染）
    /// </summary>
    private class ServiceCollectionExtensionsViewModel
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 控制器列表
        /// </summary>
        public List<ControllerViewModel> Controllers { get; set; } = [];
    }

    private static readonly string _controllerAccessorTemplate = @"
{{- for using in controller.Usings}}
using {{using}};
{{- end}}

namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.ControllerAccessors;

/// <summary>
/// {{controller.Annotation}}访问器
/// </summary>
{{if controller.IsGeneric}}
public partial class {{controller.DomainName}}ControllerAccessor(IServiceProvider serviceProvider) : BaseControllerAccessor<I{{controller.DomainName}}Controller, Add{{controller.DomainName}}RequestModel, Edit{{controller.DomainName}}RequestModel, Query{{controller.DomainName}}RequestModel, {{controller.DomainName}}DTO, {{controller.DomainName}}ListDTO>(serviceProvider), I{{controller.DomainName}}Controller
{{else}}
public partial class {{controller.DomainName}}ControllerAccessor(IServiceProvider serviceProvider) : BaseControllerAccessor(serviceProvider), I{{controller.DomainName}}Controller
{{end}}
{
    /// <summary>
    /// 项目名称
    /// </summary>
    public override string ProjectName => ""{{context.ProjectName}}"";
    /// <summary>
    /// 模块名称
    /// </summary>
    public override string ModuleName => ""{{context.ModuleName}}"";

{{- for method in controller.Methods}}
    /// <summary>
    {{- if method.Annotation}}
    /// {{method.Annotation}}
    {{- else}}
    /// {{method.Name}}
    {{- end}}
    /// </summary>
{{- for argument in method.Arguments}}
    /// <param name=""{{argument.Name}}""></param>
{{- end}}
{{- if method.HasReturnValue}}
    /// <returns></returns>
{{- end}}
{{- if method.IsAsync}}
    public async Task<{{method.NotTaskReturnType}}> {{method.Name}}({{method.ArgumentsDeclaration}})
        => await HttpHelper.SendAsync<I{{controller.DomainName}}Controller, {{method.NotTaskReturnType}}>(ProjectName, ModuleName, nameof({{method.Name}}), {{if method.SimpleArguments.Count > 0}}new() { {{- for arg in method.SimpleArguments}}[nameof({{arg.Name}})] = {{arg.ConvertCode}}{{if !for.last}}, {{end}}{{- end}} }{{else}}[]{{end}}{{if method.ComplexArguments.Count > 0}}, {{method.ComplexArguments | array.join ', '}}{{end}});
{{- else}}
    public {{method.NotTaskReturnType}} {{method.Name}}({{method.ArgumentsDeclaration}})
        => HttpHelper.SendAsync<I{{controller.DomainName}}Controller, {{method.NotTaskReturnType}}>(ProjectName, ModuleName, nameof({{method.Name}}), {{if method.SimpleArguments.Count > 0}}new() { {{- for arg in method.SimpleArguments}}[nameof({{arg.Name}})] = {{arg.ConvertCode}}{{if !for.last}}, {{end}}{{- end}} }{{else}}[]{{end}}{{if method.ComplexArguments.Count > 0}}, {{method.ComplexArguments | array.join ', '}}{{end}}).Result;
{{- end}}
{{- end}}
}";

    private static readonly string _serviceCollectionExtensionsTemplate = @"
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.ControllerAccessors
{
    /// <summary>
    /// 服务集合扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加控制器访问器
        /// </summary>
        /// <param name=""services""></param>
        public static void Add{{context.ModuleName}}ControllerAccessors(this IServiceCollection services)
        {
{{- for controller in context.Controllers}}
            services.TryAddSingleton<I{{controller.DomainName}}Controller, {{controller.DomainName}}ControllerAccessor>();
{{- end}}
        }
    }
}";

    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task ExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task AfterExcuteAsync(GeneratorCodeContext context)
    {
        foreach (IControllerModel controller in context.Controllers)
        {
            await GeneratorControllerAccessorAsync(context, controller);
        }
        await GeneratorControllerAccessorServiceCollectionExtensionsAsync(context, context.Controllers);
    }

    /// <summary>
    /// 生成控制器访问器
    /// </summary>
    /// <param name="context"></param>
    /// <param name="controller"></param>
    private async Task GeneratorControllerAccessorAsync(GeneratorCodeContext context, IControllerModel controller)
    {
        ControllerViewModel controllerViewModel = CreateControllerViewModel(controller);

        Template template = Template.Parse(_controllerAccessorTemplate);
        string codeContent = RenderTemplate(template, context, controllerViewModel);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "ControllerAccessors", $"{controller.DomainName}ControllerAccessor.cs");
    }

    /// <summary>
    /// 生成控制器访问器服务集合扩展
    /// </summary>
    /// <param name="context"></param>
    /// <param name="controllers"></param>
    private async Task GeneratorControllerAccessorServiceCollectionExtensionsAsync(GeneratorCodeContext context, List<IControllerModel> controllers)
    {
        ServiceCollectionExtensionsViewModel extensionsViewModel = new()
        {
            ProjectName = context.ProjectName,
            ModuleName = context.ModuleName,
            Controllers = [.. controllers.Select(CreateControllerViewModel)]
        };

        Template template = Template.Parse(_serviceCollectionExtensionsTemplate);
        string codeContent = RenderTemplate(template, context, extensionsViewModel);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "ControllerAccessors", "ServiceCollectionExtensions.cs");
    }

    /// <summary>
    /// 创建控制器视图模型
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    private static ControllerViewModel CreateControllerViewModel(IControllerModel controller)
    {
        return new ControllerViewModel
        {
            DomainName = controller.DomainName,
            Annotation = controller.Annotation?.Trim(),
            IsGeneric = controller.Interfaces.Any(i => i.StartsWith("IMergeBlockController<")),
            Usings = controller.Usings,
            Methods = [.. controller.Methods.Select(m => new MethodViewModel
            {
                Name = m.Name,
                Annotation = m.Annotation?.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Trim(),
                NotTaskReturnType = m.NotTaskReturnType,
                IsAsync = m.IsTaskReturnType,
                HasReturnValue = m.NotTaskReturnType != "void",
                Arguments = [.. m.Arguments.Select(a => CreateMethodArgumentViewModel(a))]
            })]
        };
    }

    /// <summary>
    /// 创建方法参数视图模型
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    private static MethodArgumentViewModel CreateMethodArgumentViewModel(MethodArgumentModel argument)
    {
        bool isSimpleType = argument.PredefinedType switch
        {
            "Guid" or "int" or "long" or "decimal" or "double" or "float" or "DateTime" or "bool" => true,
            "string" => true,
            _ => false
        };

        string convertCode = isSimpleType switch
        {
            true when argument.PredefinedType != "string" => $"{argument.Name}.ToString()",
            _ => argument.Name
        };

        return new MethodArgumentViewModel
        {
            Name = argument.Name,
            PredefinedType = argument.PredefinedType,
            IsSimpleType = isSimpleType,
            ConvertCode = convertCode
        };
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    /// <param name="template"></param>
    /// <param name="context"></param>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    private static string RenderTemplate(Template template, GeneratorCodeContext context, object viewModel)
    {
        ScriptObject scriptObject = new()
        {
            { "context", context },
            { "controller", viewModel }
        };

        TemplateContext templateContext = new()
        {
            MemberRenamer = member => member.Name,
            LoopLimit = int.MaxValue,
            StrictVariables = false
        };
        templateContext.PushGlobal(scriptObject);

        return template.Render(templateContext);
    }
}
