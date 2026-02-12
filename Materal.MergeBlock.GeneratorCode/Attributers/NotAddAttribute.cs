namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不添加特性
/// 用于标记在代码生成过程中不生成添加相关代码的类或属性
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorAddRequestModelAsync"/> 中：
/// 不会生成该领域模型的 Add{DomainName}RequestModel 类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorAddModelAsync"/> 中：
/// 不会生成该领域模型的 Add{DomainName}Model 类</description>
/// </item>
/// </list>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorAddRequestModelAsync"/> 中：
/// 该属性不会被包含在 Add{DomainName}RequestModel 类中</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorAddModelAsync"/> 中：
/// 该属性不会被包含在 Add{DomainName}Model 类中</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="LoginUserIDAttribute"/>：标记了 LoginUserIDAttribute 的属性会自动排除在添加模型之外，无需再标记 NotAddAttribute</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>标记只读的领域模型类，这些类不需要添加操作</description></item>
/// <item><description>标记自动生成的属性（如ID、创建时间等），这些属性不应该在添加请求中由用户提供</description></item>
/// <item><description>标记计算属性或导航属性，这些属性不需要在添加时设置</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 应用于类：整个领域模型不生成添加相关代码
/// [NotAdd]
/// public class ReadOnlyDomain : BaseDomain
/// {
///     // ...
/// }
/// 
/// // 应用于属性：该属性不会出现在添加请求模型中
/// public class User : BaseDomain
/// {
///     [NotAdd]
///     public DateTime LastLoginTime { get; set; }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NotAddAttribute : Attribute { }
