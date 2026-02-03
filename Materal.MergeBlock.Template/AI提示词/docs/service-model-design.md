# 服务模型设计规范

本文档描述了在 Materal.MergeBlock 项目中设计服务模型（ServiceModel）时需要遵循的规范。

> **相关文档**：[服务设计规范](service-design.md) - 服务接口设计

## 基本规范

### 命名规范

| 用途 | 命名格式 | 示例 |
|------|----------|------|
| 添加模型 | `Add{EntityName}Model` | `AddUserModel` |
| 编辑模型 | `Edit{EntityName}Model` | `EditUserModel` |
| 查询模型 | `Query{EntityName}Model` | `QueryUserModel` |
| 自定义模型 | `{Operation}{EntityName}Model` | `ChangePasswordModel` |

### 文件位置

```
{ModuleName}.Abstractions/
└── Services/
    └── Models/
        └── {EntityName}/
            └── {Operation}Model.cs  ← 服务模型
```

### 必须使用 partial

所有服务模型必须使用 `partial class`：

```csharp
public partial class ChangePasswordModel { }
```

### 必须继承的接口

| 模型类型 | 继承接口 |
|----------|----------|
| 添加模型 | `IAddServiceModel` |
| 编辑模型 | `IEditServiceModel` |
| 查询模型 | `IQueryServiceModel` |

代码生成器会自动添加这些继承，无需手动指定。

## 服务模型定义

### 添加模型

```csharp
namespace {ProjectName}.{ModuleName}.Abstractions.Services.Models.User;

/// <summary>
/// 添加用户模型
/// </summary>
public partial class AddUserModel
{
    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "账号为空")]
    [StringLength(20, MinimumLength = 4)]
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 昵称
    /// </summary>
    [Required(ErrorMessage = "昵称为空")]
    [StringLength(20)]
    public string Nickname { get; set; } = string.Empty;
}
```

### 编辑模型

```csharp
namespace {ProjectName}.{ModuleName}.Abstractions.Services.Models.User;

/// <summary>
/// 编辑用户模型
/// </summary>
public partial class EditUserModel
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    [Required]
    public Guid ID { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [Required(ErrorMessage = "昵称为空")]
    [StringLength(20)]
    public string Nickname { get; set; } = string.Empty;
}
```

### 查询模型

```csharp
namespace {ProjectName}.{ModuleName}.Abstractions.Services.Models.User;

/// <summary>
/// 查询用户模型
/// </summary>
public partial class QueryUserModel
{
    /// <summary>
    /// 账号（精确匹配）
    /// </summary>
    [Equal]
    public string? Account { get; set; }

    /// <summary>
    /// 昵称（模糊搜索）
    /// </summary>
    [Contains]
    public string? Nickname { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Equal]
    public UserStatus? Status { get; set; }
}
```

### 自定义模型

自定义模型用于定义非标准的业务方法参数。根据查询需求，可以选择继承不同的基类。

#### 继承关系

```
FilterModel (Materal.Utils.Models)
    ↓
RangeRequestModel (Materal.Utils.Models)
    ↓
PageRequestModel (Materal.Utils.Models)
```

#### 何时继承 FilterModel

当需要在服务实现中使用仓储的 `Find(FilterModel)`、`Range(RangeRequestModel)` 等便捷方法，或需要通过 `GetSearchExpression<T>()` 自动生成 Lambda 查询表达式时：

**FilterModel 提供的便捷方法**：

| 方法 | 说明 |
|------|------|
| `GetSearchExpression<T>()` | 根据查询特性自动生成 Lambda 表达式 |
| `GetSearchDelegate<T>()` | 生成委托用于内存查询 |
| `GetSortExpression<T>()` | 生成排序 Lambda 表达式 |
| `SetSortExpression<T>(IQueryable)` | 应用排序到 IQueryable |

```csharp
// 使用 FilterModel 的便捷方法
public async Task<List<User>> GetUserListAsync(GetUserListModel model)
{
    Expression<Func<User, bool>> expression = model.GetSearchExpression<User>();
    List<User> users = await _repository.FindAsync(expression);
    return users;
}
```

```csharp
/// <summary>
/// 获取用户列表模型（需要使用仓储的 FilterModel 查询方法）
/// </summary>
public partial class GetUserListModel : FilterModel
{
    /// <summary>
    /// 账号（精确匹配）
    /// </summary>
    [Equal]
    public string? Account { get; set; }

    /// <summary>
    /// 昵称（模糊搜索）
    /// </summary>
    [Contains]
    public string? Nickname { get; set; }
}
```

**配合仓储方法使用**：

```csharp
// 仓储提供的方法
List<TEntity> Find(FilterModel filterModel);
Task<List<TEntity>> FindAsync(FilterModel filterModel);
RangeModel Range(RangeRequestModel rangeRequestModel);
```

#### 何时继承 RangeRequestModel

当需要返回部分数据而非全部时：

