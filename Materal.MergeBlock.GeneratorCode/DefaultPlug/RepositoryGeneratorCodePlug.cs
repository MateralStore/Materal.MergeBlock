namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 仓储代码生成插件
/// </summary>
public class RepositoryGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task ExcuteAsync(GeneratorCodeContext context)
    {
        foreach (DomainModel domain in context.Domains)
        {
            await GeneratorEntityConfigCodeAsync(context, domain);
        }
        await GeneratorDBContextCodeAsync(context);
        foreach (DomainModel domain in context.Domains)
        {
            await GeneratorIRepositoryCodeAsync(context, domain);
            await GeneratorRepositoryImplCodeAsync(context, domain);
        }
    }

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <summary>
    /// 创建实体配置代码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorEntityConfigCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotEntityConfigAttribute>()) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"using Microsoft.EntityFrameworkCore.Metadata.Builders;");
        codeContent.AppendLine($"");
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Repository.EntityConfigs");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}配置基类");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public class {domain.Name}ConfigBase : BaseEntityConfig<{domain.Name}>");
        codeContent.AppendLine($"    {{");
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 配置");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        public override void Configure(EntityTypeBuilder<{domain.Name}> builder)");
        codeContent.AppendLine($"        {{");
        codeContent.AppendLine($"            builder = BaseConfigure(builder);");
        if (domain.IsView)
        {
            codeContent.AppendLine($"            builder.ToView(\"{domain.Name}\");");
        }
        else
        {
            codeContent.AppendLine($"            builder.ToTable(m => m.HasComment(\"{domain.Annotation}\"));");
        }
        foreach (PropertyModel property in domain.Properties)
        {
            if (property.HasAttribute<NotEntityConfigAttribute>()) continue;
            codeContent.AppendLine($"            builder.Property(e => e.{property.Name})");
            if (!property.CanNull)
            {
                codeContent.AppendLine($"                .IsRequired()");
            }
            codeContent.AppendLine($"                .HasComment(\"{property.Annotation}\")");
            AttributeArgumentModel? columnTypeArgument = property.GetAttribute<ColumnTypeAttribute>()?.GetAttributeArgument();
            if (columnTypeArgument is not null)
            {
                codeContent.AppendLine($"                .HasColumnType({columnTypeArgument.Value})");
            }
            AttributeModel? attribute = property.GetAttribute<StringLengthAttribute>();
            if (attribute is not null)
            {
                codeContent.AppendLine($"                .HasMaxLength({attribute.GetAttributeArgument()?.Value})");
            }
            codeContent.Insert(codeContent.Length - 2, ";");
        }
        codeContent.AppendLine($"        }}");
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}配置类");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public partial class {domain.Name}Config : {domain.Name}ConfigBase {{ }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleRepositoryMGCPath, "EntityConfigs", $"{domain.Name}Config.cs");
    }

    /// <summary>
    /// 创建数据库上下文代码
    /// </summary>
    /// <param name="context"></param>
    private async Task GeneratorDBContextCodeAsync(GeneratorCodeContext context)
    {
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Repository");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {context.ModuleName}数据库上下文");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    public sealed partial class {context.ModuleName}DBContext(DbContextOptions<{context.ModuleName}DBContext> options) : DbContext(options)");
        codeContent.AppendLine($"    {{");
        foreach (DomainModel domain in context.Domains)
        {
            if (domain.HasAttribute<NotInDBContextAttribute>()) continue;
            codeContent.AppendLine($"        /// <summary>");
            codeContent.AppendLine($"        /// {domain.Annotation}");
            codeContent.AppendLine($"        /// </summary>");
            codeContent.AppendLine($"        public DbSet<{domain.Name}>? {domain.Name} {{ get; set; }}");
        }
        codeContent.AppendLine($"        /// <summary>");
        codeContent.AppendLine($"        /// 配置模型");
        codeContent.AppendLine($"        /// </summary>");
        codeContent.AppendLine($"        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);");
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleRepositoryMGCPath, $"{context.ModuleName}DBContext.cs");
    }

    /// <summary>
    /// 创建仓储接口代码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorIRepositoryCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotRepositoryAttribute>()) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.Repositories");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}仓储");
        codeContent.AppendLine($"    /// </summary>");
        if (domain.HasAttribute<CacheAttribute>())
        {
            codeContent.AppendLine($"    public partial interface I{domain.Name}Repository : I{context.ModuleName}CacheRepository<{domain.Name}>");
        }
        else
        {
            codeContent.AppendLine($"    public partial interface I{domain.Name}Repository : I{context.ModuleName}Repository<{domain.Name}>");
        }
        codeContent.AppendLine($"    {{");
        if (domain.IsIndexDomain && !domain.HasAttribute<EmptyIndexAttribute>())
        {
            PropertyModel? indexGroupPropertyModel = domain.GetIndexGroupProperty();
            if (indexGroupPropertyModel is null)
            {
                codeContent.AppendLine($"        /// <summary>");
                codeContent.AppendLine($"        /// 获取最大位序");
                codeContent.AppendLine($"        /// </summary>");
                codeContent.AppendLine($"        /// <returns></returns>");
                codeContent.AppendLine($"        Task<int> GetMaxIndexAsync();");
            }
            else
            {
                codeContent.AppendLine($"        /// <summary>");
                codeContent.AppendLine($"        /// 获取最大位序");
                codeContent.AppendLine($"        /// </summary>");
                codeContent.AppendLine($"        /// <param name=\"{indexGroupPropertyModel.Name.ToLowerFirstLetter()}\"></param>");
                codeContent.AppendLine($"        /// <returns></returns>");
                codeContent.AppendLine($"        Task<int> GetMaxIndexAsync({indexGroupPropertyModel.PredefinedType} {indexGroupPropertyModel.Name.ToLowerFirstLetter()});");
            }
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleAbstractionsMGCPath, "Repositories", $"I{domain.Name}Repository.cs");
    }

    /// <summary>
    /// 创建仓储实现代码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorRepositoryImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotRepositoryAttribute>()) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Repository.Repositories");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// {domain.Annotation}仓储");
        codeContent.AppendLine($"    /// </summary>");
        if (domain.HasAttribute<CacheAttribute>())
        {
            codeContent.AppendLine($"    public partial class {domain.Name}RepositoryImpl({context.ModuleName}DBContext dbContext, ICacheHelper cacheHelper) : {context.ModuleName}CacheRepositoryImpl<{domain.Name}>(dbContext, cacheHelper), I{domain.Name}Repository, IScopedDependency<I{domain.Name}Repository>");
            codeContent.AppendLine($"    {{");
            codeContent.AppendLine($"        /// <summary>");
            codeContent.AppendLine($"        /// 获得所有缓存名称");
            codeContent.AppendLine($"        /// </summary>");
            codeContent.AppendLine($"        protected override string GetAllCacheName() => \"All{domain.Name}\";");
        }
        else
        {
            codeContent.AppendLine($"    public partial class {domain.Name}RepositoryImpl({context.ModuleName}DBContext dbContext) : {context.ModuleName}RepositoryImpl<{domain.Name}>(dbContext), I{domain.Name}Repository, IScopedDependency<I{domain.Name}Repository>");
            codeContent.AppendLine($"    {{");
        }
        if (domain.IsIndexDomain && !domain.HasAttribute<EmptyIndexAttribute>())
        {
            PropertyModel? indexGroupPropertyModel = domain.GetIndexGroupProperty();
            codeContent.AppendLine($"        /// <summary>");
            codeContent.AppendLine($"        /// 获取最大位序");
            codeContent.AppendLine($"        /// </summary>");
            if (indexGroupPropertyModel is null)
            {
                codeContent.AppendLine($"        /// <returns></returns>");
                codeContent.AppendLine($"        public async Task<int> GetMaxIndexAsync()");
                codeContent.AppendLine($"        {{");
                codeContent.AppendLine($"            if (!await DBSet.AnyAsync()) return -1;");
                codeContent.AppendLine($"            int result = await DBSet.MaxAsync(m => m.Index);");
            }
            else
            {
                codeContent.AppendLine($"        /// <param name=\"{indexGroupPropertyModel.Name.ToLowerFirstLetter()}\"></param>");
                codeContent.AppendLine($"        /// <returns></returns>");
                codeContent.AppendLine($"        public async Task<int> GetMaxIndexAsync({indexGroupPropertyModel.PredefinedType} {indexGroupPropertyModel.Name.ToLowerFirstLetter()})");
                codeContent.AppendLine($"        {{");
                codeContent.AppendLine($"            if (!await DBSet.AnyAsync(m => m.{indexGroupPropertyModel.Name} == {indexGroupPropertyModel.Name.ToLowerFirstLetter()})) return -1;");
                codeContent.AppendLine($"            int result = await DBSet.Where(m => m.{indexGroupPropertyModel.Name} == {indexGroupPropertyModel.Name.ToLowerFirstLetter()}).MaxAsync(m => m.Index);");
            }
            codeContent.AppendLine($"            return result;");
            codeContent.AppendLine($"        }}");
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleRepositoryMGCPath, "Repositories", $"{domain.Name}RepositoryImpl.cs");
    }
}
