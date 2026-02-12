namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不编辑特性
/// 用于标记在代码生成过程中不生成编辑相关代码的类或属性
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorEditRequestModelAsync"/> 中：
/// 不会生成该领域模型的 Edit{DomainName}RequestModel 类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorEditModelAsync"/> 中：
/// 不会生成该领域模型的 Edit{DomainName}Model 类</description>
/// </item>
/// </list>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorEditRequestModelAsync"/> 中：
/// 该属性不会被包含在 Edit{DomainName}RequestModel 类中</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorEditModelAsync"/> 中：
/// 该属性不会被包含在 Edit{DomainName}Model 类中</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="LoginUserIDAttribute"/>：标记了 LoginUserIDAttribute 的属性会自动排除在编辑模型之外，无需再标记 NotEditAttribute</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>标记只读的领域模型类，这些类不需要编辑操作</description></item>
/// <item><description>标记不可修改的属性（如创建时间、创建人等），这些属性在编辑时不应该被修改</description></item>
/// <item><description>标记计算属性或导航属性，这些属性不需要在编辑时设置</description></item>
/// <item><description>标记业务规则不允许修改的字段（如订单号、流水号等）</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 应用于类：整个领域模型不生成编辑相关代码
/// [NotEdit]
/// public class AuditLog : BaseDomain
/// {
///     // 审计日志通常只能添加，不能编辑
/// }
/// 
/// // 应用于属性：该属性不会出现在编辑请求模型中
/// public class Order : BaseDomain
/// {
///     [NotEdit]
///     public string OrderNo { get; set; }  // 订单号不允许修改
///     
///     [NotEdit]
///     public DateTime CreateTime { get; set; }  // 创建时间不允许修改
///     
///     public string Remark { get; set; }  // 备注可以修改
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NotEditAttribute : Attribute { }
