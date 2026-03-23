using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 树生成代码插件
/// </summary>
public class TreeGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    private class TreeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Annotation { get; set; }
        public bool IsIndexDomain { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string PropertyNamesStr { get; set; } = string.Empty;
        public string ChildPropertyNamesStr { get; set; } = string.Empty;
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
            await GeneratorTreeDTOCodeAsync(context, domain);
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
    private static readonly string _iRepositoryIndexTemplate = @"using Materal.MergeBlock.Repository.Abstractions.Repositories;

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Repositories;

/// <summary>
/// {{ domain.Annotation }}仓储(Tree)
/// </summary>
public partial interface I{{ domain.Name }}Repository : ITreeRepository<{{ domain.Name }}>
{
}";

    private async Task GeneratorIRepositoryCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.HasAttribute<NotTreeRepositoryAttribute>()) return;

        Template template = Template.Parse(_iRepositoryIndexTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Repositories", $"I{domain.Name}Repository.Tree.cs");
    }
    #endregion

    #region RepositoryImpl
    private static readonly string _repositoryImplTemplate = @"namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Repository.Repositories;
/// <summary>
/// {{ domain.Annotation }}仓储(Tree)
/// </summary>
public partial class {{ domain.Name }}RepositoryImpl
{
    /// <summary>
    /// 获取指定节点的递归子级
    /// </summary>
    /// <param name=""parentID"">父节点ID</param>
    /// <returns>所有递归子级</returns>
    public List<{{ domain.Name }}> GetAllRecursiveChildren(Guid parentID)
    {
        const string sql = """"""
            WITH TreeNodes({{ domain.PropertyNamesStr }}, Depth) AS (
                SELECT {{ domain.PropertyNamesStr }}, 0 AS Depth
                FROM {{ domain.Name }}
                WHERE ID = @RootID
                UNION ALL
                SELECT {{ domain.ChildPropertyNamesStr }}, parent.Depth + 1
                FROM {{ domain.Name }} child
                INNER JOIN TreeNodes parent ON child.ParentID = parent.ID
            )
            SELECT *
            FROM TreeNodes
            ORDER BY Depth DESC;
            """""";
        return ExcuteQuerySql<{{ domain.Name }}>(sql, [GetParameter(""@RootID"", parentID)]);
    }

    /// <summary>
    /// 获取指定节点的递归子级唯一标识
    /// </summary>
    /// <param name=""parentID"">父节点ID</param>
    /// <returns>所有递归子级ID</returns>
    public List<Guid> GetAllRecursiveChildrenID(Guid parentID)
    {
        const string sql = """"""
            WITH TreeNodes(ID, Depth) AS (
                SELECT ID, 0 AS Depth
                FROM {{ domain.Name }}
                WHERE ID = @RootID
                UNION ALL
                SELECT child.ID, parent.Depth + 1
                FROM {{ domain.Name }} child
                INNER JOIN TreeNodes parent ON child.ParentID = parent.ID
            )
            SELECT ID
            FROM TreeNodes
            ORDER BY Depth DESC;
            """""";
        return ExcuteQuerySql(sql, [GetParameter(""@RootID"", parentID)], dr => dr.GetGuid(0));
    }
}";

    private async Task GeneratorRepositoryImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.HasAttribute<NotTreeRepositoryAttribute>()) return;

        Template template = Template.Parse(_repositoryImplTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleRepositoryMGCPath, "Repositories", $"{domain.Name}RepositoryImpl.Tree.cs");
    }
    #endregion

    #region DTO
    private static readonly string _treeDTOTemplate = @"namespace {{ domain.ProjectName }}.{{ domain.ModuleName }}.Abstractions.DTO.{{ domain.Name }};

/// <summary>
/// {{ domain.Annotation }}树列表数据传输模型
/// </summary>
public partial class {{ domain.Name }}TreeListDTO : {{ domain.Name }}ListDTO, ITreeDTO<{{ domain.Name }}TreeListDTO>
{
    /// <summary>
    /// 子级
    /// </summary>
    public List<{{ domain.Name }}TreeListDTO> Children { get; set; } = [];
}";

    private async Task GeneratorTreeDTOCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.HasAttribute<NotTreeDTOAttribute>()) return;

        Template template = Template.Parse(_treeDTOTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "DTO", domain.Name, $"{domain.Name}TreeListDTO.cs");
    }
    #endregion

    #region IService
    private static readonly string _iServiceTreeTemplate = @"using Materal.MergeBlock.Abstractions.Models;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.DTO.{{ domain.Name }};

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Services;

