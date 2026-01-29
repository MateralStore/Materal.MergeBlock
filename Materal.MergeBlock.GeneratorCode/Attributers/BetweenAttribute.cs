namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 范围查询特性
/// 用于标记属性在查询时生成范围查询条件（Between），即生成最小值和最大值两个查询参数
/// </summary>
/// <remarks>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorQueryRequestModelAsync"/> 中：
/// 会为该属性生成两个查询参数：Min{PropertyName} 和 Max{PropertyName}</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorQueryModelAsync"/> 中：
/// 会为该属性生成两个查询参数，并分别添加 [GreaterThanOrEqual] 和 [LessThanOrEqual] 特性</description>
/// </item>
/// </list>
/// <para><b>适用的数据类型：</b></para>
/// <list type="bullet">
/// <item><description>数值类型：int, long, decimal, double, float 等</description></item>
/// <item><description>日期时间类型：DateTime, DateTimeOffset, DateOnly, TimeOnly 等</description></item>
/// <item><description>其他可比较类型：实现了 IComparable 接口的类型</description></item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>价格范围查询（最低价格到最高价格）</description></item>
/// <item><description>日期范围查询（开始日期到结束日期）</description></item>
/// <item><description>数量范围查询（最小数量到最大数量）</description></item>
/// <item><description>年龄范围查询（最小年龄到最大年龄）</description></item>
/// <item><description>评分范围查询（最低评分到最高评分）</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：价格范围查询
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     
///     [Between]
///     public decimal Price { get; set; }
///     
///     public int Stock { get; set; }
/// }
/// 
/// // 生成的查询请求模型：
/// public partial class QueryProductRequestModel : PageRequestModel, IQueryRequestModel
/// {
///     public string? Name { get; set; }
///     
///     public decimal? MinPrice { get; set; }  // 最低价格
///     public decimal? MaxPrice { get; set; }  // 最高价格
///     
///     public int? Stock { get; set; }
///     
///     public List&lt;Guid&gt;? IDs { get; set; }
///     public DateTime? MinCreateTime { get; set; }
///     public DateTime? MaxCreateTime { get; set; }
/// }
/// 
/// // 生成的查询服务模型：
/// public partial class QueryProductModel : PageRequestModel, IQueryServiceModel
/// {
///     [Equal("Name")]
///     public string? Name { get; set; }
///     
///     [GreaterThanOrEqual("Price")]
///     public decimal? MinPrice { get; set; }
///     
///     [LessThanOrEqual("Price")]
///     public decimal? MaxPrice { get; set; }
///     
///     [Equal("Stock")]
///     public int? Stock { get; set; }
/// }
/// 
/// // 使用示例：
/// var request = new QueryProductRequestModel
/// {
///     MinPrice = 100,   // 价格大于等于 100
///     MaxPrice = 500,   // 价格小于等于 500
///     PageIndex = 1,
///     PageSize = 20
/// };
/// var result = await productService.GetListAsync(request);
/// 
/// // 示例2：日期范围查询
/// public class Order : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     
///     [Between]
///     public DateTime OrderTime { get; set; }
///     
///     [Between]
///     public decimal TotalAmount { get; set; }
/// }
/// 
/// // 生成的查询请求模型：
/// public partial class QueryOrderRequestModel : PageRequestModel, IQueryRequestModel
/// {
///     public string? OrderNo { get; set; }
///     
///     public DateTime? MinOrderTime { get; set; }  // 开始时间
///     public DateTime? MaxOrderTime { get; set; }  // 结束时间
///     
///     public decimal? MinTotalAmount { get; set; }  // 最小金额
///     public decimal? MaxTotalAmount { get; set; }  // 最大金额
/// }
/// 
/// // 使用示例：
/// var request = new QueryOrderRequestModel
/// {
///     MinOrderTime = new DateTime(2024, 1, 1),
///     MaxOrderTime = new DateTime(2024, 12, 31),
///     MinTotalAmount = 1000,
///     MaxTotalAmount = 10000
/// };
/// 
/// // 示例3：评分范围查询
/// public class Review : BaseDomain
/// {
///     public string Content { get; set; }
///     
///     [Between]
///     public int Rating { get; set; }  // 评分 1-5
///     
///     public Guid ProductID { get; set; }
/// }
/// 
/// // 查询 4-5 星的评价：
/// var request = new QueryReviewRequestModel
/// {
///     MinRating = 4,
///     MaxRating = 5
/// };
/// 
/// // 示例4：年龄范围查询
/// public class User : BaseDomain
/// {
///     public string Username { get; set; }
///     
///     [Between]
///     public int Age { get; set; }
///     
///     public string Email { get; set; }
/// }
/// 
/// // 查询 25-35 岁的用户：
/// var request = new QueryUserRequestModel
/// {
///     MinAge = 25,
///     MaxAge = 35
/// };
/// 
/// // 对比：不使用 Between（精确查询）
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     
///     [Equal]
///     public decimal Price { get; set; }  // 精确匹配价格
/// }
/// 
/// // 生成的查询模型（只有一个 Price 参数）：
/// public partial class QueryProductRequestModel : PageRequestModel, IQueryRequestModel
/// {
///     public string? Name { get; set; }
///     public decimal? Price { get; set; }  // 精确查询
/// }
/// 
/// // 注意事项：
/// // 1. Between 适用于可比较的数值和日期类型
/// // 2. 生成的查询参数都是可空类型，可以只指定最小值或最大值
/// // 3. 如果只指定 MinValue，查询条件为 >= MinValue
/// // 4. 如果只指定 MaxValue，查询条件为 &lt;= MaxValue
/// // 5. 如果同时指定，查询条件为 MinValue &lt;= Value &lt;= MaxValue
/// // 6. 常用于价格、日期、数量等需要范围筛选的场景
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BetweenAttribute : Attribute { }
