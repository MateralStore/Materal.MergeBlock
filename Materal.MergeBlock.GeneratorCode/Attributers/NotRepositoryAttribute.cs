namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不生成仓储特性
/// 用于标记领域模型完全不生成仓储相关代码，包括仓储接口和实现
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorIRepositoryCodeAsync"/> 中：
/// 不会生成该领域模型的 I{DomainName}Repository 接口</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorRepositoryImplCodeAsync"/> 中：
/// 不会生成该领域模型的 {DomainName}RepositoryImpl 实现类</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorServiceImplsCodeAsync"/> 中：
/// 生成的服务实现会根据是否有仓储调整继承的基类</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="NotInDBContextAttribute"/>：通常与 NotInDBContextAttribute 配合使用，同时禁用仓储和 DbContext 注册</description>
/// </item>
/// <item>
/// <description><see cref="NotServiceAttribute"/>：可以与 NotServiceAttribute 配合使用，同时禁用服务层和仓储层</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>纯内存对象，不需要持久化到数据库</description></item>
/// <item><description>视图模型（ViewModel），仅用于数据展示</description></item>
/// <item><description>聚合服务，不对应具体的数据表</description></item>
/// <item><description>通过其他方式访问数据（如原始 SQL、存储过程等），不需要标准仓储</description></item>
/// <item><description>跨数据库实体，由其他 DbContext 管理</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：纯内存对象
/// [NotRepository]
/// [NotInDBContext]
/// public class CacheData : BaseDomain
/// {
///     public string Key { get; set; }
///     public string Value { get; set; }
///     public DateTime ExpireTime { get; set; }
///     
///     // 纯内存缓存对象，不需要持久化
/// }
/// 
/// // 示例2：视图模型
/// [NotRepository]
/// [NotInDBContext]
/// public class DashboardViewModel : BaseDomain
/// {
///     public int TotalOrders { get; set; }
///     public decimal TotalRevenue { get; set; }
///     public int ActiveUsers { get; set; }
///     
///     // 仪表板视图模型，数据来自多个表的聚合
/// }
/// 
/// // 示例3：聚合服务实体
/// [NotRepository]
/// [EmptyService]
/// public class ReportService : BaseDomain
/// {
///     // 该实体对应的服务会聚合多个仓储的数据
/// }
/// 
/// // 生成的服务实现（不依赖仓储）：
/// public partial class ReportServiceServiceImpl : BaseServiceImpl&lt;I{ModuleName}UnitOfWork&gt;, IReportServiceService
/// {
///     // 不注入 IReportServiceRepository，因为没有生成仓储
///     // 可以注入其他仓储来聚合数据
/// }
/// 
/// // 示例4：使用原始 SQL 查询的实体
/// [NotRepository]
/// [NotInDBContext]
/// [View]
/// public class ComplexReportView : BaseDomain
/// {
///     public string Category { get; set; }
///     public decimal Amount { get; set; }
///     public int Count { get; set; }
///     
///     // 通过原始 SQL 或存储过程查询，不需要标准仓储
/// }
/// 
/// // 在服务中直接使用 DbContext：
/// public class ReportService
/// {
///     private readonly SalesDBContext _dbContext;
///     
///     public async Task&lt;List&lt;ComplexReportView&gt;&gt; GetReportAsync()
///     {
///         return await _dbContext.Set&lt;ComplexReportView&gt;()
///             .FromSqlRaw("EXEC sp_GetComplexReport")
///             .ToListAsync();
///     }
/// }
/// 
/// // 示例5：跨数据库实体
/// [NotRepository]
/// [NotInDBContext]
/// public class ExternalData : BaseDomain
/// {
///     public string DataKey { get; set; }
///     public string DataValue { get; set; }
///     
///     // 来自外部数据库，由专门的 ExternalDbContext 管理
/// }
/// 
/// // 对比：标准实体（不使用 NotRepository）
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
/// 
/// // 会生成完整的仓储：
/// // - IProductRepository 接口（继承 I{ModuleName}Repository&lt;Product&gt;）
/// // - ProductRepositoryImpl 实现类（继承 {ModuleName}RepositoryImpl&lt;Product&gt;）
/// 
/// // 服务实现会注入仓储：
/// public partial class ProductServiceImpl : BaseServiceImpl&lt;...&gt;, IProductService
/// {
///     // 构造函数注入 IProductRepository
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NotRepositoryAttribute : Attribute { }
