using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 位序生成代码插件
/// </summary>
public class IndexGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    private class IndexViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Annotation { get; set; }
        public bool IsTreeDomain { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public List<IndexGroupPropertyViewModel> IndexGroupProperties { get; set; } = [];
    }

    private class IndexGroupPropertyViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string PredefinedType { get; set; } = string.Empty;
        public string NameToLowerFirstLetter => Name.Length > 0 ? char.ToLower(Name[0]) + Name[1..] : Name;
    }


    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ExcuteAsync(GeneratorCodeContext context)
    {
        foreach (DomainModel domain in context.Domains)
        {
            await GeneratorIRepositoryCodeAsync(context, domain);
            await GeneratorRepositoryImplCodeAsync(context, domain);
            await GeneratorIServiceCodeAsync(context, domain);
            await GeneratorServiceImplCodeAsync(context, domain);
            await GeneratorIControllerCodeAsync(context, domain);
            await GeneratorControllerImplCodeAsync(context, domain);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <summary>
    /// 渲染模板
    /// </summary>
    /// <param name="template"></param>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    private static string RenderTemplate(Template template, GeneratorCodeContext context, DomainModel domain)
    {
        IndexViewModel repositoryViewModel = new()
        {
            Name = domain.Name,
            Annotation = domain.Annotation,
            IsTreeDomain = domain.IsTreeDomain,
            ProjectName = context.ProjectName,
            ModuleName = context.ModuleName
        };
        foreach (PropertyModel item in domain.GetIndexGroupProperties())
        {
            IndexGroupPropertyViewModel indexGroupProperty = new()
            {
                Name = item.Name,
                PredefinedType = item.PredefinedType
            };
            repositoryViewModel.IndexGroupProperties.Add(indexGroupProperty);
        }

        ScriptObject scriptObject = new()
        {
            { "context", context },
            { "domain", repositoryViewModel }
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

    #region IRepository
    private static readonly string _iRepositoryIndexTemplate = @"namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Repositories;
/// <summary>
/// {{ domain.Annotation }}仓储(Index)
/// </summary>
public partial interface I{{ domain.Name }}Repository
{
    /// <summary>
    /// 获取最大位序
    /// </summary>
    {{- for param in domain.IndexGroupProperties }}
    /// <param name=""{{ param.NameToLowerFirstLetter }}""></param>
    {{- end }}
    /// <returns></returns>
    Task<int> GetMaxIndexAsync({{- for param in domain.IndexGroupProperties }}{{ param.PredefinedType }} {{ param.NameToLowerFirstLetter }}{{ if !for.last }}, {{ end }}{{ end }});
}";

    private async Task GeneratorIRepositoryCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsIndexDomain || domain.HasAttribute<NotIndexRepositoryAttribute>()) return;

        Template template = Template.Parse(_iRepositoryIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Repositories", $"I{domain.Name}Repository.Index.cs");
    }
    #endregion

    #region RepositoryImpl
    private static readonly string _repositoryImplIndexTemplate = @"namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Repository.Repositories;
/// <summary>
/// {{ domain.Annotation }}仓储(Index)
/// </summary>
public partial class {{ domain.Name }}RepositoryImpl
{
    /// <summary>
    /// 获取最大位序
    /// </summary>
    {{- for param in domain.IndexGroupProperties }}
    /// <param name=""{{ param.NameToLowerFirstLetter }}""></param>
    {{- end }}
    /// <returns></returns>
    public async Task<int> GetMaxIndexAsync({{- for param in domain.IndexGroupProperties }}{{ param.PredefinedType }} {{ param.NameToLowerFirstLetter }}{{ if !for.last }}, {{ end }}{{ end }})
    {
        {{- if domain.IndexGroupProperties.size > 0 }}
        if (!await DBSet.AnyAsync(m => {{- for param in domain.IndexGroupProperties }} m.{{ param.Name }} == {{ param.NameToLowerFirstLetter }}{{ if !for.last }} && {{ end }}{{ end }})) return -1;
        int result = await DBSet.Where(m => {{- for param in domain.IndexGroupProperties }} m.{{ param.Name }} == {{ param.NameToLowerFirstLetter }}{{ if !for.last }} && {{ end }}{{ end }}).MaxAsync(m => m.Index);
        {{- else }}
        if (!await DBSet.AnyAsync()) return -1;
        int result = await DBSet.MaxAsync(m => m.Index);
        {{- end }}
        return result;
    }
}";

    private async Task GeneratorRepositoryImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsIndexDomain || domain.HasAttribute<NotIndexRepositoryAttribute>()) return;

        Template template = Template.Parse(_repositoryImplIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleRepositoryMGCPath, "Repositories", $"{domain.Name}RepositoryImpl.Index.cs");
    }
    #endregion

    #region IService
    private static readonly string _iServiceIndexTemplate = @"using Materal.MergeBlock.Abstractions.Models;

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Services;

/// <summary>
/// {{ domain.Annotation }}服务(Index)
/// </summary>
public partial interface I{{ domain.Name }}Service
{
    /// <summary>
    /// 移动位序
    /// </summary>
    /// <param name=""model""></param>
    /// <returns></returns>
    Task MoveIndexAsync(MoveIndexModel model);
}";

    private async Task GeneratorIServiceCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsIndexDomain || domain.IsTreeDomain || domain.HasAttribute<NotIndexServiceAttribute>()) return;

        Template template = Template.Parse(_iServiceIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Services", $"I{domain.Name}Service.Index.cs");
    }
    #endregion

    #region ServiceImpl
    private static readonly string _serviceImplIndexTemplate = @"using Materal.MergeBlock.Abstractions.Models;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Domain;

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Application.Services;

