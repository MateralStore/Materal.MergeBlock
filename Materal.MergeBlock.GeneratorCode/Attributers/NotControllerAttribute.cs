namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不生成控制器特性
/// 用于标记领域模型完全不生成控制器相关代码，包括控制器接口和实现
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorIControllerCodeAsync"/> 中：
/// 不会生成该领域模型的 I{DomainName}Controller 接口</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorControllersCodeAsync"/> 中：
/// 不会生成该领域模型的 {DomainName}Controller 实现类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerAccessorsGeneratorCodePlug.GeneratorControllerAccessorAsync"/> 中：
/// 不会生成该领域模型的 {DomainName}ControllerAccessor 访问器</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.EnumControllerGeneratorCodePlug.AfterExcuteAsync"/> 中：
/// 如果是枚举类型，不会在 EnumsController 中生成对应的枚举获取方法</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="EmptyControllerAttribute"/>：EmptyControllerAttribute 生成空白控制器供扩展，而 NotControllerAttribute 完全不生成控制器</description>
/// </item>
/// <item>
/// <description>通常与 <see cref="NotServiceAttribute"/> 配合使用，同时禁用服务层和控制器层的代码生成</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>纯内部使用的领域模型，不需要对外暴露 API</description></item>
/// <item><description>仅用于数据存储的实体，不需要业务接口</description></item>
/// <item><description>中间表、关联表等辅助数据表</description></item>
/// <item><description>系统内部配置表，不允许通过 API 访问</description></item>
/// <item><description>敏感数据表，需要通过其他方式访问而不是标准 API</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：中间表，不需要控制器
/// [NotController]
/// [NotService]
/// public class UserRoleRelation : BaseDomain
/// {
///     public Guid UserID { get; set; }
///     public Guid RoleID { get; set; }
///     
///     // 通过 User 和 Role 的控制器间接操作，不需要独立的控制器
/// }
/// 
/// // 示例2：系统内部配置，不对外暴露
/// [NotController]
/// public class SystemInternalConfig : BaseDomain
/// {
///     public string ConfigKey { get; set; }
///     public string ConfigValue { get; set; }
///     
///     // 仅供系统内部使用，不允许通过 API 访问
/// }
/// 
/// // 示例3：审计日志，只写不读（通过其他方式查询）
/// [NotController]
/// public class AuditLog : BaseDomain
/// {
///     public string Action { get; set; }
///     public string UserID { get; set; }
///     public DateTime Timestamp { get; set; }
///     
///     // 日志数据通过专门的日志查询系统访问，不通过标准 API
/// }
/// 
/// // 示例4：枚举类型，不需要控制器
/// [NotController]
/// public enum InternalStatus
/// {
///     [Description("内部状态1")]
///     Status1,
///     [Description("内部状态2")]
///     Status2
/// }
/// 
/// // 对比：标准领域模型（不使用 NotController）
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
/// 
/// // 会生成完整的控制器：
/// // - IProductController 接口（包含 Add, Edit, Delete, GetInfo, GetList 等方法）
/// // - ProductController 实现类
/// // - ProductControllerAccessor 访问器
/// 
/// // 对比：使用 EmptyController（生成空白控制器）
/// [EmptyController]
/// public class CustomEntity : BaseDomain
/// {
///     public string Name { get; set; }
/// }
/// 
/// // 会生成空白控制器供手动扩展：
/// // - ICustomEntityController : IMergeBlockController
/// // - CustomEntityController : {ModuleName}Controller
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NotControllerAttribute : Attribute { }
