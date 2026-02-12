using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 位序(Index)相关代码生成插件
/// </summary>
public class IndexGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    private class IndexServiceViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Annotation { get; set; }
        public bool IsTreeDomain { get; set; }
        public string? IndexGroupPropertyName { get; set; }
        public string? TreeGroupPropertyName { get; set; }
    }

    private class IndexRepositoryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Annotation { get; set; }
        public IndexGroupPropertyViewModel? IndexGroupProperty { get; set; }
    }

    private class IndexGroupPropertyViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string PredefinedType { get; set; } = string.Empty;
        public string NameToLowerFirstLetter => Name.Length > 0 ? char.ToLower(Name[0]) + Name[1..] : Name;
    }

    private static readonly string _iServiceIndexTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services
{
    /// <summary>
    /// {{domain.Annotation}}服务(Index)
    /// </summary>
    public partial interface I{{domain.Name}}Service
    {
        /// <summary>
        /// 交换位序
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task ExchangeIndexAsync(ExchangeIndexModel model);
    }
}
";

    private static readonly string _iControllerIndexTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Controllers;

/// <summary>
/// {{domain.Annotation}}控制器(Index)
/// </summary>
public partial interface I{{domain.Name}}Controller
{
    /// <summary>
    /// 交换位序
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>
    [HttpPut]
    Task<ResultModel> ExchangeIndexAsync(ExchangeIndexRequestModel requestModel);
}
";

    private static readonly string _controllerIndexTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Application.Controllers;

