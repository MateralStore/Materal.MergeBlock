# 控制器接口设计规范

本文档指导在 Materal.MergeBlock 项目中设计控制器接口时遵循的规范。

## 控制器类型

### 1. 标准 CRUD 控制器接口（自动生成）

框架为所有实体自动生成 CRUD 控制器接口，除非实体标记了 `[NotController]` 特性：

```csharp
// 实体定义 - 默认自动生成控制器接口
public partial class User : IDomain<Guid>
{
    public string Name { get; set; }
}

// 标记 [NotController] 后不会生成控制器接口
[NotController]
public partial class InternalLog : IDomain<Guid>
{
    public string Content { get; set; }
}
```

**自动生成**：控制器接口文件 `I{Entity}Controller.cs`，位于 `MGC/Controllers/` 目录

### 2. 使用 [MapperController] 特性的服务接口

使用 `[MapperController]` 特性标记的服务方法，代码生成器自动生成控制器接口：

```csharp
public partial interface IUserService
{
    [MapperController(MapperType.Get)]
    Task<string> GetInviteCodeAsync();
}
```

**自动生成**：
- 控制器接口：`MGC/Controllers/I{Entity}Controller.Mapper.cs`

#### 无实体服务的特殊处理

**有实体**的服务：代码生成器会自动创建控制器实现，无需手动处理。

**无实体**的服务（如认证服务）：代码生成器**不会**自动创建控制器实现，需要**手动创建**：

```csharp
// Application/Controllers/AuthController.cs
namespace {ProjectName}.{ModuleName}.Application.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
public partial class AuthController : MainController<IAuthService>
{
}
```

| 服务类型 | 控制器实现 | 说明 |
|----------|------------|------|
| 有实体服务 | 代码生成器自动生成 | 位于 `MGC/Controllers/` |
| 无实体服务 | **需要手动创建** | 位于 `Application/Controllers/` |

### 3. 自定义控制器接口（手动创建）

以下情况需要手动创建控制器接口：

| 场景 | 说明 |
|------|------|
| 实体标记 `[NotController]` | 实体不需要标准 CRUD，但需要自定义 API |
| 非 IBaseService 服务 | 配置类、工具类等不继承 IBaseService 的服务 |

### 何时需要创建自定义控制器接口

框架会自动为以下情况生成控制器接口：

| 情况 | 自动生成 | 需要手动创建 |
|------|----------|--------------|
| 普通实体 | ✅ `I{Entity}Controller.cs` | ❌ |
| 实体标记 `[NotController]` | ❌ | ✅ 如需 API 则手动创建 |
| 服务使用 `[MapperController]` | ✅ `I{Entity}Controller.Mapper.cs` | ❌ |
| 不继承 IBaseService 的服务 | ❌ | ✅ 需要完整自定义 |

**注意**：手动创建的控制器接口应放在 `Controllers/` 目录（非 MGC 子目录），文件名为 `I{Entity}Controller.cs`。

## 文件位置

| 类型 | 位置 |
|------|------|
| Controller 接口 | `{ModuleName}.Abstractions/Controllers/I{Entity}Controller.cs` |

## 接口设计规范

### 基本规范

- **必须使用 `partial interface`**
- **必须继承 `IMergeBlockController`** 或其泛型版本
- 方法需要添加 `[HttpGet]`、`[HttpPost]` 等 HTTP 特性

### 接口定义示例

```csharp
using Materal.MergeBlock.Web.Abstractions.Controllers;

namespace {ProjectName}.{ModuleName}.Abstractions.Controllers;

/// <summary>
/// 应用程序配置控制器
/// </summary>
public partial interface IApplicationController : IMergeBlockController
{
    /// <summary>
    /// 获取邀请策略
    /// </summary>
    [HttpGet]
    Task<ResultModel<int>> GetInviteStrategyAsync();
}
```

### 继承关系

| 基类 | 用途 |
|------|------|
| `IMergeBlockController` | 基础接口，无泛型约束 |
| `IMergeBlockController<TAdd, TEdit, TQuery, TDTO, TListDTO>` | 标准 CRUD 控制器接口 |

### 泛型版本的接口方法

继承 `IMergeBlockController<...>` 泛型接口时，默认包含以下方法：

```csharp
[HttpPost]
Task<ResultModel<Guid>> AddAsync(TAddRequestModel requestModel);

[HttpPut]
Task<ResultModel> EditAsync(TEditRequestModel requestModel);

[HttpDelete]
Task<ResultModel> DeleteAsync([Required(ErrorMessage = "唯一标识为空")] Guid id);

[HttpGet]
Task<ResultModel<TDTO>> GetInfoAsync([Required(ErrorMessage = "唯一标识为空")] Guid id);

[HttpPost]
Task<CollectionResultModel<TListDTO>> GetListAsync(TQueryRequestModel requestModel);
```

## [MapperController] 特性

用于标记服务层方法自动生成对应的控制器接口方法。

### 特性定义

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class MapperControllerAttribute(MapperType type) : Attribute
{
    public MapperType Type { get; } = type;
    public bool IsAllowAnonymous { get; set; } = false;
}
```

### MapperType 枚举

| 值 | HTTP 方法 | 用途 |
|----|-----------|------|
| `Get` | `[HttpGet]` | 查询操作 |
| `Post` | `[HttpPost]` | 创建操作 |
| `Put` | `[HttpPut]` | 完整更新 |
| `Delete` | `[HttpDelete]` | 删除操作 |
| `Patch` | `[HttpPatch]` | 部分更新 |

### 使用示例

```csharp
public partial interface IUserService
{
    /// <summary>
    /// 修改密码
    /// </summary>
    [MapperController(MapperType.Put)]
    Task ChangePasswordAsync(ChangePasswordModel model);