/// <summary>
/// {{ domain.Annotation }}服务(Tree)
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
    Task MoveTreeNodeAsync(MoveTreeNodeModel model);
}";

    private async Task GeneratorIServiceCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.IsIndexDomain || domain.HasAttribute<NotTreeServiceAttribute>()) return;

        Template template = Template.Parse(_iServiceTreeTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Services", $"I{domain.Name}Service.Tree.cs");
    }
    #endregion

    #region ServiceImpl
    private static readonly string _serviceImplTreeTemplate = @"using Materal.MergeBlock.Abstractions.Models;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Domain;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.DTO.{{ domain.Name }};

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Application.Services;

/// <summary>
/// {{ domain.Annotation }}服务(Tree)
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
            domains = await DefaultRepository.FindAsync(m => true);
        }
        else
        {
            domains = DefaultRepository.GetAllRecursiveChildren(parentID.Value);
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
    public async Task MoveTreeNodeAsync(MoveTreeNodeModel model)
    {
        Dictionary<string, object> contextData = [];
        OnMoveTreeBefore(model, contextData);
        {{ domain.Name }} domain = await ServiceImplHelper.MoveAsync<I{{ domain.Name }}Repository, {{ domain.Name }}>(model, DefaultRepository);
        OnMoveTreeAfter(model, contextData, domain);
        UnitOfWork.RegisterEdit(domain);
        await UnitOfWork.CommitAsync();
    }

    /// <summary>
    /// 移动树节点之前
    /// </summary>
    /// <param name=""model""></param>
    /// <param name=""contextData""></param>
    /// <returns></returns>
    partial void OnMoveTreeBefore(MoveTreeNodeModel model, Dictionary<string, object> contextData);

    /// <summary>
    /// 移动树节点之后
    /// </summary>
    /// <param name=""model""></param>
    /// <param name=""contextData""></param>
    /// <param name=""domain""></param>
    /// <returns></returns>
    partial void OnMoveTreeAfter(MoveTreeNodeModel model, Dictionary<string, object> contextData, {{ domain.Name }} domain);
}";

    private async Task GeneratorServiceImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.IsIndexDomain || domain.HasAttribute<NotTreeServiceAttribute>()) return;

        Template template = Template.Parse(_serviceImplTreeTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Services", $"{domain.Name}ServiceImpl.Tree.cs");
    }
    #endregion

    #region IController
    private static readonly string _iControllerTreeTemplate = @"using Materal.MergeBlock.Abstractions.Models;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.DTO.{{ domain.Name }};

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.Controllers;

/// <summary>
/// {{ domain.Annotation }}控制器(Tree)
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
    Task<ResultModel> MoveTreeNodeAsync(MoveTreeNodeRequestModel requestModel);
}";

    private async Task GeneratorIControllerCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.IsIndexDomain || domain.HasAttribute<NotTreeControllerAttribute>()) return;

        Template template = Template.Parse(_iControllerTreeTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Controllers", $"I{domain.Name}Controller.Tree.cs");
    }
    #endregion

    #region ControllerImpl
    private static readonly string _controllerImplTreeTemplate = @"using Materal.MergeBlock.Abstractions.Models;
using {{ domain.ProjectName}}.{{ domain.ModuleName }}.Abstractions.DTO.{{ domain.Name }};

namespace {{ domain.ProjectName}}.{{ domain.ModuleName }}.Application.Controllers;

/// <summary>
/// {{ domain.Annotation }}控制器(Tree)
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
    public async Task<ResultModel> MoveTreeNodeAsync(MoveTreeNodeRequestModel requestModel)
    {
        MoveTreeNodeModel model = Mapper.Map<MoveTreeNodeModel>(requestModel) ?? throw new {{ domain.ProjectName }}Exception(""映射失败"");
        await DefaultService.MoveTreeNodeAsync(model);
        return ResultModel.Success(""移动树节点成功"");
    }
}";

    private async Task GeneratorControllerImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!domain.IsTreeDomain || domain.IsIndexDomain || domain.HasAttribute<NotTreeControllerAttribute>()) return;

        Template template = Template.Parse(_controllerImplTreeTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Controllers", $"{domain.Name}Controller.Tree.cs");
    }
    #endregion
}
