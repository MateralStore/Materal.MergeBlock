namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 视图特性
/// 用于标记领域模型对应数据库视图，而不是数据库表
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorEntityConfigCodeAsync"/> 中：
/// 生成的实体配置会调用 ToView() 方法而不是 ToTable() 方法，将实体映射到数据库视图</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="NotInDBContextAttribute"/>：通常与 NotInDBContextAttribute 配合使用，视图实体不需要在 DbContext 中注册 DbSet</description>
/// </item>
/// <item>
/// <description><see cref="NotRepositoryAttribute"/>：可以与 NotRepositoryAttribute 配合使用，视图通常只读，不需要标准仓储</description>
/// </item>
/// <item>
/// <description><see cref="QueryViewAttribute"/>：可以作为 QueryViewAttribute 的目标实体，用于查询优化</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>映射数据库视图，用于复杂查询和数据展示</description></item>
/// <item><description>只读数据实体，不需要增删改操作</description></item>
/// <item><description>聚合多个表的数据，提供统一的查询接口</description></item>
/// <item><description>性能优化，使用预计算的视图数据</description></item>
/// <item><description>实现读写分离，读操作使用视图</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：订单统计视图
/// [View]
/// [NotInDBContext]
/// [NotRepository]
/// public class OrderStatisticsView : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public string CustomerName { get; set; }  // 来自 Customer 表
///     public string StatusText { get; set; }  // 状态描述
///     public int ItemCount { get; set; }  // 订单项数量
///     public DateTime OrderTime { get; set; }
/// }
/// 
/// // 生成的实体配置：
/// public class OrderStatisticsViewConfigBase : BaseEntityConfig&lt;OrderStatisticsView&gt;
/// {
///     public override void Configure(EntityTypeBuilder&lt;OrderStatisticsView&gt; builder)
///     {
///         builder = BaseConfigure(builder);
///         builder.ToView("V_OrderStatistics");  // 映射到视图而不是表
///         builder.Property(e => e.OrderNo)
///             .IsRequired()
///             .HasComment("订单号");
///         // ... 其他属性配置
///     }
/// }
/// 
/// // 在服务中使用：
/// public class OrderService
/// {
///     private readonly SalesDBContext _dbContext;
///     
///     public async Task&lt;List&lt;OrderStatisticsView&gt;&gt; GetStatisticsAsync()
///     {
///         return await _dbContext.Set&lt;OrderStatisticsView&gt;()
///             .Where(m => m.OrderTime >= DateTime.Today)
///             .ToListAsync();
///     }
/// }
/// 
/// // 示例2：产品详情视图（包含关联数据）
/// [View]
/// [NotInDBContext]
/// public class ProductDetailView : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
///     public string CategoryName { get; set; }  // 分类名称
///     public string BrandName { get; set; }  // 品牌名称
///     public int StockQuantity { get; set; }
///     public int SalesCount { get; set; }  // 销量统计
///     public decimal AverageRating { get; set; }  // 平均评分
/// }
/// 
/// // 对应的数据库视图 SQL：
/// /*
/// CREATE VIEW V_ProductDetail AS
/// SELECT 
///     p.ID,
///     p.Name,
///     p.Price,
///     c.Name AS CategoryName,
///     b.Name AS BrandName,
///     p.StockQuantity,
///     ISNULL(SUM(oi.Quantity), 0) AS SalesCount,
///     ISNULL(AVG(r.Rating), 0) AS AverageRating
/// FROM Product p
/// LEFT JOIN Category c ON p.CategoryID = c.ID
/// LEFT JOIN Brand b ON p.BrandID = b.ID
/// LEFT JOIN OrderItem oi ON p.ID = oi.ProductID
/// LEFT JOIN Review r ON p.ID = r.ProductID
/// GROUP BY p.ID, p.Name, p.Price, c.Name, b.Name, p.StockQuantity
/// */
/// 
/// // 示例3：配合 QueryView 使用
/// [View]
/// [NotInDBContext]
/// public class OrderQueryView : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public string CustomerName { get; set; }
///     public string StatusText { get; set; }
///     public DateTime OrderTime { get; set; }
/// }
/// 
/// // 领域模型使用视图进行查询
/// [QueryView(nameof(OrderQueryView))]
/// public class Order : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public Guid CustomerID { get; set; }
///     public int Status { get; set; }
///     public DateTime OrderTime { get; set; }
/// }
/// 
/// // 示例4：只读报表视图
/// [View]
/// [NotInDBContext]
/// [NotRepository]
/// [NotService]
/// [NotController]
/// public class SalesReportView : BaseDomain
/// {
///     public string Period { get; set; }  // 时间段
///     public decimal TotalRevenue { get; set; }  // 总收入
///     public int OrderCount { get; set; }  // 订单数
///     public decimal AverageOrderValue { get; set; }  // 平均订单金额
///     public int CustomerCount { get; set; }  // 客户数
/// }
/// 
/// // 通过专门的报表服务访问
/// 
/// // 对比：标准表实体（不使用 View）
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
/// 
/// // 生成的实体配置（映射到表）：
/// public class ProductConfigBase : BaseEntityConfig&lt;Product&gt;
/// {
///     public override void Configure(EntityTypeBuilder&lt;Product&gt; builder)
///     {
///         builder = BaseConfigure(builder);
///         builder.ToTable(m => m.HasComment("产品"));  // 映射到表
///         // ... 属性配置
///     }
/// }
/// 
/// // 注意事项：
/// // 1. 视图实体通常是只读的，不应该进行增删改操作
/// // 2. 数据库视图需要预先创建，实体配置只是映射关系
/// // 3. 通常配合 NotInDBContext、NotRepository 等特性使用
/// // 4. 视图名称默认为 "{EntityName}View"，可以在 partial class 中自定义
/// // 5. 适用于复杂查询、数据聚合、性能优化等场景
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ViewAttribute() : Attribute { }
