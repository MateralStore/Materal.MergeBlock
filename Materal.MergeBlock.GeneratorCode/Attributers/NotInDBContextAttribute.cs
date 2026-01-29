namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 不在 DBContext 中生成特性
/// 用于标记领域模型不在 EF Core 的 DbContext 类中生成对应的 DbSet 属性
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorDBContextCodeAsync"/> 中：
/// 不会在 {ModuleName}DBContext 类中生成该领域模型的 DbSet&lt;{DomainName}&gt; 属性</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="NotEntityConfigAttribute"/>：NotEntityConfigAttribute 控制是否生成实体配置类，而 NotInDBContextAttribute 控制是否在 DbContext 中注册 DbSet</description>
/// </item>
/// <item>
/// <description><see cref="NotRepositoryAttribute"/>：通常与 NotRepositoryAttribute 配合使用，同时禁用仓储和 DbContext 注册</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>视图（View）实体，通过原始 SQL 查询访问，不需要 DbSet</description></item>
/// <item><description>只读查询实体，不需要通过 EF Core 的变更跟踪</description></item>
/// <item><description>临时表或中间结果集，不需要在 DbContext 中注册</description></item>
/// <item><description>使用 FromSqlRaw 或 FromSqlInterpolated 直接查询的实体</description></item>
/// <item><description>跨数据库的实体，不属于当前 DbContext 管理</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：数据库视图，不需要 DbSet
/// [NotInDBContext]
/// [View]  // 标记为视图
/// public class OrderStatisticsView : BaseDomain
/// {
///     public string OrderNo { get; set; }
///     public decimal TotalAmount { get; set; }
///     public int ItemCount { get; set; }
///     
///     // 对应数据库视图 V_OrderStatistics，通过原始 SQL 查询
/// }
/// 
/// // 在仓储中使用原始 SQL 查询：
/// public class OrderStatisticsRepository
/// {
///     public async Task&lt;List&lt;OrderStatisticsView&gt;&gt; GetStatisticsAsync()
///     {
///         return await _dbContext.Set&lt;OrderStatisticsView&gt;()
///             .FromSqlRaw("SELECT * FROM V_OrderStatistics")
///             .ToListAsync();
///     }
/// }
/// 
/// // 示例2：只读查询实体
/// [NotInDBContext]
/// [NotRepository]
/// public class ReportData : BaseDomain
/// {
///     public string Category { get; set; }
///     public decimal Amount { get; set; }
///     public int Count { get; set; }
///     
///     // 通过存储过程或复杂 SQL 查询获取，不需要 DbSet
/// }
/// 
/// // 示例3：跨数据库实体
/// [NotInDBContext]
/// public class ExternalSystemData : BaseDomain
/// {
///     public string DataKey { get; set; }
///     public string DataValue { get; set; }
///     
///     // 来自外部数据库，不在当前 DbContext 中管理
/// }
/// 
/// // 生成的 DBContext（不包含标记的实体）：
/// public sealed partial class SalesDBContext : DbContext
/// {
///     // Order 会生成 DbSet
///     public DbSet&lt;Order&gt;? Order { get; set; }
///     
///     // Product 会生成 DbSet
///     public DbSet&lt;Product&gt;? Product { get; set; }
///     
///     // OrderStatisticsView 不会生成 DbSet（标记了 NotInDBContext）
///     // ReportData 不会生成 DbSet（标记了 NotInDBContext）
///     // ExternalSystemData 不会生成 DbSet（标记了 NotInDBContext）
///     
///     protected override void OnModelCreating(ModelBuilder modelBuilder) 
///         => modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
/// }
/// 
/// // 对比：标准实体（不使用 NotInDBContext）
/// public class Customer : BaseDomain
/// {
///     public string Name { get; set; }
///     public string Email { get; set; }
/// }
/// 
/// // 会在 DBContext 中生成：
/// public sealed partial class SalesDBContext : DbContext
/// {
///     public DbSet&lt;Customer&gt;? Customer { get; set; }
/// }
/// 
/// // 注意：即使使用 NotInDBContext，仍然可以通过 Set&lt;T&gt;() 方法访问
/// var statistics = await dbContext.Set&lt;OrderStatisticsView&gt;()
///     .FromSqlRaw("SELECT * FROM V_OrderStatistics")
///     .ToListAsync();
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NotInDBContextAttribute : Attribute { }
