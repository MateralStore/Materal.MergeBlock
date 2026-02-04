using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 树相关代码生成插件
/// </summary>
public class TreeGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    private class TreeServiceViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Annotation { get; set; }
        public string? TreeGroupPropertyName { get; set; }
    }

    private class TreeQueryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Annotation { get; set; }
        public TreeGroupPropertyViewModel? TreeGroupProperty { get; set; }
    }

    private class TreeGroupPropertyViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Annotation { get; set; }
        public string NullPredefinedType { get; set; } = string.Empty;
    }

    private static readonly string _iServiceTreeTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services
{
    /// <summary>
    /// {{domain.Annotation}}服务(Tree)
    /// </summary>
    public partial interface I{{domain.Name}}Service
    {
        /// <summary>
        /// 更改父级
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task ExchangeParentAsync(ExchangeParentModel model);
        /// <summary>
        /// 查询树列表
        /// </summary>
        /// <param name=""queryModel""></param>
        Task<List<{{domain.Name}}TreeListDTO>> GetTreeListAsync(Query{{domain.Name}}TreeListModel queryModel);
    }
}
";

    private static readonly string _iControllerTreeTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Controllers;

/// <summary>
/// {{domain.Annotation}}控制器(Tree)
/// </summary>
public partial interface I{{domain.Name}}Controller
{
    /// <summary>
    /// 更改父级
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>
    [HttpPut]
    Task<ResultModel> ExchangeParentAsync(ExchangeParentRequestModel requestModel);
    /// <summary>
    /// 查询树列表
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>
    [HttpPost]
    Task<ResultModel<List<{{domain.Name}}TreeListDTO>>> GetTreeListAsync(Query{{domain.Name}}TreeListRequestModel requestModel);
}
";

    private static readonly string _controllerTreeTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Application.Controllers;

/// <summary>
/// {{domain.Annotation}}控制器(Tree)
/// </summary>
public partial class {{domain.Name}}Controller
{
    /// <summary>
    /// 更改父级
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ResultModel> ExchangeParentAsync(ExchangeParentRequestModel requestModel)
    {
        ExchangeParentModel model = Mapper.Map<ExchangeParentModel>(requestModel) ?? throw new {{context.ProjectName}}Exception(""映射失败"");
        await DefaultService.ExchangeParentAsync(model);
        return ResultModel.Success(""更改父级成功"");
    }
    /// <summary>
    /// 查询树列表
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ResultModel<List<{{domain.Name}}TreeListDTO>>> GetTreeListAsync(Query{{domain.Name}}TreeListRequestModel requestModel)
    {
        Query{{domain.Name}}TreeListModel model = Mapper.Map<Query{{domain.Name}}TreeListModel>(requestModel) ?? throw new {{context.ProjectName}}Exception(""映射失败"");
        List<{{domain.Name}}TreeListDTO> result = await DefaultService.GetTreeListAsync(model);
        return ResultModel<List<{{domain.Name}}TreeListDTO>>.Success(result, ""查询成功"");
    }
}
";

    private static readonly string _treeListDtoTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};

/// <summary>
/// {{domain.Annotation}}树列表数据传输模型
/// </summary>
public partial class {{domain.Name}}TreeListDTO : {{domain.Name}}ListDTO, ITreeDTO<{{domain.Name}}TreeListDTO>
{
    /// <summary>
    /// 子级
    /// </summary>
    public List<{{domain.Name}}TreeListDTO> Children { get; set; } = [];
}
";

    private static readonly string _treeQueryRequestModelTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};
/// <summary>
/// {{domain.Annotation}}树查询请求模型
/// </summary>
public partial class Query{{domain.Name}}TreeListRequestModel : FilterModel
{
    /// <summary>
    /// 父级唯一标识
    /// </summary>
    public Guid? ParentID { get; set; }
{{- if domain.TreeGroupProperty}}
    /// <summary>
    /// {{domain.TreeGroupProperty.Annotation}}
    /// </summary>
    [Equal]
    public {{domain.TreeGroupProperty.NullPredefinedType}} {{domain.TreeGroupProperty.Name}} { get; set; }
{{- end}}
}
";

    private static readonly string _treeQueryModelTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};
/// <summary>
/// {{domain.Annotation}}树查询模型
/// </summary>
public partial class Query{{domain.Name}}TreeListModel : FilterModel
{
    /// <summary>
    /// 父级唯一标识
    /// </summary>
    public Guid? ParentID { get; set; }
{{- if domain.TreeGroupProperty}}
    /// <summary>
    /// {{domain.TreeGroupProperty.Annotation}}
    /// </summary>
    [Equal]
    public {{domain.TreeGroupProperty.NullPredefinedType}} {{domain.TreeGroupProperty.Name}} { get; set; }
{{- end}}
}
";

    private static readonly string _serviceImplTreeTemplate = @"
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Services.Models.{{domain.Name}};