/// <summary>
/// {{ domain.Annotation }}服务(Index)
/// </summary>
public partial class {{ domain.Name }}ServiceImpl
{
    /// <summary>
    /// 移动位序
    /// </summary>
    /// <param name=""model""></param>
    /// <returns></returns>
    public async Task MoveIndexAsync(MoveIndexModel model)
    {
        Dictionary<string, object> contextData = [];
        OnMoveIndexBefore(model, contextData);
        {{- if domain.IndexGroupProperties.size > 0 }}
        List<{{ domain.Name }}> domains = await ServiceImplHelper.MoveAsync<I{{ domain.Name }}Repository, {{ domain.Name }}>(model, DefaultRepository, [{{- for param in domain.IndexGroupProperties }}nameof({{ domain.Name }}.{{ param.Name }}){{ if !for.last }}, {{ end }}{{ end }}]);
        {{- else }}
        List<{{ domain.Name }}> domains = await ServiceImplHelper.MoveAsync<I{{ domain.Name }}Repository, {{ domain.Name }}>(model, DefaultRepository, []);
        {{- end }}
        OnMoveIndexAfter(model, contextData, domains);
        foreach ({{ domain.Name }} domain in domains)
        {
            UnitOfWork.RegisterEdit(domain);
        }
        await UnitOfWork.CommitAsync();
    }

    /// <summary>
    /// 移动位序之前
    /// </summary>
    /// <param name=""model""></param>
    /// <param name=""contextData""></param>
    /// <returns></returns>
    partial void OnMoveIndexBefore(MoveIndexModel model, Dictionary<string, object> contextData);

    /// <summary>
    /// 移动位序之后
    /// </summary>
    /// <param name=""model""></param>
    /// <param name=""contextData""></param>
    /// <param name=""domains""></param>
    /// <returns></returns>
    partial void OnMoveIndexAfter(MoveIndexModel model, Dictionary<string, object> contextData, List<{{ domain.Name }}> domains);
}";

    private async Task GeneratorServiceImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsIndexDomain || domain.IsTreeDomain || domain.HasAttribute<NotIndexServiceAttribute>()) return;

        Template template = Template.Parse(_serviceImplIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Services", $"{domain.Name}ServiceImpl.Index.cs");
    }
    #endregion

    #region IController
    private static readonly string _iControllerIndexTemplate = @"using Materal.MergeBlock.Abstractions.Models;

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Controllers;

/// <summary>
/// {{ domain.Annotation }}控制器(Index)
/// </summary>
public partial interface I{{ domain.Name }}Controller
{
    /// <summary>
    /// 移动位序
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>
    [HttpPut]
    Task<ResultModel> MoveIndexAsync(MoveIndexRequestModel requestModel);
}";

    private async Task GeneratorIControllerCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsIndexDomain || domain.IsTreeDomain || domain.HasAttribute<NotIndexControllerAttribute>()) return;

        Template template = Template.Parse(_iControllerIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Controllers", $"I{domain.Name}Controller.Index.cs");
    }
    #endregion

    #region ControllerImpl
    private static readonly string _controllerImplIndexTemplate = @"using Materal.MergeBlock.Abstractions.Models;

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Application.Controllers;

/// <summary>
/// {{ domain.Annotation }}控制器(Index)
/// </summary>
public partial class {{ domain.Name }}Controller
{
    /// <summary>
    /// 移动位序
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>    
    [HttpPut]
    public async Task<ResultModel> MoveIndexAsync(MoveIndexRequestModel requestModel)
    {
        MoveIndexModel model = Mapper.Map<MoveIndexModel>(requestModel) ?? throw new {{ domain.ProjectName }}Exception(""映射失败"");
        await DefaultService.MoveIndexAsync(model);
        return ResultModel.Success(""移动位序成功"");
    }
}";

    private async Task GeneratorControllerImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsIndexDomain || domain.IsTreeDomain || domain.HasAttribute<NotIndexControllerAttribute>()) return;

        Template template = Template.Parse(_controllerImplIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Controllers", $"{domain.Name}Controller.Index.cs");
    }
    #endregion
}
