namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 数据传输模型代码生成插件
/// </summary>
public class DTOGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ExcuteAsync(GeneratorCodeContext context)
    {
        foreach (DomainModel domain in context.Domains)
        {
            await GeneratorListDTOModelAsync(context, domain, context.Domains);
            await GeneratorDTOModelAsync(context, domain, context.Domains);
            await GeneratorTreeListDTOModelAsync(context, domain);
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
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.DTO.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}列表数据传输模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class {domain.Name}ListDTO : IListDTO");
        codeContent.AppendLine($"    {{");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 唯一标识");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        [Required(ErrorMessage = \"唯一标识为空\")]");
        codeContent.AppendLine($"        public Guid ID {{ get; set; }}");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 创建时间");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        [Required(ErrorMessage = \"创建时间为空\")]");
        codeContent.AppendLine($"        public DateTime CreateTime {{ get; set; }}");
        foreach (PropertyModel property in targetDomain.Properties)
        {
            if (property.HasAttribute<NotDTOAttribute>() || property.HasAttribute<NotListDTOAttribute>()) continue;
            GeneratorDTOModelProperty(codeContent, property);
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "DTO", domain.Name, $"{domain.Name}ListDTO.cs");
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
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.DTO.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}数据传输模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class {domain.Name}DTO : {domain.Name}ListDTO, IDTO");
        codeContent.AppendLine($"    {{");
        foreach (PropertyModel property in targetDomain.Properties)
        {
            if (property.HasAttribute<NotDTOAttribute>() || !property.HasAttribute<NotListDTOAttribute>()) continue;
            GeneratorDTOModelProperty(codeContent, property);
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "DTO", domain.Name, $"{domain.Name}DTO.cs");
    }

    /// <summary>
    /// 创建树列表数据传输模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorTreeListDTOModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!(domain.IsTreeDomain && !domain.HasAttribute<EmptyTreeAttribute>()) || domain.HasAttribute<NotListDTOAttribute>()) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.DTO.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}树列表数据传输模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class {domain.Name}TreeListDTO : {domain.Name}ListDTO, ITreeDTO<{domain.Name}TreeListDTO>");
        codeContent.AppendLine($"    {{");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 子级");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        public List<{domain.Name}TreeListDTO> Children {{ get; set; }} = [];");
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "DTO", domain.Name, $"{domain.Name}TreeListDTO.cs");
    }

    /// <summary>
    /// 创建数据传输模型属性
    /// </summary>
    /// <param name="codeContent"></param>
    /// <param name="property"></param>
    private void GeneratorDTOModelProperty(StringBuilder codeContent, PropertyModel property)
    {
        if (property.Annotation is not null && !string.IsNullOrWhiteSpace(property.Annotation))
        {
            codeContent.AppendLine($"        /// <summary>");
            codeContent.AppendLine($"        /// {property.Annotation}");
            codeContent.AppendLine($"        /// </summary>");
        }
        string? queryAttributesCode = property.GetVerificationAttributesCode();
        if (queryAttributesCode is not null && !string.IsNullOrWhiteSpace(queryAttributesCode))
        {
            codeContent.AppendLine($"        {queryAttributesCode}");
        }
        codeContent.AppendLine($"        public {property.PredefinedType} {property.Name} {{ get; set; }}");
        if (property.Initializer is not null && !string.IsNullOrWhiteSpace(property.Initializer))
        {
            codeContent.Insert(codeContent.Length - 2, $" = {property.Initializer};");
        }
        if (property.HasAttribute<DTOTextAttribute>())
        {
            if (property.Annotation is not null && !string.IsNullOrWhiteSpace(property.Annotation))
            {
                codeContent.AppendLine($"        /// <summary>");
                codeContent.AppendLine($"        /// {property.Annotation}文本");
                codeContent.AppendLine($"        /// </summary>");
            }
            if (property.CanNull)
            {
                codeContent.AppendLine($"        public string? {property.Name}Text => {property.Name}?.GetDescriptionOrNull();");
            }
            else
            {
                codeContent.AppendLine($"        public string {property.Name}Text => {property.Name}.GetDescription();");
            }
        }
    }
}