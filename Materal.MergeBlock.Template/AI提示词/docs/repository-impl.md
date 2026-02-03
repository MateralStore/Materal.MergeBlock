# 仓储实现规范

本文档描述 TTA 仓储框架中原生 SQL 的执行方式，以及实现时的相关规范。

> **相关文档**：
> - [仓储设计规范](repository-design.md) - 仓储接口设计
> - [编码规范](coding-style.md) - 通用编码标准

## 1 继承体系

### 1.1 类层次结构

```
CommonRepositoryImpl<TEntity>
    ├── GetConnectionString()      - 抽象方法，由子类实现
    ├── GetConnection(string)      - 抽象方法，由子类实现
    ├── ExecuteSql                  - 执行SQL语句（流式读取）
    ├── ExecuteNonQuery            - 执行非查询SQL
    ├── ExecuteQuerySql            - 查询SQL（非泛型）
    ├── ExecuteQuerySql<TModel>     - 查询SQL（泛型）
    │   └── BindData<TModel>       - 数据绑定
    │
    └── CommonRepositoryImpl<TEntity, TPrimaryKeyType>
        └── EFRepositoryImpl<TEntity, TPrimaryKeyType, TDBContext>
            └── CacheEFRepositoryImpl<TEntity, TPrimaryKeyType, TDBContext>
```

### 1.2 DBContext 与 DBSet 获取

EFRepositoryImpl 基类已封装：

```csharp
public abstract class EFRepositoryImpl<TEntity, TPrimaryKeyType, TDBContext>(TDBContext dbContext)
    : CommonRepositoryImpl<TEntity, TPrimaryKeyType>
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    protected TDBContext DBContext { get; } = dbContext;

    /// <summary>
    /// 实体对象
    /// </summary>
    protected virtual DbSet<TEntity> DBSet => DBContext.Set<TEntity>();
}
```

自定义仓储中直接使用：

```csharp
// LINQ 查询
var users = await DBSet.Where(u => u.Status == UserStatus.Active).ToListAsync();

// 主键查询
var user = await DBSet.FindAsync(id);
```

### 1.3 项目特定基类

各项目通常会定义自己的仓储基类：

```csharp
// 仓储接口基类
public interface IProjectRepository<TDomain> : ITTARepository<TDomain>
    where TDomain : IDomain, IEntity<Guid>, new()

// 仓储实现基类
public abstract class ProjectRepositoryImpl<TDomain>(ProjectDBContext dbContext)
    : TTARepositoryImpl<TDomain, ProjectDBContext>(dbContext)
    , IProjectRepository<TDomain>
    where TDomain : IDomain, IEntity<Guid>, new()
```

## 2 文件组织

### 2.1 目录结构

```
{ProjectName}.{ModuleName}.Repository/
├── Migrations/                   ← 迁移文件，不要修改
├── Repositories/                 ← 自定义仓储实现
│   └── {EntityName}RepositoryImpl.cs
└── MGC/                          ← 自动生成，不要修改
```

### 2.2 文件命名

| 项目 | 规范 | 示例 |
|------|------|------|
| 仓储实现 | `{EntityName}RepositoryImpl.cs` | `UserRepositoryImpl.cs` |
| 文件位置 | `Repository/Repositories/` | - |
| 命名空间 | `{ProjectName}.{ModuleName}.Repository` | - |

### 2.3 必须使用 partial

所有仓储实现类必须使用 `partial` 关键字，确保与代码生成器生成的代码正确合并：

```csharp
namespace {ProjectName}.{ModuleName}.Repository.Repositories;

public partial class {EntityName}RepositoryImpl : ProjectRepositoryImpl<{EntityName}>
{
    /// <summary>
    /// 根据名称查询
    /// </summary>
    public {EntityName}Dto? GetByName(string name)
    {
        // ...
    }
}
```

> **说明**：代码生成器会自动生成 `{EntityName}RepositoryImpl` 类的基础实现，自定义方法应编写在 `Repositories/` 目录下。

## 3 原生 SQL 执行

### 3.1 执行非查询 SQL（INSERT/UPDATE/DELETE）

```csharp
/// <summary>
/// 执行非查询SQL语句
/// </summary>
/// <param name="tSql">SQL语句</param>
/// <param name="sqlParameters">参数集合</param>
/// <returns>受影响行数</returns>
protected virtual int ExecuteNonQuery(string tSql, ICollection<IDataParameter> sqlParameters)
```

