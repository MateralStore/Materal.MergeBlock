using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 控制器代码生成插件
/// </summary>
public class ControllerGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <summary>
    /// 控制器视图模型（用于模板渲染）
    /// </summary>
    private class ControllerViewModel
    {
        /// <summary>
        /// 领域模型名称（如：User）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 领域模型注释（XML文档注释中的summary内容）
        /// </summary>
        public string? Annotation { get; set; }

        /// <summary>
        /// 是否为空控制器（只有基本接口）
        /// </summary>
        public bool IsEmptyController { get; set; }

        /// <summary>
        /// 是否有服务特性
        /// </summary>
        public bool HasNotServiceAttribute { get; set; }
    }

    private static readonly string _iControllerTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Controllers;

/// <summary>
/// {{domain.Annotation}}控制器
/// </summary>
public {{if domain.IsEmptyController}}partial interface I{{domain.Name}}Controller : IMergeBlockController{{else}}partial interface I{{domain.Name}}Controller : IMergeBlockController<Add{{domain.Name}}RequestModel, Edit{{domain.Name}}RequestModel, Query{{domain.Name}}RequestModel, {{domain.Name}}DTO, {{domain.Name}}ListDTO>{{end}}
{
}
";

    private static readonly string _controllerTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Application.Controllers;

/// <summary>
/// {{domain.Annotation}}控制器
/// </summary>
public {{if domain.IsEmptyController}}
    {{- if domain.HasNotServiceAttribute}}
partial class {{domain.Name}}Controller : {{context.ModuleName}}Controller, I{{domain.Name}}Controller
    {{- else}}
partial class {{domain.Name}}Controller : {{context.ModuleName}}Controller<I{{domain.Name}}Service>, I{{domain.Name}}Controller
    {{- end}}
{{else}}
partial class {{domain.Name}}Controller : {{context.ModuleName}}Controller<Add{{domain.Name}}RequestModel, Edit{{domain.Name}}RequestModel, Query{{domain.Name}}RequestModel, Add{{domain.Name}}Model, Edit{{domain.Name}}Model, Query{{domain.Name}}Model, {{domain.Name}}DTO, {{domain.Name}}ListDTO, I{{domain.Name}}Service>, I{{domain.Name}}Controller
{{end}}
{
}
";

    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ExcuteAsync(GeneratorCodeContext context)
    {
        foreach (DomainModel domain in context.Domains)
        {
            await GeneratorIControllerCodeAsync(context, domain);
            await GeneratorControllersCodeAsync(context, domain);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <summary>
    /// 创建控制器代码接口
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorIControllerCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotControllerAttribute>()) return;

        Template template = Template.Parse(_iControllerTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Controllers", $"I{domain.Name}Controller.cs");
    }

    /// <summary>
    /// 创建控制器代码实现
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorControllersCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotControllerAttribute>()) return;

        Template template = Template.Parse(_controllerTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Controllers", $"{domain.Name}Controller.cs");
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    /// <param name="template"></param>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    private static string RenderTemplate(Template template, GeneratorCodeContext context, DomainModel domain)
    {
        var controllerViewModel = new ControllerViewModel
        {
            Name = domain.Name,
            Annotation = domain.Annotation,
            IsEmptyController = domain.HasAttribute<EmptyServiceAttribute, EmptyControllerAttribute>(),
            HasNotServiceAttribute = domain.HasAttribute<NotServiceAttribute>()
        };

        ScriptObject scriptObject = new()
        {
            { "context", context },
            { "domain", controllerViewModel }
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
