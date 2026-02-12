namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 登录用户唯一标识特性
/// 用于标记属性为当前登录用户的ID，该属性会在添加和编辑操作时自动从当前登录上下文中填充，不需要客户端传递
/// </summary>
/// <remarks>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorAddRequestModelAsync"/> 中：
/// 标记的属性不会被包含在 Add{DomainName}RequestModel 类中（自动跳过）</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorEditRequestModelAsync"/> 中：
/// 标记的属性不会被包含在 Edit{DomainName}RequestModel 类中（自动跳过）</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorAddModelAsync"/> 中：
/// 标记的属性会被包含在 Add{DomainName}Model 类中，并添加 [LoginUserID] 特性标记</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorEditModelAsync"/> 中：
/// 标记的属性会被包含在 Edit{DomainName}Model 类中，并添加 [LoginUserID] 特性标记</description>
/// </item>
/// </list>
/// <para><b>自动填充机制：</b></para>
/// <list type="bullet">
/// <item><description>控制器在接收到请求后，会将 RequestModel 映射为 ServiceModel</description></item>
/// <item><description>映射完成后，控制器会调用 BindLoginUserID() 方法</description></item>
/// <item><description>该方法会自动将当前登录用户的ID填充到标记了 [LoginUserID] 的属性中</description></item>
/// <item><description>客户端无需（也不应该）传递这些属性的值</description></item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>创建人ID（CreateUserID）：记录数据的创建者</description></item>
/// <item><description>修改人ID（UpdateUserID）：记录数据的最后修改者</description></item>
/// <item><description>所属用户ID（OwnerUserID）：标记数据的所有者</description></item>
/// <item><description>操作人ID（OperatorUserID）：记录执行操作的用户</description></item>
/// <item><description>审核人ID（AuditorUserID）：记录审核操作的用户</description></item>
/// </list>
/// <para><b>安全性说明：</b></para>
/// <list type="bullet">
/// <item><description>使用此特性可以防止客户端伪造用户ID，提高系统安全性</description></item>
/// <item><description>用户ID始终从服务端的登录上下文中获取，不信任客户端传递的值</description></item>
/// <item><description>适用于需要记录操作人信息的审计场景</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：记录创建人和修改人
/// public class Article : BaseDomain
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
///     
///     [LoginUserID]
///     [NotEdit]  // 创建人不允许修改
///     public Guid CreateUserID { get; set; }
///     
///     [LoginUserID]
///     public Guid UpdateUserID { get; set; }  // 每次修改时更新
/// }
/// 
/// // 生成的添加请求模型（不包含 LoginUserID 属性）：
/// public partial class AddArticleRequestModel : IAddRequestModel
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
///     // CreateUserID 和 UpdateUserID 不在请求模型中
/// }
/// 
/// // 生成的添加服务模型（包含 LoginUserID 属性）：
/// public partial class AddArticleModel : IAddServiceModel
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
///     
///     [LoginUserID]
///     public Guid CreateUserID { get; set; }  // 会自动填充
///     
///     [LoginUserID]
///     public Guid UpdateUserID { get; set; }  // 会自动填充
/// }
/// 
/// // 控制器中的自动填充过程：
/// public async Task&lt;ResultModel&gt; AddAsync(AddArticleRequestModel requestModel)
/// {
///     AddArticleModel model = Mapper.Map&lt;AddArticleModel&gt;(requestModel);
///     BindLoginUserID(model);  // 自动填充 CreateUserID 和 UpdateUserID
///     await DefaultService.AddAsync(model);
///     return ResultModel.Success("添加成功");
/// }
/// 
/// // 示例2：数据所有者
/// public class PrivateNote : BaseDomain
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
///     
///     [LoginUserID]
///     [NotEdit]  // 所有者不允许修改
///     public Guid OwnerUserID { get; set; }  // 笔记所有者
/// }
/// 
/// // 示例3：审核场景
/// public class LeaveRequest : BaseDomain
/// {
///     public string Reason { get; set; }
///     public DateTime StartDate { get; set; }
///     public DateTime EndDate { get; set; }
///     
///     [LoginUserID]
///     [NotEdit]
///     public Guid ApplicantUserID { get; set; }  // 申请人
///     
///     public Guid? AuditorUserID { get; set; }  // 审核人（审核时手动设置）
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class LoginUserIDAttribute : Attribute { }