**示例**：
```csharp
public void UpdateName(Guid id, string newName)
{
    string sql = "UPDATE {TableName} SET Name = @Name WHERE ID = @ID";
    var parameters = new List<IDataParameter>
    {
        new SqlParameter("@Name", newName),
        new SqlParameter("@ID", id)
    };
    int affectedRows = ExecuteNonQuery(sql, parameters);
}
```

### 3.2 执行查询 SQL（返回多行）

#### 3.2.1 非泛型查询（返回 object[][]）

```csharp
/// <summary>
/// 执行查询SQL语句（非泛型）
/// </summary>
/// <param name="tSql">SQL语句</param>
/// <param name="sqlParameters">参数集合（可选）</param>
/// <param name="onHandler">每行数据的处理回调（可选）</param>
/// <returns>结果列表</returns>
protected virtual List<object[]> ExecuteQuerySql(
    string tSql,
    ICollection<IDataParameter>? sqlParameters = null,
    Func<IDataReader, object[]>? onHandler = null)
```

**示例**：
```csharp
// 简单查询
var results = ExecuteQuerySql("SELECT * FROM Users WHERE Age > @Age",
    new[] { new SqlParameter("@Age", 18) });

// 自定义行处理
var results = ExecuteQuerySql("SELECT ID, Name, Age FROM Users", null, dr =>
{
    return new object[]
    {
        dr.GetGuid(0),
        dr.GetString(1),
        dr.GetInt32(2)
    };
});
```

#### 3.2.2 泛型查询（返回 List<TModel>）

```csharp
/// <summary>
/// 执行查询SQL语句
/// </summary>
/// <param name="tSql">SQL语句</param>
/// <param name="sqlParameters">参数集合（可选）</param>
/// <param name="onHandler">每行数据的处理回调（可选）</param>
/// <returns>结果列表</returns>
protected virtual List<TModel> ExecuteQuerySql<TModel>(
    string tSql,
    ICollection<IDataParameter>? sqlParameters = null,
    Func<IDataReader, TModel>? onHandler = null)
    where TModel : new()
```

**示例**：
```csharp
// 自动绑定（列名需与属性名一致）
var users = ExecuteQuerySql<UserDto>("SELECT * FROM Users WHERE Age > @Age",
    new[] { new SqlParameter("@Age", 18) });

// 自定义绑定
var users = ExecuteQuerySql<UserDto>("SELECT ID, Name, Age FROM Users", null, dr =>
{
    return new UserDto
    {
        ID = dr.GetGuid(0),
        Name = dr.GetString(1),
        Age = dr.GetInt32(2)
    };
});
```

### 3.3 执行流式 SQL（IDataReader）

```csharp
/// <summary>
/// 执行SQL语句
/// </summary>
/// <param name="tSql">SQL语句</param>
/// <param name="sqlParameters">参数集合（可选）</param>
/// <param name="onHandler">数据读取回调</param>
protected virtual void ExecuteSql(
    string tSql,
    ICollection<IDataParameter>? sqlParameters = null,
    Action<IDataReader>? onHandler = null)
```

**示例**：
```csharp
// 大数据量流式处理
ExecuteSql("SELECT * FROM LargeTable", null, dr =>
{
    while (dr.Read())
    {
        var id = dr.GetGuid(0);
        var name = dr.GetString(1);
        // 处理每行数据
    }
});
```

### 3.4 数据绑定工具

#### 3.4.1 BindData - 绑定单行

```csharp
/// <summary>
/// 绑定数据
/// </summary>
/// <param name="sqlDataReader">数据读取器</param>
/// <returns>绑定后的模型</returns>
protected virtual TModel BindData<TModel>(IDataReader sqlDataReader)
    where TModel : new()
```

自动将 `IDataReader` 的当前行绑定到模型，规则：
- 列名与属性名**严格匹配**
- 支持 Nullable 类型
- 自动类型转换

**示例**：
```csharp
ExecuteSql("SELECT ID, Name, Age FROM Users", null, dr =>
{
    if (dr.Read())
    {
        var user = BindData<UserDto>(dr);
    }
});
```

#### 3.4.2 BindList - 绑定列表

```csharp
/// <summary>
/// 绑定列表
/// </summary>
/// <param name="sqlDataReader">数据读取器</param>
/// <param name="onHandler">自定义绑定回调（可选）</param>
/// <returns>绑定后的列表</returns>
protected virtual List<TModel> BindList<TModel>(
    IDataReader sqlDataReader,
    Func<IDataReader, TModel>? onHandler = null)
    where TModel : new()
```

