namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不生成 ListDTO 特性
/// 用于标记在代码生成过程中不生成 ListDTO（列表数据传输对象）相关代码的类或属性
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.DTOGeneratorCodePlug.GeneratorListDTOModelAsync"/> 中：
/// 不会生成该领域模型的 {DomainName}ListDTO 类</description>
/// </item>
/// </list>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.DTOGeneratorCodePlug.GeneratorListDTOModelAsync"/> 中：
/// 该属性不会被包含在 {DomainName}ListDTO 类中</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.DTOGeneratorCodePlug.GeneratorDTOModelAsync"/> 中：
/// 该属性会被包含在 {DomainName}DTO 类中（仅排除在 ListDTO 中，详情 DTO 仍包含）</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="NotDTOAttribute"/>：NotDTOAttribute 控制属性是否出现在所有 DTO 中，而 NotListDTOAttribute 仅控制是否出现在 ListDTO 中</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>标记大字段属性，列表查询时不需要返回（如文章内容、详细描述等）</description></item>
/// <item><description>标记详情属性，仅在查看详情时需要（列表展示不需要）</description></item>
/// <item><description>标记复杂对象或 JSON 字段，列表查询时不需要</description></item>
/// <item><description>优化列表查询性能，减少数据传输量</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：应用于类，完全不生成 ListDTO
/// [NotListDTO]
/// public class DetailOnlyEntity : BaseDomain
/// {
///     public string Name { get; set; }
///     public string Description { get; set; }
///     
///     // 该实体只在详情查询时使用，不需要列表 DTO
/// }
/// 
/// // 示例2：应用于属性，排除大字段
/// public class Article : BaseDomain
/// {
///     public string Title { get; set; }
///     public string Author { get; set; }
///     public DateTime PublishTime { get; set; }
///     
///     [NotListDTO]
///     public string Content { get; set; }  // 文章内容，列表不需要
///     
///     [NotListDTO]
///     public string HtmlContent { get; set; }  // HTML 内容，列表不需要
/// }
/// 
/// // 生成的 ListDTO（不包含 NotListDTO 属性）：
/// public partial class ArticleListDTO : IListDTO
/// {
///     public Guid ID { get; set; }
///     public DateTime CreateTime { get; set; }
///     public string Title { get; set; }
///     public string Author { get; set; }
///     public DateTime PublishTime { get; set; }
///     // Content 和 HtmlContent 不在 ListDTO 中
/// }
/// 
/// // 生成的 DTO（包含所有属性）：
/// public partial class ArticleDTO : ArticleListDTO, IDTO
/// {
///     public string Content { get; set; }  // 详情 DTO 包含内容
///     public string HtmlContent { get; set; }
/// }
/// 
/// // 示例3：排除详情属性
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
///     public string Category { get; set; }
///     
///     [NotListDTO]
///     public string DetailedDescription { get; set; }  // 详细描述，列表不需要
///     
///     [NotListDTO]
///     public string Specifications { get; set; }  // 规格参数，列表不需要
///     
///     [NotListDTO]
///     public string WarrantyInfo { get; set; }  // 保修信息，列表不需要
/// }
/// 
/// // 示例4：排除 JSON 字段
/// public class Configuration : BaseDomain
/// {
///     public string Name { get; set; }
///     public string Category { get; set; }
///     public bool IsActive { get; set; }
///     
///     [NotListDTO]
///     public string JsonData { get; set; }  // JSON 配置数据，列表不需要
///     
///     [NotListDTO]
///     public string ExtendedProperties { get; set; }  // 扩展属性，列表不需要
/// }
/// 
/// // 示例5：性能优化场景
/// public class Order : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public DateTime OrderTime { get; set; }
///     public string Status { get; set; }
///     
///     [NotListDTO]
///     public string DeliveryAddress { get; set; }  // 配送地址，列表不需要
///     
///     [NotListDTO]
///     public string Remark { get; set; }  // 备注，列表不需要
///     
///     [NotListDTO]
///     public string InvoiceInfo { get; set; }  // 发票信息，列表不需要
/// }
/// 
/// // 对比：使用 NotDTO（完全不出现在任何 DTO 中）
/// public class User : BaseDomain
/// {
///     public string Username { get; set; }
///     
///     [NotDTO]
///     public string PasswordHash { get; set; }  // 不出现在 ListDTO 和 DTO 中
///     
///     [NotListDTO]
///     public string Bio { get; set; }  // 不出现在 ListDTO 中，但出现在 DTO 中
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NotListDTOAttribute : Attribute { }
