namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 数据传输模型文本特性
/// 用于标记枚举类型属性，在 DTO 中自动生成对应的文本属性，用于显示枚举的描述信息
/// </summary>
/// <remarks>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.DTOGeneratorCodePlug"/> 中：
/// 会为标记的枚举属性自动生成一个 {PropertyName}Text 只读属性，返回枚举的 Description 特性值</description>
/// </item>
/// </list>
/// <para><b>生成的文本属性特点：</b></para>
/// <list type="bullet">
/// <item><description>属性名称为原属性名加 "Text" 后缀（如 Status → StatusText）</description></item>
/// <item><description>如果原属性可空，生成的文本属性调用 GetDescriptionOrNull() 方法，返回 string?</description></item>
/// <item><description>如果原属性不可空，生成的文本属性调用 GetDescription() 方法，返回 string</description></item>
/// <item><description>文本属性为只读计算属性，无需手动赋值</description></item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>前端需要显示枚举的中文描述而不是枚举值</description></item>
/// <item><description>导出 Excel 时需要显示枚举的可读文本</description></item>
/// <item><description>API 返回数据时同时提供枚举值和文本描述</description></item>
/// <item><description>减少前端枚举映射的工作量</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// // 定义枚举
/// public enum OrderStatus
/// {
///     [Description("待支付")]
///     Pending = 0,
///     
///     [Description("已支付")]
///     Paid = 1,
///     
///     [Description("已完成")]
///     Completed = 2
/// }
/// 
/// // 在领域模型中使用
/// public class Order : BaseDomain
/// {
///     [DTOText]
///     public OrderStatus Status { get; set; }
/// }
/// 
/// // 生成的 DTO 将包含：
/// public class OrderListDTO : IListDTO
/// {
///     public OrderStatus Status { get; set; }
///     
///     // 自动生成的文本属性
///     public string StatusText => Status.GetDescription();  // 返回 "待支付"、"已支付" 等
/// }
/// 
/// // 可空枚举示例
/// public class Task : BaseDomain
/// {
///     [DTOText]
///     public TaskPriority? Priority { get; set; }
/// }
/// 
/// // 生成的 DTO：
/// public class TaskListDTO : IListDTO
/// {
///     public TaskPriority? Priority { get; set; }
///     
///     // 可空枚举生成可空文本
///     public string? PriorityText => Priority?.GetDescriptionOrNull();
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class DTOTextAttribute : Attribute
{
}
