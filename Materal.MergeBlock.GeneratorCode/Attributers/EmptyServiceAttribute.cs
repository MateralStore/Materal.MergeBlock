namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 空白服务特性
/// 用于标记领域模型生成不包含标准 CRUD 方法的空白服务，仅保留服务基础结构
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorIServicesCodeAsync"/> 中：
/// 生成的服务接口将继承 IBaseService 而不是 IBaseService&lt;TAdd, TEdit, TQuery, TDTO, TListDTO&gt;</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorServiceImplsCodeAsync"/> 中：
/// 生成的服务实现将继承简化的基类，不包含标准的增删改查方法</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorIControllerCodeAsync"/> 中：
/// 生成的控制器接口也会相应简化（通常与 EmptyControllerAttribute 配合使用）</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorControllersCodeAsync"/> 中：
/// 生成的控制器实现会根据服务类型调整继承的基类</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="EmptyControllerAttribute"/>：通常与 EmptyControllerAttribute 配合使用，同时简化服务层和控制器层</description>
/// </item>
/// <item>
/// <description><see cref="NotServiceAttribute"/>：NotServiceAttribute 完全不生成服务，而 EmptyServiceAttribute 生成空白服务供手动扩展</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>需要完全自定义服务方法，不需要标准 CRUD 操作</description></item>
/// <item><description>只读的数据模型，仅需要查询方法而不需要增删改</description></item>
/// <item><description>特殊业务逻辑的服务，标准 CRUD 不适用</description></item>
/// <item><description>聚合服务，整合多个领域模型的数据</description></item>
/// <item><description>工具类服务，不对应具体的数据表</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：统计分析服务，不需要标准 CRUD
/// [EmptyService]
/// [EmptyController]
/// public class StatisticsReport : BaseDomain
/// {
///     public string ReportName { get; set; }
///     public DateTime GenerateTime { get; set; }
/// }
/// 
/// // 生成的服务接口：
/// public partial interface IStatisticsReportService : IBaseService
/// {
///     // 空白接口，可以手动添加自定义方法
/// }
/// 
/// // 生成的服务实现：
/// public partial class StatisticsReportServiceImpl : BaseServiceImpl&lt;IStatisticsReportRepository, StatisticsReport, I{ModuleName}UnitOfWork&gt;, IStatisticsReportService
/// {
///     // 空白实现，可以手动添加自定义方法
///     // 例如：
///     // public async Task&lt;ReportDTO&gt; GenerateMonthlyReportAsync(int year, int month)
///     // {
///     //     // 自定义业务逻辑
///     //     var data = await DefaultRepository.FindAsync(...);
///     //     // 数据处理和聚合
///     //     return reportDTO;
///     // }
/// }
/// 
/// // 示例2：聚合服务，整合多个领域的数据
/// [EmptyService]
/// [NotRepository]  // 不需要仓储
/// public class DashboardService : BaseDomain
/// {
///     // 该服务会调用多个其他服务来聚合数据
/// }
/// 
/// // 对比：标准服务（不使用 EmptyService）
/// public class Product : BaseDomain
/// {
///     public string Name { get; set; }
///     public decimal Price { get; set; }
/// }
/// 
/// // 会生成完整的 CRUD 服务：
/// public partial interface IProductService : IBaseService&lt;AddProductModel, EditProductModel, QueryProductModel, ProductDTO, ProductListDTO&gt;
/// {
///     // 包含标准的 Add, Edit, Delete, GetInfo, GetList 等方法
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EmptyServiceAttribute : Attribute { }
