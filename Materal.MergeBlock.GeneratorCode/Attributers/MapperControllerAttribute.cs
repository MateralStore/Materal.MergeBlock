namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 映射控制器特性
/// 用于标记服务层方法自动生成对应的控制器方法，实现服务方法到控制器API的自动映射
/// </summary>
/// <remarks>
/// <para><b>应用于方法时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerMapperGeneratorCodePlug.GeneratorIControllerMapperCodeAsync"/> 中：
/// 会在控制器接口中生成对应的方法签名，包含指定的 HTTP 方法特性（HttpGet/HttpPost/HttpPut/HttpDelete/HttpPatch）</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerMapperGeneratorCodePlug.GeneratorControllerMapperCodeAsync"/> 中：
/// 会在控制器实现中生成对应的方法实现，自动处理参数映射、服务调用和结果包装</description>
/// </item>
/// </list>
/// <para><b>自动生成的控制器方法特点：</b></para>
/// <list type="bullet">
/// <item><description>自动添加对应的 HTTP 方法特性（[HttpGet]、[HttpPost] 等）</description></item>
/// <item><description>如果 IsAllowAnonymous = true，会添加 [AllowAnonymous] 特性</description></item>
/// <item><description>自动处理 RequestModel 到 ServiceModel 的映射</description></item>
/// <item><description>自动调用 BindLoginUserID() 绑定登录用户ID</description></item>
/// <item><description>自动包装返回结果为 ResultModel 或 ResultModel&lt;T&gt;</description></item>
/// <item><description>支持分页返回类型的特殊处理（返回 CollectionResultModel）</description></item>
/// </list>
/// <para><b>参数映射规则：</b></para>
/// <list type="bullet">
/// <item><description>简单类型参数（Guid、int、string 等）：直接传递，不需要映射</description></item>
/// <item><description>复杂类型参数（自定义类）：自动生成 Mapper.Map 映射代码</description></item>
/// <item><description>参数名称会根据需要调整（如 ServiceModel 参数映射为 RequestModel）</description></item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>服务层有自定义业务方法，需要暴露为 API 接口</description></item>
/// <item><description>不符合标准 CRUD 模式的特殊业务操作</description></item>
/// <item><description>需要快速将服务方法转换为 RESTful API</description></item>
/// <item><description>减少手动编写控制器方法的重复代码</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 在服务接口中定义方法并标记 MapperController
/// public partial interface IOrderService : IBaseService&lt;...&gt;
/// {
///     /// &lt;summary&gt;
///     /// 取消订单
///     /// &lt;/summary&gt;
///     [MapperController(MapperType.Put)]
///     Task CancelOrderAsync(CancelOrderModel model);
///     
///     /// &lt;summary&gt;
///     /// 获取订单统计
///     /// &lt;/summary&gt;
///     [MapperController(MapperType.Get, IsAllowAnonymous = true)]
///     Task&lt;OrderStatisticsDTO&gt; GetOrderStatisticsAsync(DateTime startDate, DateTime endDate);
///     
///     /// &lt;summary&gt;
///     /// 批量发货
///     /// &lt;/summary&gt;
///     [MapperController(MapperType.Post)]
///     Task BatchShipAsync(BatchShipModel model);
/// }
/// 
/// // 自动生成的控制器接口：
/// public partial interface IOrderController : IMergeBlockController
/// {
///     /// &lt;summary&gt;
///     /// 取消订单
///     /// &lt;/summary&gt;
///     [HttpPut]
///     Task&lt;ResultModel&gt; CancelOrderAsync(CancelOrderRequestModel model);
///     
///     /// &lt;summary&gt;
///     /// 获取订单统计
///     /// &lt;/summary&gt;
///     [HttpGet, AllowAnonymous]
///     Task&lt;ResultModel&lt;OrderStatisticsDTO&gt;&gt; GetOrderStatisticsAsync(DateTime startDate, DateTime endDate);
///     
///     /// &lt;summary&gt;
///     /// 批量发货
///     /// &lt;/summary&gt;
///     [HttpPost]
///     Task&lt;ResultModel&gt; BatchShipAsync(BatchShipRequestModel model);
/// }
/// 
/// // 自动生成的控制器实现：
/// public partial class OrderController
/// {
///     [HttpPut]
///     public async Task&lt;ResultModel&gt; CancelOrderAsync(CancelOrderRequestModel model)
///     {
///         CancelOrderModel serviceModel = Mapper.Map&lt;CancelOrderModel&gt;(model) ?? throw new ProjectException("映射失败");
///         BindLoginUserID(serviceModel);
///         await DefaultService.CancelOrderAsync(serviceModel);
///         return ResultModel.Success("取消订单成功");
///     }
///     
///     [HttpGet, AllowAnonymous]
///     public async Task&lt;ResultModel&lt;OrderStatisticsDTO&gt;&gt; GetOrderStatisticsAsync(DateTime startDate, DateTime endDate)
///     {
///         OrderStatisticsDTO result = await DefaultService.GetOrderStatisticsAsync(startDate, endDate);
///         return ResultModel&lt;OrderStatisticsDTO&gt;.Success(result, "获取订单统计成功");
///     }
///     
///     [HttpPost]
///     public async Task&lt;ResultModel&gt; BatchShipAsync(BatchShipRequestModel model)
///     {
///         BatchShipModel serviceModel = Mapper.Map&lt;BatchShipModel&gt;(model) ?? throw new ProjectException("映射失败");
///         BindLoginUserID(serviceModel);
///         await DefaultService.BatchShipAsync(serviceModel);
///         return ResultModel.Success("批量发货成功");
///     }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MapperControllerAttribute(MapperType type) : Attribute
{
    /// <summary>
    /// HTTP 方法类型
    /// </summary>
    public MapperType Type { get; private set; } = type;
    
    /// <summary>
    /// 是否允许匿名访问
    /// 设置为 true 时，生成的控制器方法会添加 [AllowAnonymous] 特性
    /// </summary>
    public bool IsAllowAnonymous { get; set; } = false;
}

/// <summary>
/// 映射类型枚举
/// 定义控制器方法支持的 HTTP 方法类型
/// </summary>
public enum MapperType
{
    /// <summary>
    /// HTTP GET 方法
    /// 用于查询操作，生成 [HttpGet] 特性
    /// </summary>
    Get,
    
    /// <summary>
    /// HTTP POST 方法
    /// 用于创建操作，生成 [HttpPost] 特性
    /// </summary>
    Post,
    
    /// <summary>
    /// HTTP PUT 方法
    /// 用于更新操作，生成 [HttpPut] 特性
    /// </summary>
    Put,
    
    /// <summary>
    /// HTTP DELETE 方法
    /// 用于删除操作，生成 [HttpDelete] 特性
    /// </summary>
    Delete,
    
    /// <summary>
    /// HTTP PATCH 方法
    /// 用于部分更新操作，生成 [HttpPatch] 特性
    /// </summary>
    Patch,
}
