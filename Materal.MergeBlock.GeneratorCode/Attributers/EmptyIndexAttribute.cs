namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 空白位序特性
/// 用于标记索引域（IIndexDomain）不生成位序交换相关的代码，即使领域模型实现了索引接口
/// </summary>
/// <remarks>
/// <para><b>应用于类时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorIControllerCodeAsync"/> 中：
/// 不会在控制器接口中生成 ExchangeIndexAsync 方法</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ControllerGeneratorCodePlug.GeneratorControllersCodeAsync"/> 中：
/// 不会在控制器实现中生成 ExchangeIndexAsync 方法</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorIServicesCodeAsync"/> 中：
/// 不会在服务接口中生成 ExchangeIndexAsync 方法</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.ServicesGeneratorCodePlug.GeneratorServiceImplsCodeAsync"/> 中：
/// 不会在服务实现中生成 ExchangeIndexAsync 方法及相关的 OnExchangeIndexBefore/After 钩子方法</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorIRepositoryCodeAsync"/> 中：
/// 不会在仓储接口中生成 GetMaxIndexAsync 方法</description>
/// </item>
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorRepositoryImplCodeAsync"/> 中：
/// 不会在仓储实现中生成 GetMaxIndexAsync 方法</description>
/// </item>
/// </list>
/// <para><b>前置条件：</b></para>
/// <list type="bullet">
/// <item><description>领域模型必须实现了 IIndexDomain 接口（包含 Index 属性）</description></item>
/// <item><description>如果领域模型未实现 IIndexDomain，此特性无效果</description></item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>索引域的位序由其他业务逻辑控制，不需要标准的位序交换功能</description></item>
/// <item><description>位序字段仅用于排序显示，不需要用户手动调整</description></item>
/// <item><description>位序由系统自动计算维护，不允许手动交换</description></item>
/// <item><description>需要自定义位序交换逻辑，不使用框架提供的标准实现</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 示例1：位序由创建时间自动生成，不允许手动调整
/// [EmptyIndex]
/// public class Article : BaseDomain, IIndexDomain
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
///     
///     // 位序字段存在，但不生成交换位序的方法
///     public int Index { get; set; }
/// }
/// 
/// // 示例2：位序由业务规则控制
/// [EmptyIndex]
/// public class MenuItem : BaseDomain, IIndexDomain
/// {
///     public string Name { get; set; }
///     public int Index { get; set; }
///     
///     // 位序由菜单管理模块统一控制，不使用标准的交换位序功能
/// }
/// 
/// // 对比：标准索引域（不使用 EmptyIndex）
/// public class Category : BaseDomain, IIndexDomain
/// {
///     public string Name { get; set; }
///     public int Index { get; set; }
///     
///     // 会生成完整的位序交换功能：
///     // - ICategoryController.ExchangeIndexAsync
///     // - ICategoryService.ExchangeIndexAsync
///     // - ICategoryRepository.GetMaxIndexAsync
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EmptyIndexAttribute : Attribute { }