/// <summary>
/// {{domain.Annotation}}控制器(Index)
/// </summary>
public partial class {{domain.Name}}Controller
{
    /// <summary>
    /// 交换位序
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultModel> ExchangeIndexAsync(ExchangeIndexRequestModel requestModel)
    {
        ExchangeIndexModel model = Mapper.Map<ExchangeIndexModel>(requestModel) ?? throw new {{context.ProjectName}}Exception(""映射失败"");
        await DefaultService.ExchangeIndexAsync(model);
        return ResultModel.Success(""交换位序成功"");
    }
}
";

    private static readonly string _serviceImplIndexTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Application.Services
{
    /// <summary>
    /// {{domain.Annotation}}服务(Index)
    /// </summary>
    public partial class {{domain.Name}}ServiceImpl
    {
        /// <summary>
        /// 交换位序
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task ExchangeIndexAsync(ExchangeIndexModel model)
        {
            OnExchangeIndexBefore(model);
{{- if domain.IsTreeDomain }}
{{- if domain.IndexGroupPropertyName == null || domain.IndexGroupPropertyName == """" }}
{{- if domain.TreeGroupPropertyName == null || domain.TreeGroupPropertyName == """" }}
            await ServiceImplHelper.ExchangeParentAndIndexAsync<I{{domain.Name}}Repository, {{domain.Name}}>(model, DefaultRepository, UnitOfWork, [], []);
{{- else }}
            await ServiceImplHelper.ExchangeParentAndIndexAsync<I{{domain.Name}}Repository, {{domain.Name}}>(model, DefaultRepository, UnitOfWork, [], [nameof({{domain.Name}}.{{domain.TreeGroupPropertyName}})]);
{{- end }}
{{- else }}
{{- if domain.TreeGroupPropertyName == null || domain.TreeGroupPropertyName == """" }}
            await ServiceImplHelper.ExchangeParentAndIndexAsync<I{{domain.Name}}Repository, {{domain.Name}}>(model, DefaultRepository, UnitOfWork, [nameof({{domain.Name}}.{{domain.IndexGroupPropertyName}})], []);
{{- else }}
            await ServiceImplHelper.ExchangeParentAndIndexAsync<I{{domain.Name}}Repository, {{domain.Name}}>(model, DefaultRepository, UnitOfWork, [nameof({{domain.Name}}.{{domain.IndexGroupPropertyName}})], [nameof({{domain.Name}}.{{domain.TreeGroupPropertyName}})]);
{{- end }}
{{- end }}
{{- else }}
{{- if domain.IndexGroupPropertyName == null || domain.IndexGroupPropertyName == """" }}
            await ServiceImplHelper.ExchangeIndexAsync<I{{domain.Name}}Repository, {{domain.Name}}>(model, DefaultRepository, UnitOfWork);
{{- else }}
            await ServiceImplHelper.ExchangeIndexAsync<I{{domain.Name}}Repository, {{domain.Name}}>(model, DefaultRepository, UnitOfWork, [nameof({{domain.Name}}.{{domain.IndexGroupPropertyName}})]);
{{- end }}
{{- end }}
            OnExchangeIndexAfter(model);
        }
        /// <summary>
        /// 交换位序之前
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        partial void OnExchangeIndexBefore(ExchangeIndexModel model);
        /// <summary>
        /// 交换位序之后
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        partial void OnExchangeIndexAfter(ExchangeIndexModel model);
    }
}
";

    private static readonly string _iRepositoryIndexTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Repositories;
/// <summary>
/// {{domain.Annotation}}仓储(Index)
/// </summary>
public partial interface I{{domain.Name}}Repository
{
{{- if domain.IndexGroupProperty == null}}
    /// <summary>
    /// 获取最大位序
    /// </summary>
    /// <returns></returns>
    Task<int> GetMaxIndexAsync();
{{- else}}
    /// <summary>
    /// 获取最大位序
    /// </summary>
    /// <param name=""{{domain.IndexGroupProperty.NameToLowerFirstLetter}}""></param>
    /// <returns></returns>
    Task<int> GetMaxIndexAsync({{domain.IndexGroupProperty.PredefinedType}} {{domain.IndexGroupProperty.NameToLowerFirstLetter}});
{{- end}}
}
";

    private static readonly string _repositoryImplIndexTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Repository.Repositories;
/// <summary>
/// {{domain.Annotation}}仓储(Index)
/// </summary>
public partial class {{domain.Name}}RepositoryImpl
{
    /// <summary>
    /// 获取最大位序
    /// </summary>
{{- if domain.IndexGroupProperty == null}}
    /// <returns></returns>
    public async Task<int> GetMaxIndexAsync()
    {
        if (!await DBSet.AnyAsync()) return -1;
        int result = await DBSet.MaxAsync(m => m.Index);
        return result;
    }
{{- else}}
    /// <param name=""{{domain.IndexGroupProperty.NameToLowerFirstLetter}}""></param>
    /// <returns></returns>
    public async Task<int> GetMaxIndexAsync({{domain.IndexGroupProperty.PredefinedType}} {{domain.IndexGroupProperty.NameToLowerFirstLetter}})
    {
        if (!await DBSet.AnyAsync(m => m.{{domain.IndexGroupProperty.Name}} == {{domain.IndexGroupProperty.NameToLowerFirstLetter}})) return -1;
        int result = await DBSet.Where(m => m.{{domain.IndexGroupProperty.Name}} == {{domain.IndexGroupProperty.NameToLowerFirstLetter}}).MaxAsync(m => m.Index);
        return result;
    }
{{- end}}
}
";

    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ExcuteAsync(GeneratorCodeContext context)
    {
        foreach (DomainModel domain in context.Domains)
        {
            await GeneratorIControllerIndexCodeAsync(context, domain);
            await GeneratorControllerIndexCodeAsync(context, domain);
            await GeneratorIServicesIndexCodeAsync(context, domain);
            await GeneratorServiceImplsIndexCodeAsync(context, domain);
            await GeneratorIRepositoryIndexCodeAsync(context, domain);
            await GeneratorRepositoryImplIndexCodeAsync(context, domain);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    private Task GeneratorIServicesIndexCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;
        if (!domain.IsIndexDomain || domain.HasAttribute<NotIndexServiceAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_iServiceIndexTemplate);
        IndexServiceViewModel viewModel = GetIndexServiceViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Services", $"I{domain.Name}Service.Index.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorServiceImplsIndexCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;
        if (!domain.IsIndexDomain || domain.HasAttribute<NotIndexServiceAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_serviceImplIndexTemplate);
        IndexServiceViewModel viewModel = GetIndexServiceViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Services", $"{domain.Name}ServiceImpl.Index.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorIControllerIndexCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotControllerAttribute>()) return Task.CompletedTask;
        if (!domain.IsIndexDomain) return Task.CompletedTask;
        if (domain.HasAttribute<NotIndexControllerAttribute>() || domain.HasAttribute<NotIndexServiceAttribute>()) return Task.CompletedTask;
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_iControllerIndexTemplate);
        IndexServiceViewModel viewModel = GetIndexServiceViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Controllers", $"I{domain.Name}Controller.Index.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorControllerIndexCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotControllerAttribute>()) return Task.CompletedTask;
        if (!domain.IsIndexDomain) return Task.CompletedTask;
        if (domain.HasAttribute<NotIndexControllerAttribute>() || domain.HasAttribute<NotIndexServiceAttribute>()) return Task.CompletedTask;
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_controllerIndexTemplate);
        IndexServiceViewModel viewModel = GetIndexServiceViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Controllers", $"{domain.Name}Controller.Index.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorIRepositoryIndexCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotRepositoryAttribute>()) return Task.CompletedTask;
        if (!domain.IsIndexDomain || domain.HasAttribute<NotIndexRepositoryAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_iRepositoryIndexTemplate);
        IndexRepositoryViewModel viewModel = GetIndexRepositoryViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Repositories", $"I{domain.Name}Repository.Index.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorRepositoryImplIndexCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotRepositoryAttribute>()) return Task.CompletedTask;
        if (!domain.IsIndexDomain || domain.HasAttribute<NotIndexRepositoryAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_repositoryImplIndexTemplate);
        IndexRepositoryViewModel viewModel = GetIndexRepositoryViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleRepositoryMGCPath, "Repositories", $"{domain.Name}RepositoryImpl.Index.cs");
        return Task.CompletedTask;
    }

    private static IndexServiceViewModel GetIndexServiceViewModel(DomainModel domain)
    {
        PropertyModel? indexGroupProperty = domain.GetIndexGroupProperty();
        PropertyModel? treeGroupProperty = domain.GetTreeGroupProperty();
        return new IndexServiceViewModel
        {
            Name = domain.Name,
            Annotation = domain.Annotation,
            IsTreeDomain = domain.IsTreeDomain,
            IndexGroupPropertyName = indexGroupProperty?.Name,
            TreeGroupPropertyName = treeGroupProperty?.Name
        };
    }

    private static IndexRepositoryViewModel GetIndexRepositoryViewModel(DomainModel domain)
    {
        PropertyModel? indexGroupProperty = domain.GetIndexGroupProperty();
        return new IndexRepositoryViewModel
        {
            Name = domain.Name,
            Annotation = domain.Annotation,
            IndexGroupProperty = indexGroupProperty is null
                ? null
                : new IndexGroupPropertyViewModel
                {
                    Name = indexGroupProperty.Name,
                    PredefinedType = indexGroupProperty.PredefinedType
                }
        };
    }

    private static string RenderTemplate(Template template, GeneratorCodeContext context, object domain)
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
