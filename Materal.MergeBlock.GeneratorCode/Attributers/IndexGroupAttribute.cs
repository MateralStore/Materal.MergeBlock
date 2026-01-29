namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 位序分组特性
/// 用于标记索引域（IIndexDomain）中的分组属性，使位序在不同分组内独立管理
/// </summary>
/// <remarks>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorIRepositoryCodeAsync"/> 中：
/// 生成的 GetMaxIndexAsync 方法会包含该分组属性作为参数</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorRepositoryImplCodeAsync"/> 中：
/// GetMaxIndexAsync 方法实现会根据分组属性过滤数据，返回该分组内的最大位序</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorServiceImplsCodeAsync"/> 中：
/// ExchangeIndexAsync 方法会将分组属性名称传递给位序交换辅助方法，确保位序交换在同一分组内进行</description>
/// </item>
/// </list>
/// <para><b>前置条件：</b></para>
/// <list type="bullet">
/// <item><description>领域模型必须实现了 IIndexDomain 接口（包含 Index 属性）</description></item>
/// <item><description>不能与 EmptyIndexAttribute 同时使用（EmptyIndexAttribute 会禁用所有位序功能）</description></item>
/// <item><description>一个领域模型只能有一个属性标记为 IndexGroupAttribute</description></item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>同一父级下的子项需要独立排序（如不同分类下的商品）</description></item>
/// <item><description>不同状态下的数据需要独立排序（如待办事项的不同优先级）</description></item>
/// <item><description>多租户系统中，不同租户的数据需要独立排序</description></item>
/// <item><description>分组内的数据需要独立维护位序，避免跨组干扰</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：商品在不同分类下独立排序
/// public class Product : BaseDomain, IIndexDomain
/// {
///     public string Name { get; set; }
///     
///     [IndexGroup]
///     public Guid CategoryID { get; set; }  // 按分类分组
///     
///     public int Index { get; set; }  // 位序在同一分类内独立
/// }
/// 
/// // 生成的仓储接口：
/// public partial interface IProductRepository : I{ModuleName}Repository&lt;Product&gt;
/// {
///     // 获取指定分类下的最大位序
///     Task&lt;int&gt; GetMaxIndexAsync(Guid categoryID);
/// }
/// 
/// // 生成的仓储实现：
/// public async Task&lt;int&gt; GetMaxIndexAsync(Guid categoryID)
/// {
///     if (!await DBSet.AnyAsync(m => m.CategoryID == categoryID)) return -1;
///     int result = await DBSet.Where(m => m.CategoryID == categoryID).MaxAsync(m => m.Index);
///     return result;
/// }
/// 
/// // 示例2：待办事项按优先级分组排序
/// public class TodoItem : BaseDomain, IIndexDomain
/// {
///     public string Title { get; set; }
///     
///     [IndexGroup]
///     public TaskPriority Priority { get; set; }  // 按优先级分组
///     
///     public int Index { get; set; }  // 高、中、低优先级各自独立排序
/// }
/// 
/// // 示例3：多租户系统，租户内独立排序
/// public class Announcement : BaseDomain, IIndexDomain
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
///     
///     [IndexGroup]
///     public Guid TenantID { get; set; }  // 按租户分组
///     
///     public int Index { get; set; }  // 每个租户的公告独立排序
/// }
/// 
/// // 对比：不使用 IndexGroup（全局位序）
/// public class Banner : BaseDomain, IIndexDomain
/// {
///     public string Title { get; set; }
///     public int Index { get; set; }  // 所有横幅全局排序
/// }
/// 
/// // 生成的仓储接口（无分组参数）：
/// public partial interface IBannerRepository : I{ModuleName}Repository&lt;Banner&gt;
/// {
///     Task&lt;int&gt; GetMaxIndexAsync();  // 获取全局最大位序
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class IndexGroupAttribute : Attribute { }
