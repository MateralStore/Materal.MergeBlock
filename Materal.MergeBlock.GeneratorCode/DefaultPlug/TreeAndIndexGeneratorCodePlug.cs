using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 树和位序生成代码插件
/// </summary>
public class TreeAndIndexGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    private class TreeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Annotation { get; set; }
        public bool IsIndexDomain { get; set; }
        public bool IsTreeDomain { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string PropertyNamesStr { get; set; } = string.Empty;
        public string ChildPropertyNamesStr { get; set; } = string.Empty;
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
        List<string> propertyNames = [.. domain.Properties.Select(m => m.Name)];
        List<string> allPropertyNames = ["ID", "CreateTime", "UpdateTime", .. propertyNames];
        TreeViewModel repositoryViewModel = new()
        {
            Name = domain.Name,
            Annotation = domain.Annotation,
            IsIndexDomain = domain.IsIndexDomain,
            ProjectName = context.ProjectName,
            ModuleName = context.ModuleName,
            PropertyNamesStr = string.Join(", ", allPropertyNames),
            ChildPropertyNamesStr = string.Join(", ", allPropertyNames.Select(m => $"child.{m}"))
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

    #region IService
    private static readonly string _iServiceTreeAndIndexTemplate = @"using Materal.MergeBlock.Abstractions.Models;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.DTO.{{ domain.Name }};

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Services;

/// <summary>
/// {{ domain.Annotation }}服务(TreeAndIndex)
/// </summary>
public partial interface I{{ domain.Name }}Service
{
    /// <summary>
    /// 获得树列表
    /// </summary>
    /// <param name=""parentID""></param>
    /// <returns></returns>
    Task<List<{{ domain.Name }}TreeListDTO>> GetTreeListAsync(Guid? parentID);

    /// <summary>
    /// 移动树节点
    /// </summary>
    /// <param name=""model""></param>
    /// <returns></returns>
    Task MoveTreeNodeAsync(MoveTreeNodeAndIndexModel model);
}";

    private async Task GeneratorIServiceCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || !domain.IsIndexDomain || domain.HasAttribute<NotTreeServiceAttribute>() || domain.HasAttribute<NotIndexServiceAttribute>()) return;

        Template template = Template.Parse(_iServiceTreeAndIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Services", $"I{domain.Name}Service.TreeAndIndex.cs");
    }
    #endregion

    #region ServiceImpl
    private static readonly string _serviceImplTreeAndIndexTemplate = @"using Materal.MergeBlock.Abstractions.Models;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Domain;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.DTO.{{ domain.Name }};
using Materal.Utils.Enums;

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Application.Services;

/// <summary>
/// {{ domain.Annotation }}服务(TreeAndIndex)
/// </summary>
public partial class {{ domain.Name }}ServiceImpl
{
    /// <summary>
    /// 获得树列表
    /// </summary>
    /// <param name=""parentID""></param>
    /// <returns></returns>
    public async Task<List<{{ domain.Name }}TreeListDTO>> GetTreeListAsync(Guid? parentID)
    {
        Dictionary<string, object> contextData = [];
        OnConvertTreeDTOInit(contextData);
        List<{{ domain.Name }}> domains;
        if (parentID is null)
        {
            domains = await DefaultRepository.FindAsync(m => true, m => m.Index, SortOrder.Ascending);
        }
        else
        {
            domains = DefaultRepository.GetAllRecursiveChildren(parentID.Value);
            domains = [.. domains.OrderBy(m => m.Index)];
        }
        OnConvertTreeDTOBefore(domains, contextData);
        List<{{ domain.Name }}TreeListDTO> result = domains.ToTree<{{ domain.Name }}, {{ domain.Name }}TreeListDTO>(parentID, (dto, domain) =>
        {
            OnConvertTreeDTO(dto, domain, contextData);
        }, true);
        OnConvertTreeDTOAfter(result, domains, contextData);
        return result;
    }

    /// <summary>
    /// 转换树DTO初始化时
    /// </summary>
    /// <param name=""contextData""></param>
    partial void OnConvertTreeDTOInit(Dictionary<string, object> contextData);

    /// <summary>
    /// 转换树DTO之前
    /// </summary>
    /// <param name=""domains""></param>
    /// <param name=""contextData""></param>
    partial void OnConvertTreeDTOBefore(List<{{ domain.Name }}> domains, Dictionary<string, object> contextData);

    /// <summary>
    /// 转换树DTO
    /// </summary>
    /// <param name=""dto""></param>
    /// <param name=""domain""></param>
    /// <param name=""contextData""></param>
    partial void OnConvertTreeDTO({{ domain.Name }}TreeListDTO dto, {{ domain.Name }} domain, Dictionary<string, object> contextData);

    /// <summary>
    /// 转换树DTO之后
    /// </summary>
    /// <param name=""dtos""></param>
    /// <param name=""domains""></param>
    /// <param name=""contextData""></param>
    partial void OnConvertTreeDTOAfter(List<{{ domain.Name }}TreeListDTO> dtos, List<{{ domain.Name }}> domains, Dictionary<string, object> contextData);

    /// <summary>
    /// 移动树节点
    /// </summary>
    /// <param name=""model""></param>
    /// <returns></returns>
    public async Task MoveTreeNodeAsync(MoveTreeNodeAndIndexModel model)
    {
        Dictionary<string, object> contextData = [];
        OnMoveTreeAndIndexBefore(model, contextData);
        {{- if domain.IndexGroupProperties.size > 0 }}
        List<{{ domain.Name }}> domains = await ServiceImplHelper.MoveAsync<I{{ domain.Name }}Repository, {{ domain.Name }}>(model, DefaultRepository, [{{- for param in domain.IndexGroupProperties }}nameof({{ domain.Name }}.{{ param.Name }}){{ if !for.last }}, {{ end }}{{ end }}]);
        {{- else }}
        List<{{ domain.Name }}> domains = await ServiceImplHelper.MoveAsync<I{{ domain.Name }}Repository, {{ domain.Name }}>(model, DefaultRepository, []);
        {{- end }}
        OnMoveTreeAndIndexAfter(model, contextData, domains);
        foreach ({{ domain.Name }} domain in domains)
        {
            UnitOfWork.RegisterEdit(domain);
        }
        await UnitOfWork.CommitAsync();
    }

    /// <summary>
    /// 移动树节点前
    /// </summary>
    /// <param name=""model""></param>
    /// <param name=""contextData""></param>
    /// <returns></returns>
    partial void OnMoveTreeAndIndexBefore(MoveTreeNodeAndIndexModel model, Dictionary<string, object> contextData);

    /// <summary>
    /// 移动树节点后
    /// </summary>
    /// <param name=""model""></param>
    /// <param name=""contextData""></param>
    /// <param name=""domain""></param>
    /// <returns></returns>
    partial void OnMoveTreeAndIndexAfter(MoveTreeNodeAndIndexModel model, Dictionary<string, object> contextData, List<{{ domain.Name }}> domain);
}";

    private async Task GeneratorServiceImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || !domain.IsIndexDomain || domain.HasAttribute<NotTreeServiceAttribute>() || domain.HasAttribute<NotIndexServiceAttribute>()) return;

        Template template = Template.Parse(_serviceImplTreeAndIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Services", $"{domain.Name}ServiceImpl.TreeAndIndex.cs");
    }
    #endregion

    #region IController
    private static readonly string _iControllerTreeAndIndexTemplate = @"using Materal.MergeBlock.Abstractions.Models;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.DTO.{{ domain.Name }};

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Controllers;

