using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 控制器映射代码生成插件
/// </summary>
public class ControllerMapperGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
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
        /// 方法注释
        /// </summary>
        public string? Annotation { get; set; }

        /// <summary>
        /// HTTP方法类型（Get、Post、Put、Delete、Patch）
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// 是否允许匿名访问
        /// </summary>
        public bool IsAllowAnonymous { get; set; }

        /// <summary>
        /// 返回类型（不含Task）
        /// </summary>
        public string NotTaskReturnType { get; set; } = string.Empty;

        /// <summary>
        /// 是否异步方法
        /// </summary>
        public bool IsTaskReturnType { get; set; }

        /// <summary>
        /// 是否有返回值（不是void）
        /// </summary>
        public bool HasReturnValue { get; set; }

        /// <summary>
        /// 是否为分页返回类型
        /// </summary>
        public bool IsPageReturnType { get; set; }

        /// <summary>
        /// 分页返回列表类型（仅分页类型有效）
        /// </summary>
        public string? PageResultListType { get; set; }

        /// <summary>
        /// 参数列表
        /// </summary>
        public List<MethodArgumentViewModel> Arguments { get; set; } = [];

        /// <summary>
        /// 需要映射的参数
        /// </summary>
        public List<MethodArgumentViewModel> MapperArguments => [.. Arguments.Where(a => a.NeedMap)];

        /// <summary>
        /// 方法参数声明
        /// </summary>
        public string ArgumentsDeclaration => string.Join(", ", Arguments.Select(a => $"{a.RequestPredefinedType} {a.RequestName}"));

        /// <summary>
        /// 使用的参数名
        /// </summary>
        public string UseArgumentsDeclaration => string.Join(", ", Arguments.Select(a => a.Name));
    }

    /// <summary>
    /// 方法参数视图模型（用于模板渲染）
    /// </summary>
    private class MethodArgumentViewModel
    {
        /// <summary>
        /// 原始名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 请求名称
        /// </summary>
        public string RequestName { get; set; } = string.Empty;

        /// <summary>
        /// 原始类型
        /// </summary>
        public string PredefinedType { get; set; } = string.Empty;

        /// <summary>
        /// 请求类型
        /// </summary>
        public string RequestPredefinedType { get; set; } = string.Empty;

        /// <summary>
        /// 是否需要映射
        /// </summary>
        public bool NeedMap { get; set; }
    }

    /// <summary>
    /// 服务视图模型（用于模板渲染）
    /// </summary>
    private class ServiceViewModel
    {
        /// <summary>
        /// 领域名称
        /// </summary>
        public string DomainName { get; set; } = string.Empty;

        /// <summary>
        /// 服务注释
        /// </summary>
        public string? Annotation { get; set; }

        /// <summary>
        /// using语句列表
        /// </summary>
        public List<string> Usings { get; set; } = [];

        /// <summary>
        /// 映射方法列表
        /// </summary>
        public List<MethodViewModel> Methods { get; set; } = [];
    }

    private static readonly string _iControllerMapperTemplate = @"
{{- for using in service.Usings}}
using {{using}};
{{- end }}

namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Controllers;

/// <summary>
/// {{service.Annotation}}控制器
/// </summary>
public partial interface I{{service.DomainName}}Controller : IMergeBlockController
{
{{- for method in service.Methods}}
    /// <summary>
    /// {{method.Annotation}}
    /// </summary>
{{- for argument in method.Arguments}}
    /// <param name=""{{argument.RequestName}}""></param>
{{- end}}
{{- if method.NotTaskReturnType != ""void""}}
    /// <returns></returns>
{{- end}}
{{- if method.IsAllowAnonymous}}
    [Http{{method.HttpMethod}}, AllowAnonymous]
{{- else}}
    [Http{{method.HttpMethod}}]
{{- end}}
{{- if method.IsTaskReturnType}}
{{- if method.IsPageReturnType}}
    Task<CollectionResultModel<{{method.PageResultListType}}>> {{method.Name}}({{method.ArgumentsDeclaration}});
{{- else if method.NotTaskReturnType == ""void""}}
    Task<ResultModel> {{method.Name}}({{method.ArgumentsDeclaration}});
{{- else}}
    Task<ResultModel<{{method.NotTaskReturnType}}>> {{method.Name}}({{method.ArgumentsDeclaration}});
{{- end}}
{{- else}}
{{- if method.IsPageReturnType}}
    CollectionResultModel<{{method.PageResultListType}}> {{method.Name}}({{method.ArgumentsDeclaration}});
{{- else if method.NotTaskReturnType == ""void""}}
    ResultModel {{method.Name}}({{method.ArgumentsDeclaration}});
{{- else}}
    ResultModel<{{method.NotTaskReturnType}}> {{method.Name}}({{method.ArgumentsDeclaration}});
{{- end}}
{{- end}}
{{- end}}
}
";

    private static readonly string _controllerMapperTemplate = @"
{{- for using in service.Usings}}
using {{using}};
{{- end }}

