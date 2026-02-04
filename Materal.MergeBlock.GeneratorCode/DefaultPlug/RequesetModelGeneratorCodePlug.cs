using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 请求模型代码生成插件
/// </summary>
public class RequesetModelGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <summary>
    /// 请求模型视图模型（用于模板渲染）
    /// </summary>
    private class RequestModelViewModel
    {
        /// <summary>
        /// 领域模型名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 领域模型注释
        /// </summary>
        public string? Annotation { get; set; }

        /// <summary>
        /// 属性列表
        /// </summary>
        public List<PropertyViewModel> Properties { get; set; } = [];
    }

    /// <summary>
    /// 属性视图模型（用于模板渲染）
    /// </summary>
    private class PropertyViewModel
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 属性注释
        /// </summary>
        public string? Annotation { get; set; }

        /// <summary>
        /// 预定义类型
        /// </summary>
        public string PredefinedType { get; set; } = string.Empty;

        /// <summary>
        /// 可空预定义类型
        /// </summary>
        public string NullPredefinedType { get; set; } = string.Empty;

        /// <summary>
        /// 验证特性代码
        /// </summary>
        public string? VerificationAttributesCode { get; set; }

        /// <summary>
        /// 是否有 LoginUserID 特性
        /// </summary>
        public bool HasLoginUserIDAttribute { get; set; }

        /// <summary>
        /// 初始化表达式
        /// </summary>
        public string? Initializer { get; set; }

        /// <summary>
        /// 是否有 Between 特性（用于查询模型）
        /// </summary>
        public bool HasBetweenAttribute { get; set; }
    }

    private static readonly string _addRequestModelTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};
/// <summary>
/// {{domain.Annotation}}添加请求模型
/// </summary>
public partial class Add{{domain.Name}}RequestModel : IAddRequestModel
{
{{- for property in domain.Properties}}
    {{- if property.Annotation}}
    /// <summary>
    /// {{property.Annotation}}
    /// </summary>
    {{- end}}
    {{- if property.VerificationAttributesCode}}
    {{property.VerificationAttributesCode}}
    {{- end}}
    {{- if property.HasLoginUserIDAttribute}}
    [LoginUserID]
    {{- end}}
    public {{property.PredefinedType}} {{property.Name}} { get; set; }{{if property.Initializer}} = {{property.Initializer}};{{end}}
{{- end}}
}
";

    private static readonly string _editRequestModelTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};
/// <summary>
/// {{domain.Annotation}}修改请求模型
/// </summary>
public partial class Edit{{domain.Name}}RequestModel : IEditRequestModel
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    [Required(ErrorMessage = ""唯一标识为空"")]
    public Guid ID { get; set; }
{{- for property in domain.Properties}}
    {{- if property.Annotation}}
    /// <summary>
    /// {{property.Annotation}}
    /// </summary>
    {{- end}}
    {{- if property.VerificationAttributesCode}}
    {{property.VerificationAttributesCode}}
    {{- end}}
    {{- if property.HasLoginUserIDAttribute}}
    [LoginUserID]
    {{- end}}
    public {{property.PredefinedType}} {{property.Name}} { get; set; }{{if property.Initializer}} = {{property.Initializer}};{{end}}
{{- end}}
}
";

    private static readonly string _queryRequestModelTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.RequestModel.{{domain.Name}};
