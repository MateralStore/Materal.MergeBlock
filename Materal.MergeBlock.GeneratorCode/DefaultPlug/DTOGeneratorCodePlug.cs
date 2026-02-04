using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 数据传输模型代码生成插件
/// </summary>
public class DTOGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
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
        /// 属性类型（如：string、int?）
        /// </summary>
        public string PredefinedType { get; set; } = string.Empty;

        /// <summary>
        /// 属性注释（XML文档注释中的summary内容）
        /// </summary>
        public string? Annotation { get; set; }

        /// <summary>
        /// 属性初始值（如："0"、"new List()"）
        /// </summary>
        public string? Initializer { get; set; }

        /// <summary>
        /// 属性是否可空（类型以?结尾）
        /// </summary>
        public bool CanNull { get; set; }

        /// <summary>
        /// 是否包含在ListDTO中
        /// </summary>
        public bool IncludeInListDto { get; set; }

        /// <summary>
        /// 是否包含在DTO中（ListDTO的额外属性）
        /// </summary>
        public bool IncludeInDto { get; set; }

        /// <summary>
        /// 验证特性代码（如：[Required(ErrorMessage = "名称不能为空")]）
        /// </summary>
        public string? VerificationAttributesCode { get; set; }
    }

    /// <summary>
    /// 领域模型视图模型（用于模板渲染）
    /// </summary>
    private class DomainViewModel
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
        /// 属性列表（用于生成DTO属性）
        /// </summary>
        public List<PropertyViewModel> Properties { get; set; } = [];
    }

    private static readonly string _listDtoTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};

/// <summary>
/// {{domain.Annotation}}列表数据传输模型
/// </summary>
public partial class {{domain.Name}}ListDTO : IListDTO
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    [Required(ErrorMessage = ""唯一标识为空"")]
    public Guid ID { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    [Required(ErrorMessage = ""创建时间为空"")]
    public DateTime CreateTime { get; set; }

    {{- for property in domain.Properties}}
        {{- if property.IncludeInListDto}}
    {{- if property.Annotation}}
    /// <summary>
    /// {{property.Annotation}}
    /// </summary>
    {{- end}}
    {{- if property.VerificationAttributesCode}}
    {{property.VerificationAttributesCode}}
    {{- end}}
    public {{property.PredefinedType}} {{property.Name}} { get; set; }{{if property.Initializer != null}} = {{property.Initializer}};{{end}}
        {{- end}}
    {{- end}}
}
";

    private static readonly string _dtoTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.DTO.{{domain.Name}};

/// <summary>
/// {{domain.Annotation}}数据传输模型
/// </summary>
public partial class {{domain.Name}}DTO : {{domain.Name}}ListDTO, IDTO
{
    {{- for property in domain.Properties}}
        {{- if property.IncludeInDto}}
    {{- if property.Annotation}}
    /// <summary>
    /// {{property.Annotation}}
    /// </summary>
    {{- end}}
    {{- if property.VerificationAttributesCode}}
    {{property.VerificationAttributesCode}}
    {{- end}}
    public {{property.PredefinedType}} {{property.Name}} { get; set; }{{if property.Initializer != null}} = {{property.Initializer}};{{end}}
        {{- end}}
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
            await GeneratorListDTOModelAsync(context, domain, context.Domains);
            await GeneratorDTOModelAsync(context, domain, context.Domains);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <summary>
    /// 创建列表数据传输模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    /// <param name="domains"></param>
    private async Task GeneratorListDTOModelAsync(GeneratorCodeContext context, DomainModel domain, List<DomainModel> domains)
    {
        if (domain.HasAttribute<NotListDTOAttribute>()) return;
        DomainModel targetDomain = domain.GetQueryDomain(domains);

        Template template = Template.Parse(_listDtoTemplate);
        string codeContent = RenderTemplate(template, context, domain, targetDomain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "DTO", domain.Name, $"{domain.Name}ListDTO.cs");
    }

    /// <summary>
    /// 创建数据传输模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    /// <param name="domains"></param>
    private async Task GeneratorDTOModelAsync(GeneratorCodeContext context, DomainModel domain, List<DomainModel> domains)
    {
        if (domain.HasAttribute<NotDTOAttribute>()) return;
        DomainModel targetDomain = domain.GetQueryDomain(domains);

        Template template = Template.Parse(_dtoTemplate);
        string codeContent = RenderTemplate(template, context, domain, targetDomain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "DTO", domain.Name, $"{domain.Name}DTO.cs");
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    /// <param name="template"></param>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    /// <param name="targetDomain"></param>
    /// <returns></returns>
    private static string RenderTemplate(Template template, GeneratorCodeContext context, DomainModel domain, DomainModel? targetDomain)
    {
        var domainViewModel = new DomainViewModel
        {
            Name = domain.Name,
            Annotation = domain.Annotation
        };

        if (targetDomain != null)
        {
            domainViewModel.Properties = [.. targetDomain.Properties
                .Where(p => !p.HasAttribute<NotDTOAttribute>() || !p.HasAttribute<NotListDTOAttribute>())
                .Select(p => new PropertyViewModel
                {
                    Name = p.Name,
                    PredefinedType = p.PredefinedType,
                    Annotation = p.Annotation,
                    Initializer = p.Initializer,
                    CanNull = p.CanNull,
                    IncludeInListDto = !p.HasAttribute<NotDTOAttribute>() && !p.HasAttribute<NotListDTOAttribute>(),
                    IncludeInDto = p.HasAttribute<NotDTOAttribute>() && !p.HasAttribute<NotListDTOAttribute>(),
                    VerificationAttributesCode = p.GetVerificationAttributesCode()
                })];
        }

        ScriptObject scriptObject = new()
        {
            { "context", context },
            { "domain", domainViewModel }
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