```csharp
/// <summary>
/// 获取统计数据模型（需要范围查询）
/// </summary>
public partial class GetStatisticsModel : RangeRequestModel
{
    /// <summary>
    /// 开始时间
    /// </summary>
    [GreaterThanOrEqual]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [LessThanOrEqual]
    public DateTime? EndTime { get; set; }
}
```

**配合仓储方法使用**：

```csharp
// 仓储提供的方法
(List<TEntity> data, RangeModel rangeInfo) Range(RangeRequestModel rangeRequestModel);
Task<(List<TEntity> data, RangeModel rangeInfo)> RangeAsync(RangeRequestModel rangeRequestModel);
```

#### 何时继承 PageRequestModel

当需要分页查询时：

```csharp
/// <summary>
/// 分页查询模型
/// </summary>
public partial class GetUserPageModel : PageRequestModel
{
    /// <summary>
    /// 账号（精确匹配）
    /// </summary>
    [Equal]
    public string? Account { get; set; }

    /// <summary>
    /// 昵称（模糊搜索）
    /// </summary>
    [Contains]
    public string? Nickname { get; set; }
}
```

**配合仓储方法使用**：

```csharp
// 仓储提供的方法
(List<TEntity> data, PageModel pageInfo) Paging(PageRequestModel pageRequestModel);
Task<(List<TEntity> data, PageModel pageInfo)> PagingAsync(PageRequestModel pageRequestModel);
```

#### 不继承任何基类

当方法不需要使用仓储的 FilterModel 查询功能时：

```csharp
/// <summary>
/// 修改密码模型（无查询功能，无需继承）
/// </summary>
public partial class ChangePasswordModel
{
    /// <summary>
    /// 旧密码
    /// </summary>
    [Required(ErrorMessage = "旧密码为空")]
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密码
    /// </summary>
    [Required(ErrorMessage = "新密码为空")]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
}
```

> **重要**：验证特性和查询特性只有在继承 `FilterModel`（或其子类）时才有效，用于仓储的 `Find(FilterModel)`、`Range(RangeRequestModel)`、`Paging(PageRequestModel)` 等便捷方法自动生成查询条件。不继承 `FilterModel` 的模型，这些特性不会产生任何效果。

## 验证特性

### 常用验证特性

| 特性 | 说明 | 示例 |
|------|------|------|
| `[Required]` | 必填字段 | `[Required(ErrorMessage = "名称为空")]` |
| `[StringLength]` | 字符串长度 | `[StringLength(50)]` |
| `[Range]` | 数值范围 | `[Range(0, 100)]` |
| `[Min]` | 最小值 | `[Min(0)]` |
| `[Max]` | 最大值 | `[Max(100)]` |
| `[EmailAddress]` | 邮箱格式 | `[EmailAddress]` |
| `[RegularExpression]` | 正则验证 | `[RegularExpression(@"^\d+$")]` |

### 查询特性

用于查询模型，控制生成查询条件：

| 特性 | 说明 | 生成内容 |
|------|------|----------|
| `[Equal]` | 等值匹配 | 单个查询参数 |
| `[Contains]` | 模糊搜索 | 单个查询参数 |
| `[StartContains]` | 开头匹配 | 单个查询参数 |
| `[Between]` | 范围查询 | Min/Max 两个参数 |
| `[GreaterThan]` | 大于 | 单个查询参数 |
| `[GreaterThanOrEqual]` | 大于等于 | 单个查询参数 |
| `[LessThan]` | 小于 | 单个查询参数 |
| `[LessThanOrEqual]` | 小于等于 | 单个查询参数 |

## 特殊特性

### LoginUserID 特性

用于 Guid 字段，自动赋值为当前登录用户 ID：

```csharp
/// <summary>
/// 创建订单模型
/// </summary>
public partial class CreateOrderModel
{
    /// <summary>
    /// 订单名称
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 创建人ID（自动填充当前登录用户ID）
    /// </summary>
    [LoginUserID]
    public Guid CreateUserID { get; set; }
}
```

**注意**：
- `[LoginUserID]` 特性只能用于 `Guid` 类型的字段
- 框架会自动从当前请求上下文中获取登录用户 ID 并赋值

### DTOText 特性

用于枚举类型字段，自动生成描述文本属性：

```csharp
/// <summary>
/// 用户状态
/// </summary>
[Required]
[DTOText]
public UserStatus Status { get; set; }
```

## 禁止操作

- **不要**在 MGC 文件夹下编写任何代码
- **不要**忘记使用 `partial` 关键字
- **不要**在服务模型中使用 HTTP 特定类型（如 `IFormFile`）
- **不要**手动添加 `IAddServiceModel`、`IEditServiceModel`、`IQueryServiceModel` 继承

## 正确操作

- 服务模型放在 `Services/Models/{EntityName}/` 目录
- 服务模型使用 `partial class`
- 为每个属性添加 XML 文档注释
- 使用 `[Required]`、`[StringLength]` 等验证特性
- 查询模型使用 `[Equal]`、`[Contains]` 等查询特性
