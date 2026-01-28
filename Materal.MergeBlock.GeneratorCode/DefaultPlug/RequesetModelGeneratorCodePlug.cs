namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 请求模型代码生成插件
/// </summary>
public class RequesetModelGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
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
            await GeneratorTreeQueryRequestModelAsync(context, domain);
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
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.RequestModel.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}添加请求模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class Add{domain.Name}RequestModel : IAddRequestModel");
        codeContent.AppendLine($"    {{");
        foreach (PropertyModel property in domain.Properties)
        {
            if (property.HasAttribute<NotAddAttribute>()) continue;
            if (property.HasAttribute<LoginUserIDAttribute>()) continue;
            GeneratorOperationalModelProperty(codeContent, property);
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "RequestModel", domain.Name, $"Add{domain.Name}RequestModel.cs");
    }

    /// <summary>
    /// 创建修改请求模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorEditRequestModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotEditAttribute>()) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.RequestModel.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}修改请求模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class Edit{domain.Name}RequestModel : IEditRequestModel");
        codeContent.AppendLine($"    {{");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 唯一标识");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        [Required(ErrorMessage = \"唯一标识为空\")]");
        codeContent.AppendLine($"        public Guid ID {{ get; set; }}");
        foreach (PropertyModel property in domain.Properties)
        {
            if (property.HasAttribute<NotEditAttribute>()) continue;
            if (property.HasAttribute<LoginUserIDAttribute>()) continue;
            GeneratorOperationalModelProperty(codeContent, property);
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "RequestModel", domain.Name, $"Edit{domain.Name}RequestModel.cs");
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
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.RequestModel.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}查询请求模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class Query{domain.Name}RequestModel : PageRequestModel, IQueryRequestModel");
        codeContent.AppendLine($"    {{");
        foreach (PropertyModel property in targetDomain.Properties)
        {
            if (!property.HasQueryAttribute || domain.HasAttribute<NotQueryAttribute>()) continue;
            if (!property.HasAttribute<BetweenAttribute>())
            {
                if (property.Annotation is not null && !string.IsNullOrWhiteSpace(property.Annotation))
                {
                    codeContent.AppendLine($"        /// <summary>");
                    codeContent.AppendLine($"        /// {property.Annotation}");
                    codeContent.AppendLine($"        /// </summary>");
                }
                codeContent.AppendLine($"        public {property.NullPredefinedType} {property.Name} {{ get; set; }}");
            }
            else
            {
                if (property.Annotation is not null && !string.IsNullOrWhiteSpace(property.Annotation))
                {
                    codeContent.AppendLine($"        /// <summary>");
                    codeContent.AppendLine($"        /// 最小{property.Annotation}");
                    codeContent.AppendLine($"        /// </summary>");
                }
                codeContent.AppendLine($"        public {property.NullPredefinedType} Min{property.Name} {{ get; set; }}");
                if (property.Annotation is not null && !string.IsNullOrWhiteSpace(property.Annotation))
                {
                    codeContent.AppendLine($"        /// <summary>");
                    codeContent.AppendLine($"        /// 最大{property.Annotation}");
                    codeContent.AppendLine($"        /// </summary>");
                }
                codeContent.AppendLine($"        public {property.NullPredefinedType} Max{property.Name} {{ get; set; }}");
            }
        }
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 唯一标识组");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        public List<Guid>? IDs {{ get; set; }}");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 最小创建时间");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        public DateTime? MinCreateTime {{ get; set; }}");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 最大创建时间");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        public DateTime? MaxCreateTime {{ get; set; }}");
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "RequestModel", domain.Name, $"Query{domain.Name}RequestModel.cs");
    }

    /// <summary>
    /// 创建树查询请求模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorTreeQueryRequestModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (!(domain.IsTreeDomain && !domain.HasAttribute<EmptyTreeAttribute>()) || domain.HasAttribute<NotQueryAttribute>()) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.RequestModel.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}树查询请求模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class Query{domain.Name}TreeListRequestModel : FilterModel");
        codeContent.AppendLine($"    {{");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 父级唯一标识");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        public Guid? ParentID {{ get; set; }}");
        PropertyModel? treePropertyModel = domain.GetTreeGroupProperty();
        if (treePropertyModel is not null)
        {
            codeContent.AppendLine($"        /// <summary>");
            codeContent.AppendLine($"        /// {treePropertyModel.Annotation}");
            codeContent.AppendLine($"        /// </summary>");
            codeContent.AppendLine($"        [Equal]");
            codeContent.AppendLine($"        public {treePropertyModel.NullPredefinedType} {treePropertyModel.Name} {{ get; set; }}");
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "RequestModel", domain.Name, $"Query{domain.Name}TreeListRequestModel.cs");
    }

    /// <summary>
    /// 创建操作模型属性
    /// </summary>
    /// <param name="codeContent"></param>
    /// <param name="property"></param>
    private void GeneratorOperationalModelProperty(StringBuilder codeContent, PropertyModel property)
    {
        if (property.Annotation is not null && !string.IsNullOrWhiteSpace(property.Annotation))
        {
            codeContent.AppendLine($"        /// <summary>");
            codeContent.AppendLine($"        /// {property.Annotation}");
            codeContent.AppendLine($"        /// </summary>");
        }
        string? verificationAttributesCode = property.GetVerificationAttributesCode();
        if (verificationAttributesCode is not null && !string.IsNullOrWhiteSpace(verificationAttributesCode))
        {
            codeContent.AppendLine($"        {verificationAttributesCode}");
        }
        if (property.HasAttribute<LoginUserIDAttribute>())
        {
            codeContent.AppendLine($"        [{nameof(LoginUserIDAttribute).RemoveAttributeSuffix()}]");
        }
        codeContent.AppendLine($"        public {property.PredefinedType} {property.Name} {{ get; set; }}");
        if (property.Initializer is not null && !string.IsNullOrWhiteSpace(property.Initializer))
        {
            codeContent.Insert(codeContent.Length - 2, $" = {property.Initializer};");
        }
    }
}
