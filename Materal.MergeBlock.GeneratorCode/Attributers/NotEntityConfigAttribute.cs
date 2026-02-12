namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不生成实体配置特性
/// 用于标记在代码生成过程中不生成 EF Core 实体配置相关代码的类或属性
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorEntityConfigCodeAsync"/> 中：
/// 不会生成该领域模型的 {DomainName}Config 和 {DomainName}ConfigBase 类</description>
/// </item>
/// </list>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorEntityConfigCodeAsync"/> 中：
/// 该属性不会在实体配置类中生成对应的 Fluent API 配置代码（如 HasComment、IsRequired、HasMaxLength 等）</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>标记不需要映射到数据库的领域模型（如纯内存对象、视图模型等）</description></item>
/// <item><description>标记计算属性或导航属性，这些属性不需要数据库配置</description></item>
/// <item><description>标记使用 Data Annotations 特性配置的属性，不需要 Fluent API 重复配置</description></item>
/// <item><description>标记需要手动配置的属性，避免自动生成的配置覆盖</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：应用于类，完全不生成实体配置
/// [NotEntityConfig]
/// public class ViewModel : BaseDomain
/// {
///     public string DisplayName { get; set; }
///     public int CalculatedValue { get; set; }
///     
///     // 纯内存对象，不映射到数据库
/// }
/// 
/// // 示例2：应用于属性，排除计算属性
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
///     public decimal Cost { get; set; }
///     
///     [NotEntityConfig]
///     [NotMapped]  // EF Core 特性，标记不映射到数据库
///     public decimal Profit => Price - Cost;  // 计算属性，不需要配置
/// }
/// 
/// // 生成的实体配置（不包含 Profit 属性）：
/// public class ProductConfigBase : BaseEntityConfig&lt;Product&gt;
/// {
///     public override void Configure(EntityTypeBuilder&lt;Product&gt; builder)
///     {
///         builder = BaseConfigure(builder);
///         builder.ToTable(m => m.HasComment("产品"));
///         builder.Property(e => e.Name)
///             .IsRequired()
///             .HasComment("名称");
///         builder.Property(e => e.Price)
///             .IsRequired()
///             .HasComment("价格");
///         builder.Property(e => e.Cost)
///             .IsRequired()
///             .HasComment("成本");
///         // Profit 属性不在配置中
///     }
/// }
/// 
/// // 示例3：排除导航属性
/// public class Order : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public Guid CustomerID { get; set; }
///     
///     [NotEntityConfig]
///     public Customer Customer { get; set; }  // 导航属性，通过其他方式配置关系
///     
///     [NotEntityConfig]
///     public List&lt;OrderItem&gt; OrderItems { get; set; }  // 集合导航属性
/// }
/// 
/// // 示例4：使用 Data Annotations 配置的属性
/// public class Article : BaseDomain
/// {
///     [MaxLength(200)]
///     [NotEntityConfig]  // 已使用 Data Annotations，不需要 Fluent API
///     public string Title { get; set; }
///     
///     [Column(TypeName = "nvarchar(max)")]
///     [NotEntityConfig]
///     public string Content { get; set; }
///     
///     public DateTime PublishTime { get; set; }  // 这个属性会生成配置
/// }
/// 
/// // 生成的实体配置：
/// public class ArticleConfigBase : BaseEntityConfig&lt;Article&gt;
/// {
///     public override void Configure(EntityTypeBuilder&lt;Article&gt; builder)
///     {
///         builder = BaseConfigure(builder);
///         builder.ToTable(m => m.HasComment("文章"));
///         // Title 和 Content 不在配置中，使用 Data Annotations
///         builder.Property(e => e.PublishTime)
///             .IsRequired()
///             .HasComment("发布时间");
///     }
/// }
/// 
/// // 示例5：需要手动配置的复杂属性
/// public class User : BaseDomain
/// {
///     public string Username { get; set; }
///     
///     [NotEntityConfig]
///     public string Email { get; set; }  // 需要在 partial class 中手动配置唯一索引
/// }
/// 
/// // 在 partial class 中手动配置：
/// public partial class UserConfig : UserConfigBase
/// {
///     public override void Configure(EntityTypeBuilder&lt;User&gt; builder)
///     {
///         base.Configure(builder);
///         
///         // 手动配置 Email 的唯一索引
///         builder.Property(e => e.Email)
///             .IsRequired()
///             .HasMaxLength(100);
///         builder.HasIndex(e => e.Email)
///             .IsUnique();
///     }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class NotEntityConfigAttribute : Attribute { }
