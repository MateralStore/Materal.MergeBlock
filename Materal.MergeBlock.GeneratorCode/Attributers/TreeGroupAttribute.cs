namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 树分组特性
/// 用于标记树形域（ITreeDomain）中的分组属性，使树形结构在不同分组内独立管理
/// </summary>
/// <remarks>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.TreeGeneratorCodePlug.GeneratorTreeQueryRequestModelAsync"/> 中：
/// 生成的树形查询请求模型会包含该分组属性作为查询条件</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.TreeGeneratorCodePlug.GeneratorTreeQueryModelAsync"/> 中：
/// 生成的树形查询服务模型会包含该分组属性，并添加 [Equal] 特性用于精确查询</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.TreeGeneratorCodePlug.GeneratorServiceImplsTreeCodeAsync"/> 中：
/// ExchangeParentAsync 方法会将分组属性名称传递给辅助方法，确保树形操作在同一分组内进行</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.IndexGeneratorCodePlug.GeneratorServiceImplsIndexCodeAsync"/> 中：
/// 当领域同时为 IIndexDomain 时，ExchangeIndexAsync 会将 TreeGroup 属性名称传递给辅助方法，确保位序交换/父级交换在同一分组内进行</description>
/// </item>
/// </list>
/// <para><b>前置条件：</b></para>
/// <list type="bullet">
/// <item><description>领域模型必须实现了 ITreeDomain 接口（包含 ParentID 属性）</description></item>
/// <item><description>当生成器启用 Tree 相关代码生成时，此特性才会生效</description></item>
/// <item><description>一个领域模型只能有一个属性标记为 TreeGroupAttribute</description></item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>多租户系统中，不同租户的树形数据需要独立管理</description></item>
/// <item><description>同一类型的树形数据按不同维度分组（如不同项目、不同部门等）</description></item>
/// <item><description>树形结构需要在特定范围内独立维护，避免跨组干扰</description></item>
/// <item><description>实现多个独立的树形结构共存</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：多租户系统的组织架构
/// public class Organization : BaseDomain, ITreeDomain
/// {
///     public string Name { get; set; }
///     public Guid? ParentID { get; set; }
///     
///     [TreeGroup]
///     public Guid TenantID { get; set; }  // 按租户分组
///     
///     // 每个租户有独立的组织架构树
/// }
/// 
/// // 生成的树形查询请求模型：
/// public partial class QueryOrganizationTreeListRequestModel : FilterModel
/// {
///     public Guid? ParentID { get; set; }
///     
///     [Equal]
///     public Guid? TenantID { get; set; }  // 租户ID作为查询条件
/// }
/// 
/// // 生成的服务实现（树形操作在同一租户内）：
/// public partial class OrganizationServiceImpl : BaseServiceImpl&lt;...&gt;
/// {
///     public async Task ExchangeParentAsync(ExchangeParentModel model)
///     {
///         OnExchangeParentBefore(model);
///         // 传递分组属性名称，确保在同一租户内更改父级
///         await ServiceImplHelper.ExchangeParentByGroupPropertiesAsync&lt;...&gt;(
///             model, DefaultRepository, UnitOfWork, nameof(Organization.TenantID));
///         OnExchangeParentAfter(model);
///     }
/// }
/// 
/// // 示例2：项目任务树
/// public class ProjectTask : BaseDomain, ITreeDomain, IIndexDomain
/// {
///     public string Title { get; set; }
///     public Guid? ParentID { get; set; }
///     public int Index { get; set; }
///     
///     [TreeGroup]
///     [IndexGroup]  // 同时作为位序分组
///     public Guid ProjectID { get; set; }  // 按项目分组
///     
///     // 每个项目有独立的任务树，位序也在项目内独立
/// }
/// 
/// // 示例3：部门分类树
/// public class DepartmentCategory : BaseDomain, ITreeDomain
/// {
///     public string Name { get; set; }
///     public Guid? ParentID { get; set; }
///     
///     [TreeGroup]
///     public Guid DepartmentID { get; set; }  // 按部门分组
///     
///     // 每个部门有独立的分类树
/// }
/// 
/// // 示例4：多维度分组
/// public class KnowledgeBase : BaseDomain, ITreeDomain
/// {
///     public string Title { get; set; }
///     public Guid? ParentID { get; set; }
///     
///     [TreeGroup]
///     public string Category { get; set; }  // 按类别分组
///     
///     // 不同类别（技术、业务、管理等）有独立的知识库树
/// }
/// 
/// // 对比：不使用 TreeGroup（全局树形结构）
/// public class Menu : BaseDomain, ITreeDomain
/// {
///     public string Name { get; set; }
///     public Guid? ParentID { get; set; }
///     public int Index { get; set; }
///     
///     // 全局菜单树，所有菜单项在同一个树中
/// }
/// 
/// // 生成的树形查询请求模型（无分组条件）：
/// public partial class QueryMenuTreeListRequestModel : FilterModel
/// {
///     public Guid? ParentID { get; set; }
///     // 没有分组属性
/// }
/// 
/// // 注意事项：
/// // 1. TreeGroup 和 IndexGroup 可以标记在同一个属性上，实现树形和位序的双重分组
/// // 2. 查询树形列表时必须指定分组属性的值
/// // 3. 更改父级和交换位序操作会自动限制在同一分组内
/// // 4. 适用于需要多个独立树形结构共存的场景
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class TreeGroupAttribute : Attribute { }
