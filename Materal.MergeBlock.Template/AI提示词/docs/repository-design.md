# 仓储设计规范

本文档描述了在 Materal.MergeBlock 项目中设计仓储接口时需要遵循的规范。

> **相关文档**：
> - [实体设计规范](entity-design.md) - 实体定义
> - [服务实现规范](service-impl.md) - 工作单元使用说明

## 1 仓储继承体系

### 1.1 接口继承关系

```
IRepository
    ↓
IRepository<TEntity>
    ↓
IRepository<TEntity, TPrimaryKeyType>
    ↓
IEFRepository<TEntity, TPrimaryKeyType>
```

### 1.2 缓存仓储继承关系

```
IRepository
    ↓
ICacheRepository<T>
    ↓
ICacheEFRepository<TEntity, TPrimaryKeyType>
```

### 1.3 接口说明

| 接口 | 说明 | 命名空间 |
|------|------|----------|
| `IRepository` | 空标记接口，标识仓储 | Materal.TTA.Common |
| `IRepository<TEntity>` | 基础查询方法 | Materal.TTA.Common |
| `IRepository<TEntity, TPrimaryKeyType>` | 主键操作方法 | Materal.TTA.Common |
| `IEFRepository<TEntity, TPrimaryKeyType>` | EF 扩展，无额外方法 | Materal.TTA.EFRepository |
| `ICacheRepository<T>` | 缓存查询方法 | Materal.TTA.Common |
| `ICacheEFRepository<TEntity, TPrimaryKeyType>` | 缓存 + EF | Materal.TTA.EFRepository |

> **说明**：项目中使用的是 Guid 主键，因此仓储类型为 `IEFRepository<TEntity, Guid>`。

## 2 基本规范

### 2.1 命名规范

| 项目 | 规范 | 示例 |
|------|------|------|
| 仓储接口 | `I{EntityName}Repository` | `IUserRepository` |
| 文件位置 | `Repository/Repositories/` | - |
| 命名空间 | `{ProjectName}.{ModuleName}.Abstractions.Repositories` | `{ProjectName}.{ModuleName}.Abstractions.Repositories` |

### 2.2 文件结构

```
{ProjectName}.{ModuleName}.Abstractions/
└── Repositories/
    └── I{EntityName}Repository.cs        ← 自定义仓储接口
```

### 2.3 必须使用 partial

所有仓储接口必须使用 `partial` 关键字：

```csharp
namespace {ProjectName}.{ModuleName}.Abstractions.Repositories;

public partial interface IClassStudentRepository
{
    int GetStudentCount();
}
```

> **说明**：代码生成器会自动生成基础仓储接口（`I{EntityName}Repository`）和实现（`{EntityName}Repository`），自定义方法应编写在 `Repositories/` 目录下，不会被覆盖。

## 3 仓储接口方法

### 3.1 存在性检查

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `ExistedAsync(Expression<Func<T, bool>>)` | `Task<bool>` | 根据表达式判断是否存在 |
| `ExistedAsync(TPrimaryKeyType id)` | `Task<bool>` | 根据主键判断是否存在 |
| `ExistedAsync(FilterModel)` | `Task<bool>` | 根据 FilterModel 判断是否存在 |

### 3.2 计数

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `CountAsync(Expression<Func<T, bool>>)` | `Task<int>` | 根据表达式统计数量 |
| `CountAsync(FilterModel)` | `Task<int>` | 根据 FilterModel 统计数量 |

### 3.3 获取单条

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `FirstAsync(Expression<Func<T, bool>>)` | `Task<T>` | 获取第一条，无则抛异常 |
| `FirstAsync(TPrimaryKeyType id)` | `Task<T>` | 根据主键获取，无则抛异常 |
| `FirstAsync(FilterModel)` | `Task<T>` | 根据 FilterModel 获取，无则抛异常 |
| `FirstOrDefaultAsync(Expression<Func<T, bool>>)` | `Task<T?>` | 获取或返回默认 |
| `FirstOrDefaultAsync(TPrimaryKeyType id)` | `Task<T?>` | 主键获取或返回默认 |
| `FirstOrDefaultAsync(FilterModel)` | `Task<T?>` | FilterModel 获取或返回默认 |

### 3.4 查询多条

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `FindAsync(Expression<Func<T, bool>>)` | `Task<List<T>>` | 根据表达式查找所有匹配项 |
| `FindAsync(Expression, Expression<Func<T, object>>, SortOrder)` | `Task<List<T>>` | 带排序的查找 |
| `FindAsync(FilterModel)` | `Task<List<T>>` | 根据 FilterModel 查找 |
| `FindAsync(FilterModel, Expression, SortOrder)` | `Task<List<T>>` | FilterModel 带排序 |

