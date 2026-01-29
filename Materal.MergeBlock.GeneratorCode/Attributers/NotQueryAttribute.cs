namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不生成查询特性
/// 用于标记在代码生成过程中不生成查询相关代码的类或属性
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorQueryRequestModelAsync"/> 中：
/// 不会生成该领域模型的 Query{DomainName}RequestModel 类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorQueryModelAsync"/> 中：
/// 不会生成该领域模型的 Query{DomainName}Model 类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorTreeQueryRequestModelAsync"/> 中：
/// 不会生成该领域模型的 Query{DomainName}TreeListRequestModel 类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorTreeQueryModelAsync"/> 中：
/// 不会生成该领域模型的 Query{DomainName}TreeListModel 类</description>
/// </item>
/// </list>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorQueryRequestModelAsync"/> 中：
/// 该属性不会被包含在 Query{DomainName}RequestModel 类中作为查询条件</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorQueryModelAsync"/> 中：
/// 该属性不会被包含在 Query{DomainName}Model 类中作为查询条件</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>标记只写不查的领域模型（如日志表、审计表等）</description></item>
/// <item><description>标记不需要作为查询条件的属性（如计算属性、大字段等）</description></item>
/// <item><description>标记敏感属性，不允许作为查询条件</description></item>
/// <item><description>标记二进制数据、JSON 字段等不适合作为查询条件的属性</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：应用于类，完全不生成查询模型
/// [NotQuery]
/// public class AuditLog : BaseDomain
/// {
///     public string Action { get; set; }
///     public string UserID { get; set; }
///     public DateTime Timestamp { get; set; }
///     public string Details { get; set; }
///     
///     // 审计日志只写不查，通过专门的日志查询系统访问
/// }
/// 
/// // 示例2：应用于属性，排除不适合查询的字段
/// public class Article : BaseDomain
/// {
///     [Contains]
///     public string Title { get; set; }  // 可以作为查询条件
///     
///     public string Author { get; set; }  // 可以作为查询条件
///     
///     [NotQuery]
///     public string Content { get; set; }  // 文章内容不作为查询条件
///     
///     [NotQuery]
///     public string HtmlContent { get; set; }  // HTML 内容不作为查询条件
///     
///     public DateTime PublishTime { get; set; }  // 可以作为查询条件
/// }
/// 
/// // 生成的查询请求模型（不包含 NotQuery 属性）：
/// public partial class QueryArticleRequestModel : PageRequestModel, IQueryRequestModel
/// {
///     public string? Title { get; set; }
///     public string? Author { get; set; }
///     public DateTime? PublishTime { get; set; }
///     // Content 和 HtmlContent 不在查询模型中
///     
///     public List&lt;Guid&gt;? IDs { get; set; }
///     public DateTime? MinCreateTime { get; set; }
///     public DateTime? MaxCreateTime { get; set; }
/// }
/// 
/// // 示例3：排除敏感属性
/// public class User : BaseDomain
/// {
///     public string Username { get; set; }  // 可以作为查询条件
///     public string Email { get; set; }  // 可以作为查询条件
///     
///     [NotQuery]
///     [NotDTO]
///     public string PasswordHash { get; set; }  // 密码哈希不允许查询
///     
///     [NotQuery]
///     public string SecurityStamp { get; set; }  // 安全戳不允许查询
///     
///     public bool IsActive { get; set; }  // 可以作为查询条件
/// }
/// 
/// // 示例4：排除二进制和 JSON 字段
/// public class Attachment : BaseDomain
/// {
///     public string FileName { get; set; }  // 可以作为查询条件
///     public string FileType { get; set; }  // 可以作为查询条件
///     public long FileSize { get; set; }  // 可以作为查询条件
///     
///     [NotQuery]
///     [NotDTO]
///     public byte[] FileContent { get; set; }  // 二进制内容不作为查询条件
///     
///     [NotQuery]
///     public string Metadata { get; set; }  // JSON 元数据不作为查询条件
/// }
/// 
/// // 示例5：排除计算属性
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }  // 可以作为查询条件
///     public decimal Price { get; set; }  // 可以作为查询条件
///     public string Category { get; set; }  // 可以作为查询条件
///     
///     [NotQuery]
///     [NotMapped]
///     public decimal DiscountPrice => Price * 0.8m;  // 计算属性不作为查询条件
///     
///     [NotQuery]
///     public string FullDescription { get; set; }  // 完整描述不作为查询条件
/// }
/// 
/// // 对比：标准属性（会生成查询条件）
/// public class Order : BaseDomain
/// {
///     [Contains]
///     public string OrderNo { get; set; }  // 生成模糊查询条件
///     
///     [Between]
///     public decimal TotalAmount { get; set; }  // 生成范围查询条件（MinTotalAmount, MaxTotalAmount）
///     
///     [Equal]
///     public string Status { get; set; }  // 生成精确查询条件
/// }
/// 
/// // 生成的查询模型：
/// public partial class QueryOrderModel : PageRequestModel, IQueryServiceModel
/// {
///     [Contains("OrderNo")]
///     public string? OrderNo { get; set; }
///     
///     [GreaterThanOrEqual("TotalAmount")]
///     public decimal? MinTotalAmount { get; set; }
///     
///     [LessThanOrEqual("TotalAmount")]
///     public decimal? MaxTotalAmount { get; set; }
///     
///     [Equal("Status")]
///     public string? Status { get; set; }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class NotQueryAttribute : Attribute { }
