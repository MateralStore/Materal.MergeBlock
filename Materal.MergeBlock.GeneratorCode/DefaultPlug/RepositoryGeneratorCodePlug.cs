using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 仓储代码生成插件
/// </summary>
public class RepositoryGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <summary>
    /// 仓储视图模型（用于模板渲染）
    /// </summary>
    private class RepositoryViewModel
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
        /// 是否为视图
        /// </summary>
        public bool IsView { get; set; }

        /// <summary>
        /// 属性列表
        /// </summary>
        public List<PropertyViewModel> Properties { get; set; } = [];

        /// <summary>
        /// 是否有缓存特性
        /// </summary>
        public bool HasCacheAttribute { get; set; }
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
        /// 是否可空
        /// </summary>
        public bool CanNull { get; set; }

        /// <summary>
        /// 列类型（如果有 ColumnType 特性）
        /// </summary>
        public string? ColumnType { get; set; }

        /// <summary>
        /// 最大长度（如果有 StringLength 特性）
        /// </summary>
        public string? MaxLength { get; set; }

        /// <summary>
        /// 预定义类型
        /// </summary>
        public string PredefinedType { get; set; } = string.Empty;

        /// <summary>
        /// 首字母小写的名称
        /// </summary>
        public string NameToLowerFirstLetter => Name.Length > 0 ? char.ToLower(Name[0]) + Name[1..] : Name;
    }

    private static readonly string _entityConfigTemplate = @"
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {{context.ProjectName}}.{{context.ModuleName}}.Repository.EntityConfigs;
/// <summary>
/// {{domain.Annotation}}配置基类
/// </summary>
public class {{domain.Name}}ConfigBase : BaseEntityConfig<{{domain.Name}}>
{
    /// <summary>
    /// 配置
    /// </summary>
    public override void Configure(EntityTypeBuilder<{{domain.Name}}> builder)
    {
        builder = BaseConfigure(builder);
{{- if domain.IsView}}
        builder.ToView(""{{domain.Name}}"");
{{- else}}
        builder.ToTable(m => m.HasComment(""{{domain.Annotation}}""));
{{- end}}
{{- for property in domain.Properties}}
        builder.Property(e => e.{{property.Name}})
{{- if !property.CanNull}}
            .IsRequired()
{{- end}}
            .HasComment(""{{property.Annotation}}"")
{{- if property.ColumnType}}
            .HasColumnType({{property.ColumnType}})
{{- end}}
{{- if property.MaxLength}}
            .HasMaxLength({{property.MaxLength}})
{{- end}};
{{- end}}
    }
}
/// <summary>
/// {{domain.Annotation}}配置类
/// </summary>
public partial class {{domain.Name}}Config : {{domain.Name}}ConfigBase { }
";

    private static readonly string _dbContextTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Repository;
/// <summary>
/// {{context.ModuleName}}数据库上下文
/// </summary>
public sealed partial class {{context.ModuleName}}DBContext(DbContextOptions<{{context.ModuleName}}DBContext> options) : DbContext(options)
{
{{- for domain in context.Domains}}
    /// <summary>
    /// {{domain.Annotation}}
    /// </summary>
    public DbSet<{{domain.Name}}>? {{domain.Name}} { get; set; }
{{- end}}
    /// <summary>
    /// 配置模型
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
}
";

    private static readonly string _iRepositoryTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Repositories;
/// <summary>
/// {{domain.Annotation}}仓储
/// </summary>
{{if domain.HasCacheAttribute}}
public partial interface I{{domain.Name}}Repository : I{{context.ModuleName}}CacheRepository<{{domain.Name}}>
{{else}}
public partial interface I{{domain.Name}}Repository : I{{context.ModuleName}}Repository<{{domain.Name}}>
{{end}}
{
}
";

    private static readonly string _repositoryImplTemplate = @"
