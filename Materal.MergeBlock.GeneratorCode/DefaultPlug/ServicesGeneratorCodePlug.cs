using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 服务代码生成插件
/// </summary>
public class ServicesGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <summary>
    /// 服务视图模型（用于模板渲染）
    /// </summary>
    private class ServiceViewModel
    {
        /// <summary>
        /// 领域名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 注释
        /// </summary>
        public string? Annotation { get; set; }

        /// <summary>
        /// 是否空服务（仅基础接口）
        /// </summary>
        public bool IsEmptyService { get; set; }

        /// <summary>
        /// 服务实现基类声明
        /// </summary>
        public string ServiceImplBaseType { get; set; } = string.Empty;
    }

    private static readonly string _iServiceTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services
{
    /// <summary>
    /// {{domain.Annotation}}服务
    /// </summary>
{{- if domain.IsEmptyService }}
    public partial interface I{{domain.Name}}Service : IBaseService
{{- else }}
    public partial interface I{{domain.Name}}Service : IBaseService<Add{{domain.Name}}Model, Edit{{domain.Name}}Model, Query{{domain.Name}}Model, {{domain.Name}}DTO, {{domain.Name}}ListDTO>
{{- end }}
    {
    }
}
";

    private static readonly string _serviceImplTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Application.Services
{
    /// <summary>
    /// {{domain.Annotation}}服务
    /// </summary>
    public partial class {{domain.Name}}ServiceImpl : {{domain.ServiceImplBaseType}}, I{{domain.Name}}Service, IScopedDependency<I{{domain.Name}}Service>
    {
    }
}
";

    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ExcuteAsync(GeneratorCodeContext context)
    {
        foreach (DomainModel domain in context.Domains)
        {
            await GeneratorIServicesCodeAsync(context, domain);
            await GeneratorServiceImplsCodeAsync(context, domain, context.Domains);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <summary>
    /// 创建服务代码接口
    /// </summary>
    private Task GeneratorIServicesCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_iServiceTemplate);
        ServiceViewModel serviceViewModel = GetServiceViewModel(context, domain, domain);

        string codeContent = RenderTemplate(template, context, serviceViewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Services", $"I{domain.Name}Service.cs");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建服务代码实现
    /// </summary>
    private Task GeneratorServiceImplsCodeAsync(GeneratorCodeContext context, DomainModel domain, List<DomainModel> domains)
    {
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;

        DomainModel targetDomain = domain.GetQueryDomain(domains);

        Template template = Template.Parse(_serviceImplTemplate);
        ServiceViewModel serviceViewModel = GetServiceViewModel(context, domain, targetDomain);

        string codeContent = RenderTemplate(template, context, serviceViewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Services", $"{domain.Name}ServiceImpl.cs");
        return Task.CompletedTask;
    }

    private static ServiceViewModel GetServiceViewModel(GeneratorCodeContext context, DomainModel domain, DomainModel targetDomain)
    {
        return new ServiceViewModel
        {
            Name = domain.Name,
            Annotation = domain.Annotation,
            IsEmptyService = domain.HasAttribute<EmptyServiceAttribute>(),
            ServiceImplBaseType = GetServiceImplBaseType(context, domain, targetDomain)
        };
    }

    private static string GetServiceImplBaseType(GeneratorCodeContext context, DomainModel domain, DomainModel targetDomain)
    {
        bool isEmptyService = domain.HasAttribute<EmptyServiceAttribute>();
        bool hasNotRepositoryAttribute = domain.HasAttribute<NotRepositoryAttribute>();
        bool isTargetDomainDifferent = targetDomain != domain;

        if (isEmptyService)
        {
            if (hasNotRepositoryAttribute)
            {
                return $"BaseServiceImpl<I{context.ModuleName}UnitOfWork>";
            }
            if (isTargetDomainDifferent)
            {
                return $"BaseServiceImpl<I{domain.Name}Repository, I{targetDomain.Name}Repository, {domain.Name}, {targetDomain.Name}, I{context.ModuleName}UnitOfWork>";
            }
            return $"BaseServiceImpl<I{domain.Name}Repository, {domain.Name}, I{context.ModuleName}UnitOfWork>";
        }

        if (isTargetDomainDifferent)
        {
            return $"BaseServiceImpl<Add{domain.Name}Model, Edit{domain.Name}Model, Query{domain.Name}Model, {domain.Name}DTO, {domain.Name}ListDTO, I{domain.Name}Repository, I{targetDomain.Name}Repository, {domain.Name}, {targetDomain.Name}, I{context.ModuleName}UnitOfWork>";
        }
        return $"BaseServiceImpl<Add{domain.Name}Model, Edit{domain.Name}Model, Query{domain.Name}Model, {domain.Name}DTO, {domain.Name}ListDTO, I{domain.Name}Repository, {domain.Name}, I{context.ModuleName}UnitOfWork>";
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    private static string RenderTemplate(Template template, GeneratorCodeContext context, ServiceViewModel domain)
    {
        ScriptObject scriptObject = new()
        {
            { "context", context },
            { "domain", domain }
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