/// <summary>
/// {{ domain.Annotation }}控制器(TreeAndIndex)
/// </summary>
public partial interface I{{ domain.Name }}Controller
{
    /// <summary>
    /// 获得树列表
    /// </summary>
    /// <param name=""parentID""></param>
    /// <returns></returns>
    [HttpGet]
    Task<ResultModel<List<{{ domain.Name }}TreeListDTO>>> GetTreeListAsync(Guid? parentID);

    /// <summary>
    /// 移动树节点
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>
    [HttpPut]
    Task<ResultModel> MoveTreeNodeAsync(MoveTreeNodeAndIndexRequestModel requestModel);
}";

    private async Task GeneratorIControllerCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || !domain.IsIndexDomain || domain.HasAttribute<NotTreeControllerAttribute>() || domain.HasAttribute<NotIndexControllerAttribute>()) return;

        Template template = Template.Parse(_iControllerTreeAndIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Controllers", $"I{domain.Name}Controller.TreeAndIndex.cs");
    }
    #endregion

    #region ControllerImpl
    private static readonly string _controllerImplTreeAndIndexTemplate = @"using Materal.MergeBlock.Abstractions.Models;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.DTO.{{ domain.Name }};

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Application.Controllers;

/// <summary>
/// {{ domain.Annotation }}控制器(TreeAndIndex)
/// </summary>
public partial class {{ domain.Name }}Controller
{
    /// <summary>
    /// 获得树列表
    /// </summary>
    /// <param name=""parentID""></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ResultModel<List<{{ domain.Name}}TreeListDTO>>> GetTreeListAsync(Guid? parentID)
    {
        List<{{ domain.Name}}TreeListDTO> result = await DefaultService.GetTreeListAsync(parentID);
        return ResultModel<List<{{ domain.Name}}TreeListDTO>>.Success(result, ""获得树列表成功"");
    }

    /// <summary>
    /// 移动树节点
    /// </summary>
    /// <param name=""requestModel""></param>
    /// <returns></returns>    
    [HttpPut]
    public async Task<ResultModel> MoveTreeNodeAsync(MoveTreeNodeAndIndexRequestModel requestModel)
    {
        MoveTreeNodeAndIndexModel model = Mapper.Map<MoveTreeNodeAndIndexModel>(requestModel) ?? throw new {{ domain.ProjectName }}Exception(""映射失败"");
        await DefaultService.MoveTreeNodeAsync(model);
        return ResultModel.Success(""移动树节点成功"");
    }
}";

    private async Task GeneratorControllerImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || !domain.IsIndexDomain || domain.HasAttribute<NotTreeControllerAttribute>() || domain.HasAttribute<NotIndexControllerAttribute>()) return;

        Template template = Template.Parse(_controllerImplTreeAndIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Controllers", $"{domain.Name}Controller.TreeAndIndex.cs");
    }
    #endregion
}
