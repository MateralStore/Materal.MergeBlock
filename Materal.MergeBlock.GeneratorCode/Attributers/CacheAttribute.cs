namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 缓存特性
/// 用于标记领域模型使用缓存仓储实现，提升数据访问性能
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorIRepositoryCodeAsync"/> 中：
/// 生成的仓储接口将继承 I{ModuleName}CacheRepository&lt;{DomainName}&gt; 而不是 I{ModuleName}Repository&lt;{DomainName}&gt;</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorRepositoryImplCodeAsync"/> 中：
/// 生成的仓储实现将继承 {ModuleName}CacheRepositoryImpl&lt;{DomainName}&gt; 并需要注入 ICacheHelper，同时需要实现 GetAllCacheName() 方法返回缓存键名</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorServiceImplsCodeAsync"/> 中：
/// 生成的服务实现中的树列表查询方法会优先从缓存中获取数据，提升查询性能</description>
/// </item>
/// </list>
/// <para><b>缓存仓储的特点：</b></para>
/// <list type="bullet">
/// <item><description>自动缓存所有数据到内存中，适用于数据量不大且读多写少的场景</description></item>
/// <item><description>写操作（增删改）会自动清除缓存，保证数据一致性</description></item>
/// <item><description>读操作优先从缓存获取，显著提升查询性能</description></item>
/// <item><description>需要额外的内存开销来存储缓存数据</description></item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>字典表、配置表等数据量小且变化不频繁的基础数据</description></item>
/// <item><description>组织架构、权限角色等层级结构数据</description></item>
/// <item><description>枚举扩展表、系统参数表等系统级配置数据</description></item>
/// <item><description>商品分类、地区信息等树形结构的基础数据</description></item>
/// </list>
/// <para><b>不适用场景：</b></para>
/// <list type="bullet">
/// <item><description>数据量大（超过万条）的业务数据表</description></item>
/// <item><description>频繁变更的业务数据（如订单、日志等）</description></item>
/// <item><description>实时性要求极高的数据</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 应用于字典表：数据量小，读多写少
/// [Cache]
/// public class Dictionary : BaseDomain
/// {
///     public string Code { get; set; }
///     public string Name { get; set; }
///     public string Value { get; set; }
/// }
/// 
/// // 应用于组织架构：树形结构，频繁查询
/// [Cache]
/// public class Organization : BaseTreeDomain&lt;Organization&gt;
/// {
///     public string Name { get; set; }
///     public string Code { get; set; }
/// }
/// 
/// // 不推荐：订单数据量大且频繁变更
/// // [Cache]  // ❌ 不要对订单使用缓存
/// public class Order : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     // ...
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CacheAttribute : Attribute { }
