namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不生成服务特性
/// 用于标记领域模型完全不生成服务相关代码，包括服务接口和实现
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorIServicesCodeAsync"/> 中：
/// 不会生成该领域模型的 I{DomainName}Service 接口</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorServiceImplsCodeAsync"/> 中：
/// 不会生成该领域模型的 {DomainName}ServiceImpl 实现类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorControllersCodeAsync"/> 中：
/// 生成的控制器实现会根据是否有服务调整继承的基类</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="EmptyServiceAttribute"/>：EmptyServiceAttribute 生成空白服务供扩展，而 NotServiceAttribute 完全不生成服务</description>
/// </item>
/// <item>
/// <description><see cref="NotRepositoryAttribute"/>：可以与 NotRepositoryAttribute 配合使用，同时禁用服务层和仓储层</description>
/// </item>
/// <item>
/// <description><see cref="NotControllerAttribute"/>：可以与 NotControllerAttribute 配合使用，同时禁用控制器层和服务层</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>纯数据实体，不需要业务逻辑层</description></item>
/// <item><description>中间表、关联表，通过其他服务间接操作</description></item>
/// <item><description>配置表、字典表，通过专门的配置服务统一管理</description></item>
/// <item><description>视图实体，只读数据不需要业务逻辑</description></item>
/// <item><description>系统内部使用的实体，不对外提供服务接口</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：中间表，不需要独立的服务
/// [NotService]
/// [NotController]
/// [NotRepository]
/// public class UserRoleRelation : BaseDomain
/// {
///     public Guid UserID { get; set; }
///     public Guid RoleID { get; set; }
///     
///     // 通过 UserService 和 RoleService 间接操作，不需要独立的服务
/// }
/// 
/// // 示例2：配置表，通过统一的配置服务管理
/// [NotService]
/// [NotController]
/// public class SystemConfig : BaseDomain
/// {
///     public string ConfigKey { get; set; }
///     public string ConfigValue { get; set; }
///     public string Category { get; set; }
///     
///     // 通过 ConfigurationService 统一管理所有配置，不需要独立的服务
/// }
/// 
/// // 统一的配置服务：
/// public class ConfigurationService
/// {
///     private readonly ISystemConfigRepository _repository;
///     
///     public async Task&lt;string&gt; GetConfigValueAsync(string key)
///     {
///         var config = await _repository.FindFirstAsync(m => m.ConfigKey == key);
///         return config?.ConfigValue ?? string.Empty;
///     }
/// }
/// 
/// // 示例3：视图实体，只读数据
/// [NotService]
/// [NotController]
/// [NotRepository]
/// [NotInDBContext]
/// [View]
/// public class OrderSummaryView : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public int ItemCount { get; set; }
///     
///     // 数据库视图，通过 OrderService 查询，不需要独立的服务
/// }
/// 
/// // 示例4：系统内部实体
/// [NotService]
/// [NotController]
/// public class AuditLog : BaseDomain
/// {
///     public string Action { get; set; }
///     public string UserID { get; set; }
///     public DateTime Timestamp { get; set; }
///     
///     // 系统内部审计日志，通过 AuditService 统一记录
/// }
/// 
/// // 示例5：枚举扩展表
/// [NotService]
/// public class StatusDescription : BaseDomain
/// {
///     public int StatusCode { get; set; }
///     public string Description { get; set; }
///     public string Color { get; set; }
///     
///     // 枚举描述扩展表，通过缓存服务加载，不需要独立的业务服务
/// }
/// 
/// // 对比：标准实体（不使用 NotService）
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
/// 
/// // 会生成完整的服务：
/// // - IProductService 接口（继承 IBaseService&lt;...&gt;）
/// // - ProductServiceImpl 实现类（包含 Add, Edit, Delete, GetInfo, GetList 等方法）
/// 
/// // 控制器会注入服务：
/// public partial class ProductController : {ModuleName}Controller&lt;...&gt;, IProductController
/// {
///     // 构造函数注入 IProductService
///     // DefaultService 属性可用
/// }
/// 
/// // 对比：使用 EmptyService（生成空白服务）
/// [EmptyService]
/// public class CustomEntity : BaseDomain
/// {
///     public string Name { get; set; }
/// }
/// 
/// // 会生成空白服务供手动扩展：
/// // - ICustomEntityService : IBaseService
/// // - CustomEntityServiceImpl : BaseServiceImpl&lt;...&gt;
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NotServiceAttribute : Attribute { }
