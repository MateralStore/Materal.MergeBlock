namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 查询视图特性
/// 用于标记领域模型在查询时使用指定的视图或目标实体，而不是自身
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.DTOGeneratorCodePlug.GeneratorListDTOModelAsync"/> 中：
/// 生成 DTO 时会使用 TargetName 指定的目标实体的属性，而不是当前领域模型的属性</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.DTOGeneratorCodePlug.GeneratorDTOModelAsync"/> 中：
/// 生成 DTO 时会使用 TargetName 指定的目标实体的属性</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorQueryRequestModelAsync"/> 中：
/// 生成查询请求模型时会使用 TargetName 指定的目标实体的属性</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorQueryModelAsync"/> 中：
/// 生成查询服务模型时会使用 TargetName 指定的目标实体的属性</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>领域模型对应数据库视图，查询时使用视图实体的属性</description></item>
/// <item><description>领域模型有对应的查询专用实体，查询条件使用查询实体的属性</description></item>
/// <item><description>实现读写分离，写操作使用领域模型，读操作使用视图模型</description></item>
/// <item><description>优化查询性能，使用包含冗余字段的视图实体</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：使用数据库视图进行查询
/// // 定义视图实体
/// [View]
/// [NotInDBContext]
/// public class OrderView : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public string CustomerName { get; set; }  // 来自关联表的冗余字段
///     public string StatusText { get; set; }  // 状态文本
///     public DateTime OrderTime { get; set; }
/// }
/// 
/// // 定义领域模型，查询时使用视图
/// [QueryView(nameof(OrderView))]
/// public class Order : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public Guid CustomerID { get; set; }  // 外键
///     public int Status { get; set; }  // 状态枚举值
///     public DateTime OrderTime { get; set; }
/// }
/// 
/// // 生成的查询请求模型（使用 OrderView 的属性）：
/// public partial class QueryOrderRequestModel : PageRequestModel, IQueryRequestModel
/// {
///     public string? OrderNo { get; set; }
///     public decimal? TotalAmount { get; set; }
///     public string? CustomerName { get; set; }  // 可以按客户名称查询
///     public string? StatusText { get; set; }  // 可以按状态文本查询
///     public DateTime? OrderTime { get; set; }
///     
///     public List&lt;Guid&gt;? IDs { get; set; }
///     public DateTime? MinCreateTime { get; set; }
///     public DateTime? MaxCreateTime { get; set; }
/// }
/// 
/// // 生成的 DTO（使用 OrderView 的属性）：
/// public partial class OrderListDTO : IListDTO
/// {
///     public Guid ID { get; set; }
///     public DateTime CreateTime { get; set; }
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public string CustomerName { get; set; }  // 包含客户名称
///     public string StatusText { get; set; }  // 包含状态文本
///     public DateTime OrderTime { get; set; }
/// }
/// 
/// // 示例2：读写分离场景
/// // 查询专用实体（包含冗余字段）
/// public class ProductQueryModel : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
///     public string CategoryName { get; set; }  // 分类名称（冗余）
///     public string BrandName { get; set; }  // 品牌名称（冗余）
///     public int StockQuantity { get; set; }
/// }
/// 
/// // 领域模型（写操作）
/// [QueryView(nameof(ProductQueryModel))]
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
///     public Guid CategoryID { get; set; }  // 外键
///     public Guid BrandID { get; set; }  // 外键
///     public int StockQuantity { get; set; }
/// }
/// 
/// // 查询时使用 ProductQueryModel 的属性，可以直接按分类名称、品牌名称查询
/// 
/// // 示例3：性能优化场景
/// // 优化后的查询视图
/// public class ArticleQueryView : BaseDomain
/// {
///     [Contains]
///     public string Title { get; set; }
///     public string Author { get; set; }
///     public string CategoryName { get; set; }  // 分类名称
///     public int ViewCount { get; set; }
///     public int CommentCount { get; set; }  // 评论数（冗余）
///     public DateTime PublishTime { get; set; }
/// }
/// 
/// // 领域模型
/// [QueryView(nameof(ArticleQueryView))]
/// public class Article : BaseDomain
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
///     public string Author { get; set; }
///     public Guid CategoryID { get; set; }
///     public int ViewCount { get; set; }
///     public DateTime PublishTime { get; set; }
/// }
/// 
/// // 查询时使用 ArticleQueryView，包含预计算的评论数和分类名称，避免关联查询
/// 
/// // 注意事项：
/// // 1. TargetName 必须是有效的实体类名称
/// // 2. 目标实体应该包含查询所需的所有属性
/// // 3. 通常配合 [View]、[NotInDBContext] 等特性使用
/// // 4. 适用于读写分离、性能优化等场景
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class QueryViewAttribute(string targetName) : Attribute
{
    /// <summary>
    /// 目标名称
    /// 指定查询时使用的目标实体类名称，该实体的属性将用于生成查询模型和 DTO
    /// </summary>
    public string TargetName { get; set; } = targetName;
}
