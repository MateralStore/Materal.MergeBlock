namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 空白树特性
/// 用于标记树形域（ITreeDomain）不生成树形结构相关的代码，即使领域模型实现了树形接口
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorIControllerCodeAsync"/> 中：
/// 不会在控制器接口中生成 ExchangeParentAsync 和 GetTreeListAsync 方法</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorControllersCodeAsync"/> 中：
/// 不会在控制器实现中生成 ExchangeParentAsync 和 GetTreeListAsync 方法</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorIServicesCodeAsync"/> 中：
/// 不会在服务接口中生成 ExchangeParentAsync 和 GetTreeListAsync 方法</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorServiceImplsCodeAsync"/> 中：
/// 不会在服务实现中生成 ExchangeParentAsync、GetTreeListAsync 方法及相关的钩子方法（OnExchangeParentBefore/After、OnToTreeBefore/After、OnConvertToTreeDTO）</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.DTOGeneratorCodePlug.GeneratorTreeListDTOModelAsync"/> 中：
/// 不会生成 {DomainName}TreeListDTO 类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.RequesetModelGeneratorCodePlug.GeneratorTreeQueryRequestModelAsync"/> 中：
/// 不会生成 Query{DomainName}TreeListRequestModel 类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesModelGeneratorCodePlug.GeneratorTreeQueryModelAsync"/> 中：
/// 不会生成 Query{DomainName}TreeListModel 类</description>
/// </item>
/// </list>
/// <para><b>前置条件：</b></para>
/// <list type="bullet">
/// <item><description>领域模型必须实现了 ITreeDomain 接口（包含 ParentID 属性）</description></item>
/// <item><description>如果领域模型未实现 ITreeDomain，此特性无效果</description></item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>树形域的父子关系由其他业务逻辑控制，不需要标准的父级交换功能</description></item>
/// <item><description>ParentID 字段仅用于数据关联，不需要树形查询功能</description></item>
/// <item><description>树形结构由系统自动维护，不允许手动调整父级</description></item>
/// <item><description>需要自定义树形查询逻辑，不使用框架提供的标准实现</description></item>
/// <item><description>虽然有父子关系，但业务上不需要以树形结构展示</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：评论回复，有父子关系但不需要树形展示
/// [EmptyTree]
/// public class Comment : BaseDomain, ITreeDomain
/// {
///     public string Content { get; set; }
///     public Guid? ParentID { get; set; }  // 父评论ID，用于回复功能
///     
///     // 虽然有父子关系，但业务上按时间线性展示，不需要树形结构
/// }
/// 
/// // 示例2：任务依赖，父子关系由工作流控制
/// [EmptyTree]
/// public class WorkflowTask : BaseDomain, ITreeDomain
/// {
///     public string TaskName { get; set; }
///     public Guid? ParentID { get; set; }  // 前置任务ID
///     
///     // 任务依赖关系由工作流引擎管理，不使用标准的树形操作
/// }
/// 
/// // 示例3：组织架构，但使用自定义的树形查询
/// [EmptyTree]
/// public class Department : BaseDomain, ITreeDomain
/// {
///     public string Name { get; set; }
///     public Guid? ParentID { get; set; }
///     
///     // 需要自定义树形查询逻辑（如包含权限过滤），不使用标准实现
/// }
/// 
/// // 对比：标准树形域（不使用 EmptyTree）
/// public class Category : BaseDomain, ITreeDomain
/// {
///     public string Name { get; set; }
///     public Guid? ParentID { get; set; }
/// }
/// 
/// // 会生成完整的树形功能：
/// // - ICategoryController.ExchangeParentAsync (更改父级)
/// // - ICategoryController.GetTreeListAsync (查询树列表)
/// // - ICategoryService.ExchangeParentAsync
/// // - ICategoryService.GetTreeListAsync
/// // - CategoryTreeListDTO (树形 DTO，包含 Children 属性)
/// // - QueryCategoryTreeListRequestModel (树形查询请求模型)
/// // - QueryCategoryTreeListModel (树形查询服务模型)
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EmptyTreeAttribute : Attribute { }
