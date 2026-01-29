namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 空白控制器特性
/// 用于标记领域模型生成不包含标准 CRUD 方法的空白控制器，仅保留控制器基础结构
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorIControllerCodeAsync"/> 中：
/// 生成的控制器接口将继承 IMergeBlockController 而不是 IMergeBlockController&lt;TAdd, TEdit, TQuery, TDTO, TListDTO&gt;</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorControllersCodeAsync"/> 中：
/// 生成的控制器实现将继承简化的基类，不包含标准的增删改查方法</description>
/// </item>
/// </list>
/// <para><b>与其他特性的关系：</b></para>
/// <list type="bullet">
/// <item>
/// <description><see cref="EmptyServiceAttribute"/>：通常与 EmptyServiceAttribute 配合使用，同时简化服务层和控制器层</description>
/// </item>
/// <item>
/// <description><see cref="NotControllerAttribute"/>：NotControllerAttribute 完全不生成控制器，而 EmptyControllerAttribute 生成空白控制器供手动扩展</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>需要完全自定义控制器方法，不需要标准 CRUD 操作</description></item>
/// <item><description>只读的数据模型，仅需要查询方法而不需要增删改</description></item>
/// <item><description>特殊业务逻辑的控制器，标准 CRUD 不适用</description></item>
/// <item><description>工具类控制器，不对应具体的数据表</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：统计分析控制器，不需要标准 CRUD
/// [EmptyController]
/// [EmptyService]
/// public class StatisticsReport : BaseDomain
/// {
///     public string ReportName { get; set; }
///     public DateTime GenerateTime { get; set; }
/// }
/// 
/// // 生成的控制器接口：
/// public partial interface IStatisticsReportController : IMergeBlockController
/// {
///     // 空白接口，可以手动添加自定义方法
/// }
/// 
/// // 生成的控制器实现：
/// public partial class StatisticsReportController : {ModuleName}Controller&lt;IStatisticsReportService&gt;, IStatisticsReportController
/// {
///     // 空白实现，可以手动添加自定义方法
///     // 例如：
///     // [HttpGet]
///     // public async Task&lt;ResultModel&lt;ReportDTO&gt;&gt; GenerateReport(DateTime startDate, DateTime endDate)
///     // {
///     //     // 自定义实现
///     // }
/// }
/// 
/// // 示例2：只读配置控制器
/// [EmptyController]
/// public class SystemConfig : BaseDomain
/// {
///     public string ConfigKey { get; set; }
///     public string ConfigValue { get; set; }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EmptyControllerAttribute : Attribute { }