    /// <summary>
    /// 获取邀请码（允许匿名访问）
    /// </summary>
    [MapperController(MapperType.Get, IsAllowAnonymous = true)]
    Task<string> GetInviteCodeAsync();
}
```

### 自动生成的接口效果

Service 接口中的方法标记 `[MapperController]` 后，代码生成器会自动在控制器接口中生成：

```csharp
// 自动生成的控制器接口
public partial interface IUserController : IMergeBlockController
{
    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPut]
    Task<ResultModel> ChangePasswordAsync(ChangePasswordRequestModel model);

    /// <summary>
    /// 获取邀请码
    /// </summary>
    [HttpGet, AllowAnonymous]
    Task<ResultModel<string>> GetInviteCodeAsync();
}
```

### 自动处理逻辑

使用 `[MapperController]` 特性时，框架自动在生成的接口中添加：

| 处理项 | 说明 |
|--------|------|
| HTTP 特性 | 根据 `MapperType` 添加 `[HttpGet]`、`[HttpPost]` 等 |
| 匿名访问 | `IsAllowAnonymous = true` 时添加 `[AllowAnonymous]` |
| 返回类型包装 | 根据 Service 返回类型自动包装：`Task` → `ResultModel`、`Task<T>` → `ResultModel<T>`、`Task<(List, RangeModel)>` → `CollectionResultModel<T>` |

## 返回类型规范

### 控制器接口返回类型

| 返回类型 | 用途 |
|---------|------|
| `Task<ResultModel>` | 无返回数据的操作（如启用、禁用） |
| `Task<ResultModel<T>>` | 返回单个对象（如详情、Token、不分页列表） |
| `Task<CollectionResultModel<T>>` | 返回分页集合列表|

### 与 Service 层返回类型的对应关系

| Service 层返回 | Controller 接口返回 |
|----------------|---------------------|
| `Task` | `Task<ResultModel>` |
| `Task<T>` | `Task<ResultModel<T>>` |
| `Task<(List<T> data, RangeModel rangeInfo)>` | `Task<CollectionResultModel<T>>` |

```csharp
// Service 层 - 返回原始类型
public partial interface IUserService
{
    [MapperController(MapperType.Get)]
    Task<int> GetInviteRewardAsync();  // 返回 int

    [MapperController(MapperType.Get)]
    Task<string> GetInviteCodeAsync();  // 返回 string

    [MapperController(MapperType.Post)]
    Task<UserInfoDTO> GetUserInfoAsync();  // 返回 DTO
}

// Controller 接口层 - 自动包装为 ResultModel
public partial interface IUserController : IMergeBlockController
{
    [HttpGet]
    Task<ResultModel<int>> GetInviteRewardAsync();

    [HttpGet]
    Task<ResultModel<string>> GetInviteCodeAsync();

    [HttpPost]
    Task<ResultModel<UserInfoDTO>> GetUserInfoAsync();
}
```

## 特殊场景

### IndexDomain 实体

实现 `IIndexDomain` 接口的实体会自动生成交换位序方法：

```csharp
public partial interface I{Entity}Controller : IMergeBlockController<..., ...>
{
    /// <summary>
    /// 交换位序
    /// </summary>
    [HttpPut]
    Task<ResultModel> ExchangeIndexAsync(ExchangeIndexRequestModel requestModel);
}
```

### TreeDomain 实体

实现 `ITreeDomain` 接口的实体会自动生成树相关方法：

```csharp
public partial interface I{Entity}Controller : IMergeBlockController<..., ...>
{
    /// <summary>
    /// 更改父级
    /// </summary>
    [HttpPut]
    Task<ResultModel> ExchangeParentAsync(ExchangeParentRequestModel requestModel);

    /// <summary>
    /// 查询树列表
    /// </summary>
    [HttpPost]
    Task<ResultModel<List<{Entity}TreeListDTO>>> GetTreeListAsync(Query{Entity}TreeListRequestModel requestModel);
}
```

## 认证与授权

### 允许匿名访问

在控制器接口方法上添加 `[AllowAnonymous]` 特性：

```csharp
public partial interface IAuthController : IMergeBlockController
{
    /// <summary>
    /// 登录
    /// </summary>
    [HttpPost, AllowAnonymous]
    Task<ResultModel<TokenDTO>> LoginAsync(LoginRequestModel model);
}
```

使用 `[MapperController]` 时，通过 `IsAllowAnonymous = true` 参数：

```csharp
[MapperController(MapperType.Post, IsAllowAnonymous = true)]
Task<TokenDTO> LoginAsync(LoginModel model);
```

## 注意事项

1. **必须使用 `partial`**：Controller 接口必须使用 `partial` 关键字
2. **不要在 MGC 目录编写代码**：代码生成时会覆盖 MGC 目录下的文件

## 文件放置总结

| 类型 | 位置 |
|------|------|
| Controller 接口 | `{ModuleName}.Abstractions/Controllers/I{Entity}Controller.cs` |

## 禁止操作

- ❌ 不要在 MGC 文件夹下编写任何代码
- ❌ 不要忘记使用 `partial` 关键字