namespace {{context.ProjectName}}.{{context.ModuleName}}.Application.Services
{
    /// <summary>
    /// {{domain.Annotation}}服务(Tree)
    /// </summary>
    public partial class {{domain.Name}}ServiceImpl
    {
        /// <summary>
        /// 更改父级
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task ExchangeParentAsync(ExchangeParentModel model)
        {
            OnExchangeParentBefore(model);
{{- if domain.TreeGroupPropertyName == null || domain.TreeGroupPropertyName == """" }}
            await ServiceImplHelper.ExchangeParentByGroupPropertiesAsync<I{{domain.Name}}Repository, {{domain.Name}}>(model, DefaultRepository, UnitOfWork);
{{- else }}
            await ServiceImplHelper.ExchangeParentByGroupPropertiesAsync<I{{domain.Name}}Repository, {{domain.Name}}>(model, DefaultRepository, UnitOfWork, nameof({{domain.Name}}.{{domain.TreeGroupPropertyName}}));
{{- end }}
            OnExchangeParentAfter(model);
        }
        /// <summary>
        /// 更改父级之前
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        partial void OnExchangeParentBefore(ExchangeParentModel model);
        /// <summary>
        /// 更改父级之后
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        partial void OnExchangeParentAfter(ExchangeParentModel model);
        /// <summary>
        /// 查询树列表
        /// </summary>
        /// <param name=""queryModel""></param>
        public async Task<List<{{domain.Name}}TreeListDTO>> GetTreeListAsync(Query{{domain.Name}}TreeListModel queryModel)
        {
            #region 排序表达式
            Type domainType = typeof({{domain.Name}});
            Expression<Func<{{domain.Name}}, object>> sortExpression = m => m.CreateTime;
            SortOrder sortOrder = SortOrder.Descending;
            if (queryModel.SortPropertyName is not null && !string.IsNullOrWhiteSpace(queryModel.SortPropertyName) && domainType.GetProperty(queryModel.SortPropertyName) is not null)
            {
                sortExpression = queryModel.GetSortExpression<{{domain.Name}}>() ?? sortExpression;
                sortOrder = queryModel.IsAsc ? SortOrder.Ascending : SortOrder.Descending;
            }
            else if (domainType.IsAssignableTo<IIndexDomain>())
            {
                ParameterExpression parameterExpression = Expression.Parameter(domainType, ""m"");
                MemberExpression memberExpression = Expression.Property(parameterExpression, nameof(IIndexDomain.Index));
                UnaryExpression unaryExpression = Expression.Convert(memberExpression, typeof(object));
                sortExpression = Expression.Lambda<Func<{{domain.Name}}, object>>(unaryExpression, parameterExpression);
                sortOrder = SortOrder.Ascending;
            }
            #endregion
            #region 查询数据源
            List<{{domain.Name}}> allInfo;
            if (DefaultRepository is ICacheEFRepository<{{domain.Name}}, Guid> cacheRepository)
            {
                allInfo = await cacheRepository.GetAllInfoFromCacheAsync();
                Func<{{domain.Name}}, bool> searchDlegate = queryModel.GetSearchDelegate<{{domain.Name}}>();
                Func<{{domain.Name}}, object> sortDlegate = sortExpression.Compile();
                if (sortOrder == SortOrder.Ascending)
                {
                    allInfo = [.. allInfo.Where(searchDlegate).OrderBy(sortDlegate)];
                }
                else
                {
                    allInfo = [.. allInfo.Where(searchDlegate).OrderByDescending(sortDlegate)];
                }
            }
            else
            {
                allInfo = await DefaultRepository.FindAsync(queryModel, sortExpression, sortOrder);
            }
            #endregion
            Dictionary<string, object> contextData = [];
            OnToTreeBefore(allInfo, queryModel, contextData);
            List<{{domain.Name}}TreeListDTO> result = allInfo.ToTree<{{domain.Name}}, {{domain.Name}}TreeListDTO>(queryModel.ParentID, (dto, domain) =>
            {
                Mapper.Map(domain, dto);
                OnConvertToTreeDTO(dto, domain, queryModel, contextData);
            });
            OnToTreeAfter(result, queryModel, contextData);
            return result;
        }
        /// <summary>
        /// 转换树之前
        /// </summary>
        /// <param name=""allInfo""></param>
        /// <param name=""queryModel""></param>
        /// <param name=""contextData""></param>
        partial void OnToTreeBefore(List<{{domain.Name}}> allInfo, Query{{domain.Name}}TreeListModel queryModel, Dictionary<string, object> contextData);
        /// <summary>
        /// 转换树之后
        /// </summary>
        /// <param name=""dtos""></param>
        /// <param name=""queryModel""></param>
        /// <param name=""contextData""></param>
        partial void OnToTreeAfter(List<{{domain.Name}}TreeListDTO> dtos, Query{{domain.Name}}TreeListModel queryModel, Dictionary<string, object> contextData);
        /// <summary>
        /// 转换为树DTO
        /// </summary>
        /// <param name=""dto""></param>
        /// <param name=""domain""></param>
        /// <param name=""queryModel""></param>
        /// <param name=""contextData""></param>
        partial void OnConvertToTreeDTO({{domain.Name}}TreeListDTO dto, {{domain.Name}} domain, Query{{domain.Name}}TreeListModel queryModel, Dictionary<string, object> contextData);
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
            await GeneratorIControllerTreeCodeAsync(context, domain);
            await GeneratorControllerTreeCodeAsync(context, domain);
            await GeneratorIServicesTreeCodeAsync(context, domain);
            await GeneratorServiceImplsTreeCodeAsync(context, domain);
            await GeneratorTreeListDTOModelAsync(context, domain);
            await GeneratorTreeQueryRequestModelAsync(context, domain);
            await GeneratorTreeQueryModelAsync(context, domain);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    private Task GeneratorIServicesTreeCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;
        if (!domain.IsTreeDomain || domain.HasAttribute<NotTreeServiceAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_iServiceTreeTemplate);
        TreeServiceViewModel serviceViewModel = GetTreeServiceViewModel(domain);

        string codeContent = RenderTemplate(template, context, serviceViewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Services", $"I{domain.Name}Service.Tree.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorServiceImplsTreeCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;
        if (!domain.IsTreeDomain || domain.HasAttribute<NotTreeServiceAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_serviceImplTreeTemplate);
        TreeServiceViewModel serviceViewModel = GetTreeServiceViewModel(domain);

        string codeContent = RenderTemplate(template, context, serviceViewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Services", $"{domain.Name}ServiceImpl.Tree.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorIControllerTreeCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotControllerAttribute>()) return Task.CompletedTask;
        if (!domain.IsTreeDomain) return Task.CompletedTask;
        if (domain.HasAttribute<NotTreeControllerAttribute>() || domain.HasAttribute<NotTreeServiceAttribute>()) return Task.CompletedTask;
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_iControllerTreeTemplate);
        TreeServiceViewModel viewModel = GetTreeServiceViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Controllers", $"I{domain.Name}Controller.Tree.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorControllerTreeCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotControllerAttribute>()) return Task.CompletedTask;
        if (!domain.IsTreeDomain) return Task.CompletedTask;
        if (domain.HasAttribute<NotTreeControllerAttribute>() || domain.HasAttribute<NotTreeServiceAttribute>()) return Task.CompletedTask;
        if (domain.HasAttribute<NotServiceAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_controllerTreeTemplate);
        TreeServiceViewModel viewModel = GetTreeServiceViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Controllers", $"{domain.Name}Controller.Tree.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorTreeListDTOModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.HasAttribute<NotTreeDTOAttribute>() || domain.HasAttribute<NotListDTOAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_treeListDtoTemplate);
        TreeServiceViewModel viewModel = GetTreeServiceViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "DTO", domain.Name, $"{domain.Name}TreeListDTO.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorTreeQueryRequestModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.HasAttribute<NotTreeQueryAttribute>() || domain.HasAttribute<NotQueryAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_treeQueryRequestModelTemplate);
        TreeQueryViewModel viewModel = GetTreeQueryViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "RequestModel", domain.Name, $"Query{domain.Name}TreeListRequestModel.cs");
        return Task.CompletedTask;
    }

    private Task GeneratorTreeQueryModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.HasAttribute<NotTreeQueryAttribute>() || domain.HasAttribute<NotQueryAttribute>()) return Task.CompletedTask;

        Template template = Template.Parse(_treeQueryModelTemplate);
        TreeQueryViewModel viewModel = GetTreeQueryViewModel(domain);

        string codeContent = RenderTemplate(template, context, viewModel);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Services", "Models", domain.Name, $"Query{domain.Name}TreeListModel.cs");
        return Task.CompletedTask;
    }

    private static TreeServiceViewModel GetTreeServiceViewModel(DomainModel domain)
    {
        PropertyModel? treeGroupProperty = domain.GetTreeGroupProperty();
        return new TreeServiceViewModel
        {
            Name = domain.Name,
            Annotation = domain.Annotation,
            TreeGroupPropertyName = treeGroupProperty?.Name
        };
    }

    private static TreeQueryViewModel GetTreeQueryViewModel(DomainModel domain)
    {
        PropertyModel? treePropertyModel = domain.GetTreeGroupProperty();
        return new TreeQueryViewModel
        {
            Name = domain.Name,
            Annotation = domain.Annotation,
            TreeGroupProperty = treePropertyModel is null
                ? null
                : new TreeGroupPropertyViewModel
                {
                    Name = treePropertyModel.Name,
                    Annotation = treePropertyModel.Annotation,
                    NullPredefinedType = treePropertyModel.NullPredefinedType
                }
        };
    }

    private static string RenderTemplate(Template template, GeneratorCodeContext context, TreeServiceViewModel domain)
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

    private static string RenderTemplate(Template template, GeneratorCodeContext context, TreeQueryViewModel domain)
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