namespace {{context.ProjectName}}.{{context.ModuleName}}.Application.Controllers;

/// <summary>
/// {{service.Annotation}}控制器
/// </summary>
public partial class {{service.DomainName}}Controller
{
{{- for method in service.Methods}}
    /// <summary>
    /// {{method.Annotation}}
    /// </summary>
{{- for argument in method.Arguments}}
    /// <param name=""{{argument.RequestName}}""></param>
{{- end}}
{{- if method.NotTaskReturnType != ""void""}}
    /// <returns></returns>
{{- end}}
{{- if method.IsAllowAnonymous}}
    [Http{{method.HttpMethod}}, AllowAnonymous]
{{- else}}
    [Http{{method.HttpMethod}}]
{{- end}}
{{- if method.IsTaskReturnType}}
{{- if method.IsPageReturnType}}
    public async Task<CollectionResultModel<{{method.PageResultListType}}>> {{method.Name}}({{method.ArgumentsDeclaration}})
    {
    {{- for arg in method.MapperArguments}}
        {{arg.PredefinedType}} {{arg.Name}} = Mapper.Map<{{arg.PredefinedType}}>({{arg.RequestName}}) ?? throw new {{context.ProjectName}}Exception(""映射失败"");
    {{- end}}
        (List<{{method.PageResultListType}}> result, RangeModel rangeInfo) = await DefaultService.{{method.Name}}({{method.UseArgumentsDeclaration}});
        return CollectionResultModel<{{method.PageResultListType}}>.Success(result, rangeInfo, ""{{method.Annotation}}成功"");
    }
{{- else if method.NotTaskReturnType == ""void""}}
    public async Task<ResultModel> {{method.Name}}({{method.ArgumentsDeclaration}})
    {
    {{- for arg in method.MapperArguments}}
        {{arg.PredefinedType}} {{arg.Name}} = Mapper.Map<{{arg.PredefinedType}}>({{arg.RequestName}}) ?? throw new {{context.ProjectName}}Exception(""映射失败"");
    {{- end}}
        await DefaultService.{{method.Name}}({{method.UseArgumentsDeclaration}});
        return ResultModel.Success(""{{method.Annotation}}成功"");
    }
{{- else}}
    public async Task<ResultModel<{{method.NotTaskReturnType}}>> {{method.Name}}({{method.ArgumentsDeclaration}})
    {
    {{- for arg in method.MapperArguments}}
        {{arg.PredefinedType}} {{arg.Name}} = Mapper.Map<{{arg.PredefinedType}}>({{arg.RequestName}}) ?? throw new {{context.ProjectName}}Exception(""映射失败"");
    {{- end}}
        {{method.NotTaskReturnType}} result = await DefaultService.{{method.Name}}({{method.UseArgumentsDeclaration}});
        return ResultModel<{{method.NotTaskReturnType}}>.Success(result, ""{{method.Annotation}}成功"");
    }
{{- end}}
{{- else}}

{{- if method.IsPageReturnType}}
    public CollectionResultModel<{{method.PageResultListType}}> {{method.Name}}({{method.ArgumentsDeclaration}})
    {
    {{- for arg in method.MapperArguments}}
        {{arg.PredefinedType}} {{arg.Name}} = Mapper.Map<{{arg.PredefinedType}}>({{arg.RequestName}}) ?? throw new {{context.ProjectName}}Exception(""映射失败"");
    {{- end}}
        (List<{{method.PageResultListType}}> result, RangeModel rangeInfo) = DefaultService.{{method.Name}}({{method.UseArgumentsDeclaration}});
        return CollectionResultModel<{{method.PageResultListType}}>.Success(result, rangeInfo, ""{{method.Annotation}}成功"");
    }
{{- else if method.NotTaskReturnType == ""void""}}
    public ResultModel {{method.Name}}({{method.ArgumentsDeclaration}})
    {
    {{- for arg in method.MapperArguments}}
        {{arg.PredefinedType}} {{arg.Name}} = Mapper.Map<{{arg.PredefinedType}}>({{arg.RequestName}}) ?? throw new {{context.ProjectName}}Exception(""映射失败"");
    {{- end}}
        DefaultService.{{method.Name}}({{method.UseArgumentsDeclaration}});
        return ResultModel.Success(""{{method.Annotation}}成功"");
    }
{{- else}}
    public ResultModel<{{method.NotTaskReturnType}}> {{method.Name}}({{method.ArgumentsDeclaration}})
    {
    {{- for arg in method.MapperArguments}}
        {{arg.PredefinedType}} {{arg.Name}} = Mapper.Map<{{arg.PredefinedType}}>({{arg.RequestName}}) ?? throw new {{context.ProjectName}}Exception(""映射失败"");
    {{- end}}
        {{method.NotTaskReturnType}} result = DefaultService.{{method.Name}}({{method.UseArgumentsDeclaration}});
        return ResultModel<{{method.NotTaskReturnType}}>.Success(result, ""{{method.Annotation}}成功"");
    }
{{- end}}
{{- end}}
{{- end}}
}
";

    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ExcuteAsync(GeneratorCodeContext context)
    {
        foreach (IServiceModel service in context.Services)
        {
            await GeneratorIControllerMapperCodeAsync(context, service);
            await GeneratorControllerMapperCodeAsync(context, service);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <summary>
    /// 创建控制器代码接口
    /// </summary>
    private async Task GeneratorIControllerMapperCodeAsync(GeneratorCodeContext context, IServiceModel service)
    {
        if (!service.HasMapperMethod) return;
        ServiceViewModel serviceViewModel = CreateServiceViewModel(context, service);
        serviceViewModel.Usings = GetUniqueUsings(context, service);

        Template template = Template.Parse(_iControllerMapperTemplate);
        string codeContent = RenderTemplate(template, context, serviceViewModel);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Controllers", $"I{service.DomainName}Controller.Mapper.cs");
    }

    /// <summary>
    /// 创建控制器代码实现
    /// </summary>
    private async Task GeneratorControllerMapperCodeAsync(GeneratorCodeContext context, IServiceModel service)
    {
        if (!service.HasMapperMethod) return;
        ServiceViewModel serviceViewModel = CreateServiceViewModel(context, service);
        serviceViewModel.Usings = GetFullUsings(context, service);

        Template template = Template.Parse(_controllerMapperTemplate);
        string codeContent = RenderTemplate(template, context, serviceViewModel);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Controllers", $"{service.DomainName}Controller.Mapper.cs");
    }

    /// <summary>
    /// 创建服务视图模型
    /// </summary>
    private static ServiceViewModel CreateServiceViewModel(GeneratorCodeContext context, IServiceModel service)
    {
        var methods = new List<MethodViewModel>();
        foreach (MethodModel method in service.Methods)
        {
            AttributeModel? attribute = method.Attributes.GetAttribute<MapperControllerAttribute>();
            string? httpMethodValue = attribute?.GetAttributeArgument()?.Value;
            if (attribute is null || httpMethodValue is null) continue;

            string httpMethod = httpMethodValue switch
            {
                "MapperType.Get" => "Get",
                "MapperType.Post" => "Post",
                "MapperType.Put" => "Put",
                "MapperType.Delete" => "Delete",
                "MapperType.Patch" => "Patch",
                _ => string.Empty
            };

            string? isAllowAnonymous = attribute.GetAttributeArgument(nameof(MapperControllerAttribute.IsAllowAnonymous))?.Value;
            bool allowAnonymous = isAllowAnonymous is not null && isAllowAnonymous.Equals("true", StringComparison.OrdinalIgnoreCase);

            bool isPageReturn = IsPageReturnType(method.NotTaskReturnType, out PageReturnTypeModel pageReturn);

            var methodViewModel = new MethodViewModel
            {
                Name = method.Name,
                Annotation = method.Annotation?.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Trim() ?? method.Name,
                HttpMethod = httpMethod,
                IsAllowAnonymous = allowAnonymous,
                NotTaskReturnType = method.NotTaskReturnType,
                IsTaskReturnType = method.IsTaskReturnType,
                HasReturnValue = method.ReturnType != "void",
                IsPageReturnType = isPageReturn,
                PageResultListType = isPageReturn ? pageReturn.PageResultListType : null,
                Arguments = [.. method.Arguments.Select(a => new MethodArgumentViewModel
                {
                    Name = a.Name,
                    RequestName = a.RequestName,
                    PredefinedType = a.PredefinedType,
                    RequestPredefinedType = a.RequestPredefinedType,
                    NeedMap = a.RequestName != a.Name
                })]
            };
            methods.Add(methodViewModel);
        }

        return new ServiceViewModel
        {
            DomainName = service.DomainName,
            Annotation = service.Annotation,
            Methods = methods
        };
    }

    /// <summary>
    /// 获取唯一using列表（接口）
    /// </summary>
    private static List<string> GetUniqueUsings(GeneratorCodeContext context, IServiceModel service)
    {
        var addedUsings = new HashSet<string>();
        foreach (string usingCode in service.Usings)
        {
            string trueUsingCode = usingCode;
            if (trueUsingCode.Contains($"{context.ProjectName}.{context.ModuleName}.Abstractions.Services.Models"))
            {
                trueUsingCode = trueUsingCode.Replace("Services.Models", "RequestModel");
            }
            addedUsings.Add(trueUsingCode);
        }
        return [.. addedUsings];
    }

    /// <summary>
    /// 获取完整using列表（实现）
    /// </summary>
    private static List<string> GetFullUsings(GeneratorCodeContext context, IServiceModel service)
    {
        var addedUsings = new HashSet<string>();
        foreach (string usingCode in service.Usings)
        {
            string trueUsingCode = usingCode;
            if (trueUsingCode.Contains($"{context.ProjectName}.{context.ModuleName}.Abstractions.Services.Models"))
            {
                if (addedUsings.Add(trueUsingCode))
                {
                    // 保留原始Services.Models的using
                }
                trueUsingCode = trueUsingCode.Replace("Services.Models", "RequestModel");
            }
            addedUsings.Add(trueUsingCode);
        }
        return [.. addedUsings];
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    private static string RenderTemplate(Template template, GeneratorCodeContext context, ServiceViewModel service)
    {
        ScriptObject scriptObject = new()
        {
            { "context", context },
            { "service", service }
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

    /// <summary>
    /// 是分页返回类型
    /// </summary>
    private static bool IsPageReturnType(string notTaskReturnType, out PageReturnTypeModel pageReturn)
    {
        try
        {
            string code = notTaskReturnType;
            pageReturn = new();
            if (!code.StartsWith("(List<") || !code.EndsWith(')')) return false;
            code = code[1..^1];
            string[] temps = code.Split(',');
            if (temps.Length != 2) return false;
            string left = temps[0].Trim();
            string right = temps[1].Trim();
            pageReturn.LeftType = left[..left.IndexOf(' ')].Trim();
            pageReturn.LeftName = left[left.IndexOf(' ')..].Trim();
            pageReturn.RightType = right[..right.IndexOf(' ')].Trim();
            pageReturn.RightName = right[right.IndexOf(' ')..].Trim();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"解析代码失败:\r\n{notTaskReturnType}", ex);
        }
    }

    /// <summary>
    /// 分页返回类型
    /// </summary>
    private class PageReturnTypeModel
    {
        /// <summary>
        /// 左侧类型
        /// </summary>
        public string LeftType { get; set; } = string.Empty;

        /// <summary>
        /// 分页返回列表类型
        /// </summary>
        public string PageResultListType => LeftType[5..^1];

        /// <summary>
        /// 左侧名称
        /// </summary>
        public string LeftName { get; set; } = string.Empty;

        /// <summary>
        /// 右侧类型
        /// </summary>
        public string RightType { get; set; } = string.Empty;

        /// <summary>
        /// 右侧名称
        /// </summary>
        public string RightName { get; set; } = string.Empty;
    }
}