namespace {{context.ProjectName}}.{{context.ModuleName}}.Repository.Repositories;
/// <summary>
/// {{domain.Annotation}}仓储
/// </summary>
{{if domain.HasCacheAttribute}}
public partial class {{domain.Name}}RepositoryImpl({{context.ModuleName}}DBContext dbContext, ICacheHelper cacheHelper) : {{context.ModuleName}}CacheRepositoryImpl<{{domain.Name}}>(dbContext, cacheHelper), I{{domain.Name}}Repository, IScopedDependency<I{{domain.Name}}Repository>
{
    /// <summary>
    /// 获得所有缓存名称
    /// </summary>
    protected override string GetAllCacheName() => ""All{{domain.Name}}"";
{{else}}
public partial class {{domain.Name}}RepositoryImpl({{context.ModuleName}}DBContext dbContext) : {{context.ModuleName}}RepositoryImpl<{{domain.Name}}>(dbContext), I{{domain.Name}}Repository, IScopedDependency<I{{domain.Name}}Repository>
{
{{end}}
}
";

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

        Template template = Template.Parse(_entityConfigTemplate);
        string codeContent = RenderTemplate(template, context, domain, forEntityConfig: true);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleRepositoryMGCPath, "EntityConfigs", $"{domain.Name}Config.cs");
    }

    /// <summary>
    /// 创建数据库上下文代码
    /// </summary>
    /// <param name="context"></param>
    private async Task GeneratorDBContextCodeAsync(GeneratorCodeContext context)
    {
        Template template = Template.Parse(_dbContextTemplate);
        string codeContent = RenderTemplateForDBContext(template, context);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleRepositoryMGCPath, $"{context.ModuleName}DBContext.cs");
    }

    /// <summary>
    /// 创建仓储接口代码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorIRepositoryCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotRepositoryAttribute>()) return;

        Template template = Template.Parse(_iRepositoryTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleAbstractionsMGCPath, "Repositories", $"I{domain.Name}Repository.cs");
    }

    /// <summary>
    /// 创建仓储实现代码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    private async Task GeneratorRepositoryImplCodeAsync(GeneratorCodeContext context, DomainModel domain)
    {
        if (domain.HasAttribute<NotRepositoryAttribute>()) return;

        Template template = Template.Parse(_repositoryImplTemplate);
        string codeContent = RenderTemplate(template, context, domain);

        context.SaveAs(new StringBuilder(codeContent), context.ModuleRepositoryMGCPath, "Repositories", $"{domain.Name}RepositoryImpl.cs");
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    /// <param name="template"></param>
    /// <param name="context"></param>
    /// <param name="domain"></param>
    /// <param name="forEntityConfig">是否为实体配置模板</param>
    /// <returns></returns>
    private static string RenderTemplate(Template template, GeneratorCodeContext context, DomainModel domain, bool forEntityConfig = false)
    {
        var repositoryViewModel = new RepositoryViewModel
        {
            Name = domain.Name,
            Annotation = domain.Annotation,
            IsView = domain.IsView,
            HasCacheAttribute = domain.HasAttribute<CacheAttribute>()
        };

        if (forEntityConfig)
        {
            repositoryViewModel.Properties = [.. domain.Properties
                .Where(p => !p.HasAttribute<NotEntityConfigAttribute>())
                .Select(p => new PropertyViewModel
                {
                    Name = p.Name,
                    Annotation = p.Annotation,
                    CanNull = p.CanNull,
                    ColumnType = p.GetAttribute<ColumnTypeAttribute>()?.GetAttributeArgument()?.Value,
                    MaxLength = p.GetAttribute<StringLengthAttribute>()?.GetAttributeArgument()?.Value
                })];
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

    /// <summary>
    /// 渲染数据库上下文模板
    /// </summary>
    /// <param name="template"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private static string RenderTemplateForDBContext(Template template, GeneratorCodeContext context)
    {
        var domains = context.Domains
            .Where(d => !d.HasAttribute<NotInDBContextAttribute>())
            .Select(d => new RepositoryViewModel
            {
                Name = d.Name,
                Annotation = d.Annotation
            })
            .ToList();

        ScriptObject scriptObject = new()
        {
            { "context", context },
            { "context.Domains", domains }
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
