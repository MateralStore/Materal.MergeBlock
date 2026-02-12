namespace Materal.MergeBlock.GeneratorCode.Attributers;

/// <summary>
/// 列类型特性
/// 用于指定实体属性在数据库中的列类型，覆盖 EF Core 的默认类型映射
/// </summary>
/// <remarks>
/// <para><b>应用于属性时的影响：</b></para>
/// <list type="bullet">
/// <item>
/// <description>在 <see cref="DefaultPlug.RepositoryGeneratorCodePlug.GeneratorEntityConfigCodeAsync"/> 中：
/// 生成的实体配置类会调用 HasColumnType() 方法设置数据库列类型</description>
/// </item>
/// </list>
/// <para><b>典型使用场景：</b></para>
/// <list type="bullet">
/// <item><description>需要使用特定精度的 decimal 类型（如 decimal(18,2)）</description></item>
/// <item><description>需要使用特定长度的字符串类型（如 varchar(max)、nvarchar(max)）</description></item>
/// <item><description>需要使用数据库特定类型（如 SQL Server 的 money、datetime2 等）</description></item>
/// <item><description>需要使用 JSON、XML 等特殊数据类型</description></item>
/// <item><description>需要精确控制数值类型的存储格式</description></item>
/// </list>
/// <para><b>注意事项：</b></para>
/// <list type="bullet">
/// <item><description>SqlType 参数值会直接传递给 EF Core 的 HasColumnType() 方法</description></item>
/// <item><description>不同数据库的类型名称可能不同，需要根据实际使用的数据库选择正确的类型</description></item>
/// <item><description>类型名称通常需要使用字符串字面量（如 "decimal(18,2)"）</description></item>
/// </list>
/// <para><b>使用示例：</b></para>
/// <code>
/// public class Product : BaseDomain
/// {
///     // 指定价格字段使用 decimal(18,2) 类型
///     [ColumnType("decimal(18,2)")]
///     public decimal Price { get; set; }
///     
///     // 指定描述字段使用 nvarchar(max) 类型
///     [ColumnType("nvarchar(max)")]
///     public string Description { get; set; }
///     
///     // 指定创建时间使用 datetime2 类型（SQL Server）
///     [ColumnType("datetime2")]
///     public DateTime CreateTime { get; set; }
///     
///     // 指定 JSON 数据类型（PostgreSQL）
///     [ColumnType("jsonb")]
///     public string Metadata { get; set; }
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ColumnTypeAttribute(string sqlType) : Attribute
{
    /// <summary>
    /// SQL类型
    /// </summary>
    public string SqlType { get; private set; } = sqlType;
}
