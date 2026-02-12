namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不生成 DTO 特性
/// 用于标记在代码生成过程中不生成 DTO（数据传输对象）相关代码的类或属性
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.DTOGeneratorCodePlug.GeneratorListDTOModelAsync"/> 中：
/// 不会生成该领域模型的 {DomainName}ListDTO 类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.DTOGeneratorCodePlug.GeneratorDTOModelAsync"/> 中：
/// 不会生成该领域模型的 {DomainName}DTO 类</description>
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
/// 该属性不会被包含在 {DomainName}DTO 类中</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="NotListDTOAttribute"/>：NotListDTOAttribute 仅控制属性是否出现在 ListDTO 中，而 NotDTOAttribute 控制属性是否出现在所有 DTO 中</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>标记不需要返回给客户端的领域模型（如纯内部使用的实体）</description></item>
/// <item><description>标记敏感属性，不应该在 API 响应中暴露（如密码哈希、加密密钥等）</description></item>
/// <item><description>标记仅用于内部计算的属性，客户端不需要</description></item>
/// <item><description>标记导航属性或复杂对象，避免循环引用</description></item>
/// <item><description>标记大字段（如二进制数据），不适合在 DTO 中传输</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：应用于类，完全不生成 DTO
/// [NotDTO]
/// public class InternalCache : BaseDomain
/// {
///     public string CacheKey { get; set; }
///     public string CacheValue { get; set; }
///     
///     // 纯内部缓存实体，不需要返回给客户端
/// }
/// 
/// // 示例2：应用于属性，排除敏感信息
/// public class User : BaseDomain
/// {
///     public string Username { get; set; }
///     public string Email { get; set; }
///     
///     [NotDTO]
///     public string PasswordHash { get; set; }  // 密码哈希不应该返回给客户端
///     
///     [NotDTO]
///     public string SecurityStamp { get; set; }  // 安全戳不应该暴露
///     
///     public DateTime LastLoginTime { get; set; }
/// }
/// 
/// // 生成的 DTO（不包含 NotDTO 属性）：
/// public partial class UserListDTO : IListDTO
/// {
///     public Guid ID { get; set; }
///     public DateTime CreateTime { get; set; }
///     public string Username { get; set; }
///     public string Email { get; set; }
///     public DateTime LastLoginTime { get; set; }
///     // PasswordHash 和 SecurityStamp 不在 DTO 中
/// }
/// 
/// // 示例3：排除导航属性
/// public class Order : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public Guid CustomerID { get; set; }
///     
///     [NotDTO]
///     public Customer Customer { get; set; }  // 导航属性，避免循环引用
///     
///     [NotDTO]
///     public List&lt;OrderItem&gt; OrderItems { get; set; }  // 集合导航属性
/// }
/// 
/// // 示例4：排除大字段
/// public class Attachment : BaseDomain
/// {
///     public string FileName { get; set; }
///     public string FileType { get; set; }
///     public long FileSize { get; set; }
///     
///     [NotDTO]
///     public byte[] FileContent { get; set; }  // 文件内容不在 DTO 中，通过专门的下载接口获取
/// }
/// 
/// // 示例5：排除计算属性
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
///     public int Stock { get; set; }
///     
///     [NotDTO]
///     public decimal InternalCost { get; set; }  // 内部成本，不对外暴露
///     
///     [NotDTO]
///     public decimal ProfitMargin => Price - InternalCost;  // 利润率，内部计算
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NotDTOAttribute : Attribute { }