## 4 参数化查询

始终使用参数化查询防止 SQL 注入：

```csharp
// ✅ 正确：使用参数
var sql = "SELECT * FROM Users WHERE Name = @Name AND Age > @Age";
var parameters = new List<IDataParameter>
{
    new SqlParameter("@Name", userName),
    new SqlParameter("@Age", minAge)
};
var users = ExecuteQuerySql<UserDto>(sql, parameters);

// ❌ 错误：字符串拼接（禁止使用）
var sql = $"SELECT * FROM Users WHERE Name = '{userName}'"; // SQL注入风险！
```

## 5 完整示例

```csharp
namespace {ProjectName}.{ModuleName}.Repository.Repositories;

public partial class {EntityName}RepositoryImpl : ProjectRepositoryImpl<{EntityName}>
{
    public {EntityName}RepositoryImpl(ProjectDBContext dbContext)
    : base(dbContext) { }

    /// <summary>
    /// 根据名称查询
    /// </summary>
    public {EntityName}Dto? GetByName(string name)
    {
        string sql = "SELECT * FROM {TableName} WHERE Name = @Name";
        var parameters = new List<IDataParameter>
        {
            new SqlParameter("@Name", name)
        };
        var results = ExecuteQuerySql<{EntityName}Dto>(sql, parameters);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// 批量更新状态
    /// </summary>
    public int BatchUpdateStatus(IEnumerable<Guid> ids, int status)
    {
        string sql = "UPDATE {TableName} SET Status = @Status, UpdateTime = @UpdateTime WHERE ID IN (@IDs)";
        var idList = string.Join(",", ids.Select(id => $"'{id}'"));
        var parameters = new List<IDataParameter>
        {
            new SqlParameter("@Status", status),
            new SqlParameter("@UpdateTime", DateTime.Now),
            new SqlParameter("@IDs", idList)
        };
        return ExecuteNonQuery(sql, parameters);
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public StatisticsDto GetStatistics()
    {
        string sql = @"
            SELECT
                COUNT(*) AS TotalCount,
                SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS ActiveCount
            FROM {TableName}";
        var results = ExecuteQuerySql<StatisticsDto>(sql);
        return results.FirstOrDefault() ?? new StatisticsDto();
    }
}
```

## 6 注意事项

### 6.1 连接管理

- 方法内部自动管理连接的开闭，无需手动处理
- 使用 `using` 确保连接正确释放

### 6.2 参数类型

- 使用 `IDataParameter` 接口
- 具体实现因数据库而异：
  - SQL Server: `System.Data.SqlClient.SqlParameter`
  - MySQL: `MySql.Data.MySqlClient.MySqlParameter`

### 6.3 列名匹配

- 自动绑定时，SQL 列名需与模型属性名严格匹配
- 使用 `SELECT *` 时，确保列名与属性名一致

### 6.4 大数据量

- 处理大数据量时使用 `ExecuteSql` 流式读取
- 避免使用 `ExecuteQuerySql` 一次性加载大量数据

### 6.5 事务

- 如需事务支持，应在调用方处理或扩展基类方法

## 7 禁止操作

| 禁止项 | 说明 |
|--------|------|
| ❌ 在 MGC 文件夹下编写代码 | 代码生成时会删除重建 |
| ❌ 不使用 `partial` 关键字 | 会导致与生成代码冲突 |
| ❌ 拼接用户输入 | 必须使用参数化查询 |
| ❌ 编写业务逻辑 | 仓储只负责数据访问 |

## 8 正确操作

| 推荐项 | 说明 |
|--------|------|
| ✅ 自定义仓储放 `Repositories/` 目录 | 不会被代码生成器覆盖 |
| ✅ 遵循命名规范 | `I{EntityName}Repository`, `{EntityName}RepositoryImpl` |
| ✅ 复杂查询使用参数化 | 防止 SQL 注入 |
| ✅ 合理使用缓存仓储 | 读多写少的数据使用缓存提升性能 |
| ✅ 添加 XML 文档注释 | 公开 API 必须添加 |

## 9 参考资料

- [仓储设计规范](repository-design.md) - 仓储接口设计
- [编码规范](coding-style.md) - 通用编码标准
- [服务实现规范](service-impl.md) - 工作单元使用说明
