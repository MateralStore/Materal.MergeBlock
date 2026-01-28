namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 服务模型代码生成插件
/// </summary>
public class ServicesModelGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ExcuteAsync(GeneratorCodeContext context)
    {
        foreach (DomainModel domain in context.Domains)
        {
            await GeneratorAddModelAsync(context, domain);
            await GeneratorEditModelAsync(context, domain);
            await GeneratorQueryModelAsync(context, domain, context.Domains);
            await GeneratorTreeQueryModelAsync(context, domain);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <summary>
    /// 创建添加模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorAddModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotAddAttribute>()) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.Services.Models.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}添加模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class Add{domain.Name}Model : IAddServiceModel");
        codeContent.AppendLine($"    {{");
        foreach (PropertyModel property in domain.Properties)
        {
            if (property.HasAttribute<NotAddAttribute>()) continue;
            GeneratorOperationalModelProperty(codeContent, property);
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "Services", "Models", domain.Name, $"Add{domain.Name}Model.cs");
    }

    /// <summary>
    /// 创建修改模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorEditModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotEditAttribute>()) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.Services.Models.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}修改模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class Edit{domain.Name}Model : IEditServiceModel");
        codeContent.AppendLine($"    {{");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 唯一标识");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        [Required(ErrorMessage = \"唯一标识为空\")]");
        codeContent.AppendLine($"        public Guid ID {{ get; set; }}");
        foreach (PropertyModel property in domain.Properties)
        {
            if (property.HasAttribute<NotEditAttribute>()) continue;
            GeneratorOperationalModelProperty(codeContent, property);
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "Services", "Models", domain.Name, $"Edit{domain.Name}Model.cs");
    }

    /// <summary>
    /// 创建查询模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    /// <param name="domains"></param>
    private async Task GeneratorQueryModelAsync(GeneratorCodeContext context, DomainModel domain, List<DomainModel> domains)
    {
        if (domain.HasAttribute<NotQueryAttribute>()) return;
        DomainModel targetDomain = domain.GetQueryDomain(domains);
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.Services.Models.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}查询模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class Query{domain.Name}Model : PageRequestModel, IQueryServiceModel");
        codeContent.AppendLine($"    {{");
        foreach (PropertyModel property in targetDomain.Properties)
        {
            if (domain.HasAttribute<NotQueryAttribute>() || !property.HasQueryAttribute) continue;
            if (property.HasAttribute<BetweenAttribute>())
            {
                if (property.Annotation is not null && !string.IsNullOrWhiteSpace(property.Annotation))
                {
                    codeContent.AppendLine($"        /// <summary>");
                    codeContent.AppendLine($"        /// 最小{property.Annotation}");
                    codeContent.AppendLine($"        /// </summary>");
                }
                codeContent.AppendLine($"        [GreaterThanOrEqual(\"{property.Name}\")]");
                codeContent.AppendLine($"        public {property.NullPredefinedType} Min{property.Name} {{ get; set; }}");
                if (property.Annotation is not null && !string.IsNullOrWhiteSpace(property.Annotation))
                {
                    codeContent.AppendLine($"        /// <summary>");
                    codeContent.AppendLine($"        /// 最大{property.Annotation}");
                    codeContent.AppendLine($"        /// </summary>");
                }
                codeContent.AppendLine($"        [LessThanOrEqual(\"{property.Name}\")]");
                codeContent.AppendLine($"        public {property.NullPredefinedType} Max{property.Name} {{ get; set; }}");
            }
            else
            {
                if (property.Annotation is not null && !string.IsNullOrWhiteSpace(property.Annotation))
                {
                    codeContent.AppendLine($"        /// <summary>");
                    codeContent.AppendLine($"        /// {property.Annotation}");
                    codeContent.AppendLine($"        /// </summary>");
                }
                string? queryAttributesCode = property.GetQueryAttributesCode();
                if (queryAttributesCode is not null && !string.IsNullOrWhiteSpace(queryAttributesCode))
                {
                    codeContent.AppendLine($"        {queryAttributesCode}");
                }
                codeContent.AppendLine($"        public {property.NullPredefinedType} {property.Name} {{ get; set; }}");
            }
        }
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 唯一标识组");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        [Contains(\"ID\")]");
        codeContent.AppendLine($"        public List<Guid>? IDs {{ get; set; }}");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 最小创建时间");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        [GreaterThanOrEqual(\"CreateTime\")]");
        codeContent.AppendLine($"        public DateTime? MinCreateTime {{ get; set; }}");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 最大创建时间");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        [LessThanOrEqual(\"CreateTime\")]");
        codeContent.AppendLine($"        public DateTime? MaxCreateTime {{ get; set; }}");
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "Services", "Models", domain.Name, $"Query{domain.Name}Model.cs");
    }

    /// <summary>
    /// 创建树查询模型
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorTreeQueryModelAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if ((!domain.IsTreeDomain && !domain.HasAttribute<EmptyTreeAttribute>()) || domain.HasAttribute<NotQueryAttribute>()) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.Services.Models.{domain.Name}");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}树查询模型");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class Query{domain.Name}TreeListModel : FilterModel");
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
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "Services", "Models", domain.Name, $"Query{domain.Name}TreeListModel.cs");
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