### 3.5 范围查询

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `RangeAsync(Expression, long skip, long take)` | `Task<(List<T>, RangeModel)>` | 指定数量范围查询 |
| `RangeAsync(Expression, RangeRequestModel)` | `Task<(List<T>, RangeModel)>` | 使用 RangeRequestModel |
| `RangeAsync(Expression, Expression, SortOrder, RangeRequestModel)` | `Task<(List<T>, RangeModel)>` | 带排序的范围查询 |

### 3.6 分页查询

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `PagingAsync(PageRequestModel)` | `Task<(List<T>, PageModel)>` | 简单分页 |
| `PagingAsync(Expression, long pageIndex, long pageSize)` | `Task<(List<T>, PageModel)>` | 手动指定页码和大小 |
| `PagingAsync(Expression, Expression, SortOrder, PageRequestModel)` | `Task<(List<T>, PageModel)>` | 带排序的分页 |

### 3.7 RangeModel 和 PageModel

```csharp
// 范围模型
public class RangeModel
{
    public long Skip { get; set; }      // 跳过的数量
    public long Take { get; set; }      // 获取的数量
    public long Total { get; set; }     // 总数量
}

// 分页模型
public class PageModel
{
    public long PageIndex { get; set; } // 当前页码（从1开始）
    public long PageSize { get; set; }  // 每页大小
    public long Total { get; set; }     // 总数量
    public long PageCount { get; set; } // 总页数
}
```

## 4 自定义仓储接口

### 4.1 适用场景

当标准查询方法无法满足需求时，可以在仓储接口中添加自定义方法声明：

- 聚合查询（如 COUNT DISTINCT）
- 复杂的 JOIN 查询
- 分组统计
- 原生 SQL 查询

### 4.2 接口定义示例

```csharp
namespace {ProjectName}.{ModuleName}.Abstractions.Repositories;

public partial interface IClassStudentRepository
{
    /// <summary>
    /// 获得学生总数
    /// </summary>
    /// <returns>学生总数</returns>
    int GetStudentCount();

    /// <summary>
    /// 获得最后一次班级到期流水
    /// </summary>
    /// <param name="classID">班级唯一标识</param>
    /// <param name="dateTime">时间</param>
    /// <returns>班级到期流水</returns>
    ClassEndDateStatement GetLastClassEndDateStatement(Guid classID, DateTime dateTime);
}
```

## 5 缓存仓储

### 5.1 启用方式

在实体类上添加 `[Cache]` 特性：

```csharp
namespace {ProjectName}.{ModuleName}.Abstractions.Domain
{
    /// <summary>
    /// 字典
    /// </summary>
    [Cache]
    public class Dictionary : BaseDomain, IDomain
    {
        /// <summary>
        /// 键
        /// </summary>
        [Required, StringLength(50)]
        [Equal]
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 值
        /// </summary>
        [Required, StringLength(500)]
        public string Value { get; set; } = string.Empty;
    }
}
```

### 5.2 缓存方法

`ICacheRepository<T>` 提供的缓存操作接口：

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `GetAllInfoFromCache()` | `List<T>` | 同步 - 从缓存获取所有数据 |
| `GetAllInfoFromCacheAsync()` | `Task<List<T>>` | 异步 - 从缓存获取所有数据 |
| `GetInfoFromCache(string key)` | `List<T>` | 同步 - 按键获取数据 |
| `GetInfoFromCacheAsync(string key)` | `Task<List<T>>` | 异步 - 按键获取数据 |
| `ClearAllCache()` | `void` | 同步 - 清理全部缓存 |
| `ClearAllCacheAsync()` | `Task` | 异步 - 清理全部缓存 |
| `ClearCache(string key)` | `void` | 同步 - 清理指定缓存 |
| `ClearCacheAsync(string key)` | `Task` | 异步 - 清理指定缓存 |

### 5.3 适用场景

缓存仓储适用于以下场景：

- **读多写少**：数据频繁读取但很少修改
- **数据量小**：数据量不大，可以全部加载到内存
- **实时性要求低**：可以容忍一定程度的数据延迟

**典型应用**：

- 字典表（系统配置、枚举映射）
- 组织架构（部门、岗位）
- 静态数据（省份城市、分类目录）

## 6 禁止操作

- **不要**在 MGC 文件夹下编写自定义代码
- **不要**忘记使用 `partial` 关键字
- **不要**在仓储中编写业务逻辑（验证、计算等）
- **不要**在自定义 SQL 中拼接用户输入（必须参数化）

## 7 正确操作

- 自定义仓储放在 `Repository/Repositories/` 目录
- 遵循命名规范 `I{EntityName}Repository`
- 复杂查询使用原生 SQL 时必须参数化
- 合理使用缓存：读多写少的数据使用缓存仓储提升性能