/// <summary>
/// {{domain.Annotation}}查询请求模型
/// </summary>
public partial class Query{{domain.Name}}RequestModel : PageRequestModel, IQueryRequestModel
{
{{- for property in domain.Properties}}
    {{- if !property.HasBetweenAttribute}}
        {{- if property.Annotation}}
    /// <summary>
    /// {{property.Annotation}}
    /// </summary>
        {{- end}}
    public {{property.NullPredefinedType}} {{property.Name}} { get; set; }
    {{- else}}
        {{- if property.Annotation}}
    /// <summary>
    /// 最小{{property.Annotation}}
    /// </summary>
        {{- end}}
    public {{property.NullPredefinedType}} Min{{property.Name}} { get; set; }
        {{- if property.Annotation}}
    /// <summary>
    /// 最大{{property.Annotation}}
    /// </summary>
        {{- end}}
    public {{property.NullPredefinedType}} Max{{property.Name}} { get; set; }
    {{- end}}
{{- end}}
    /// <summary>
    /// 唯一标识组
    /// </summary>
    public List<Guid>? IDs { get; set; }
    /// <summary>
    /// 最小创建时间
    /// </summary>
    public DateTime? MinCreateTime { get; set; }
    /// <summary>
    /// 最大创建时间
    /// </summary>
    public DateTime? MaxCreateTime { get; set; }
}
";

    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ExcuteAsync(GeneratorCodeContext context)
    {
        foreach (DomainModel domain in context.Domains)
        {
            await GeneratorAddRequestModelAsync(context, domain);
            await GeneratorEditRequestModelAsync(context, domain);
            await GeneratorQueryRequestModelAsync(context, domain, context.Domains);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <summary>
    /// 创建添加请求模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorAddRequestModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotAddAttribute>()) return;

        Template template = Template.Parse(_addRequestModelTemplate);
        string codeContent = RenderTemplate(template, context, domain, forAddModel: true);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "RequestModel", domain.Name, $"Add{domain.Name}RequestModel.cs");
    }

    /// <summary>
    /// 创建修改请求模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorEditRequestModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotEditAttribute>()) return;

        Template template = Template.Parse(_editRequestModelTemplate);
        string codeContent = RenderTemplate(template, context, domain, forEditModel: true);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "RequestModel", domain.Name, $"Edit{domain.Name}RequestModel.cs");
    }

    /// <summary>
    /// 创建查询请求模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    /// <param name="domains"></param>
    private async Task GeneratorQueryRequestModelAsync(GeneratorCodeContext context, DomainModel domain, List<DomainModel> domains)
    {
        if (domain.HasAttribute<NotQueryAttribute>()) return;
        DomainModel targetDomain = domain.GetQueryDomain(domains);

        Template template = Template.Parse(_queryRequestModelTemplate);
        string codeContent = RenderTemplate(template, context, domain, targetDomain: targetDomain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "RequestModel", domain.Name, $"Query{domain.Name}RequestModel.cs");
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    /// <param name="template"></param>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    /// <param name="forAddModel">是否为添加模型</param>
    /// <param name="forEditModel">是否为编辑模型</param>
    /// <param name="targetDomain">目标领域（用于查询模型）</param>
    /// <returns></returns>
    private static string RenderTemplate(
        Template template,
        GeneratorCodeContext context,
        DomainModel domain,
        bool forAddModel = false,
        bool forEditModel = false,
        DomainModel? targetDomain = null)
    {
        var requestModelViewModel = new RequestModelViewModel
        {
            Name = domain.Name,
            Annotation = domain.Annotation
        };

        // 根据不同的模型类型过滤属性
        if (forAddModel)
        {
            requestModelViewModel.Properties = [.. domain.Properties
                .Where(p => !p.HasAttribute<NotAddAttribute>() && !p.HasAttribute<LoginUserIDAttribute>())
                .Select(p => ToPropertyViewModel(p))];
        }
        else if (forEditModel)
        {
            requestModelViewModel.Properties = [.. domain.Properties
                .Where(p => !p.HasAttribute<NotEditAttribute>() && !p.HasAttribute<LoginUserIDAttribute>())
                .Select(p => ToPropertyViewModel(p))];
        }
        else if (targetDomain is not null)
        {
            // 查询模型使用目标领域
            requestModelViewModel.Properties = [.. targetDomain.Properties
                .Where(p => p.HasQueryAttribute && !domain.HasAttribute<NotQueryAttribute>())
                .Select(p => ToPropertyViewModel(p, forQueryModel: true))];
        }

        ScriptObject scriptObject = new()
        {
            { "context", context },
            { "domain", requestModelViewModel }
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
    /// 将属性模型转换为视图模型
    /// </summary>
    /// <param name="property"></param>
    /// <param name="forQueryModel">是否为查询模型</param>
    /// <returns></returns>
    private static PropertyViewModel ToPropertyViewModel(PropertyModel property, bool forQueryModel = false) => new()
    {
        Name = property.Name,
        Annotation = property.Annotation,
        PredefinedType = property.PredefinedType,
        NullPredefinedType = property.NullPredefinedType,
        VerificationAttributesCode = forQueryModel ? null : property.GetVerificationAttributesCode(),
        HasLoginUserIDAttribute = property.HasAttribute<LoginUserIDAttribute>(),
        Initializer = property.Initializer,
        HasBetweenAttribute = property.HasAttribute<BetweenAttribute>()
    };
}
