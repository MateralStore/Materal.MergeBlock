# 服务设计规范

本文档描述了在 Materal.MergeBlock 项目中设计服务接口（IService）时需要遵循的规范。

> **相关文档**：[服务模型设计规范](service-model-design.md) - 服务模型属性定义

## 服务类型

### 1. 实体服务

继承泛型 `IBaseService<...>` 接口的服务，用于实体的 CRUD 操作：

```csharp
public partial interface IUserService : IBaseService<AddUserModel, EditUserModel, QueryUserModel, UserDTO, UserListDTO>
{
}
```

### 2. 自定义服务

需要继承非泛型的 `IBaseService`，用于获取当前登录用户信息：

```csharp
public partial interface IStatisticsService : IBaseService
{
    /// <summary>
    /// 获取统计数据
    /// </summary>
    Task<StatisticsDTO> GetStatisticsAsync();
}
```

## 基本规范

### 命名规范

| 项目 | 规范 | 示例 |
|------|------|------|
| 服务接口 | `I{EntityName}Service` 或 `I{Feature}Service` | `IUserService`、`IStatisticsService` |

### 文件位置

```
{ModuleName}.Abstractions/
└── Services/
    └── I{EntityName}Service.cs      ← 服务接口（包含所有方法）
```

### 文件组织规则

当服务接口方法较少时（如少于 5 个自定义方法），所有方法应直接放在主文件中，无需使用 `.Custom`、`.Extensions` 等辅助后缀：

```csharp
// 正确：方法直接定义在主文件中
public partial interface IUserService : IBaseService<...>
{
    Task<UserDTO> GetByAccountAsync(string account);
    Task ResetPasswordAsync(ResetPasswordModel model);
}
```

只有当服务接口方法非常多、需要按职责拆分时，才考虑拆分到多个文件。拆分时应使用描述性的文件名，如 `I{EntityName}Service.Auth.cs`（认证相关）或 `I{EntityName}Service.Password.cs`（密码相关）。

### 必须使用 partial

服务接口必须使用 `partial` 关键字：

```csharp
public partial interface IUserService { }
```

## IBaseService 接口

### 非泛型 IBaseService

继承非泛型的 `IBaseService` 接口，用于获取当前登录用户信息：

```csharp
public interface IBaseService
{
    /// <summary>
    /// 登录用户ID（框架自动填充）
    /// </summary>
    Guid LoginUserID { get; set; }
}
```

**作用**：服务实现中可以通过 `LoginUserID` 获取当前登录用户的 ID，用于记录操作人等场景。

### 泛型 IBaseService

继承泛型 `IBaseService<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO>` 接口：

```csharp
public partial interface I{EntityName}Service : IBaseService<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO>
```

| 参数 | 说明 | 对应模型 |
|------|------|----------|
| `TAddModel` | 添加模型 | `Add{EntityName}Model` |
| `TEditModel` | 编辑模型 | `Edit{EntityName}Model` |
| `TQueryModel` | 查询模型 | `Query{EntityName}Model` |
| `TDTO` | 单个数据传输对象 | `{EntityName}DTO` |
| `TListDTO` | 列表数据传输对象 | `{EntityName}ListDTO` |

### 标准方法

继承泛型 `IBaseService` 后，自动包含以下方法：

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `AddAsync` | `Task<Guid>` | 添加 |
| `EditAsync` | `Task` | 编辑 |
| `DeleteAsync` | `Task` | 删除 |
| `GetInfoAsync` | `Task<TDTO>` | 获取单个 |
| `GetListAsync` | `Task<(List<TListDTO>, RangeModel)>` | 获取分页列表 |

### 分页查询

`GetListAsync` 方法返回 `(List<TListDTO> data, RangeModel rangeInfo)` 元组，包含分页数据及范围信息：

```csharp
// 返回类型
Task<(List<TListDTO> data, RangeModel rangeInfo)> GetListAsync(TQueryModel model);
```

**RangeModel 定义**：

```csharp
public class RangeModel
{
    /// <summary>
    /// 跳过的数量
    /// </summary>
    public long Skip { get; set; }

    /// <summary>
    /// 获取的数量
    /// </summary>
    public long Take { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public long Total { get; set; }
}
```

**使用示例**：

```csharp
public async Task<(List<UserListDTO> users, RangeModel range)> GetUserListAsync(QueryUserModel model)
{
    // 分页查询
}
```

### 添加自定义方法

可以在继承 `IBaseService` 的接口中添加自定义方法：

```csharp
public partial interface IUserService : IBaseService<AddUserModel, EditUserModel, QueryUserModel, UserDTO, UserListDTO>
{
    /// <summary>
    /// 根据账号获取用户
    /// </summary>
    Task<UserDTO?> GetByAccountAsync(string account);
}
```

## 自定义服务接口

### 基本定义

不继承泛型 `IBaseService` 的服务接口，需要继承非泛型的 `IBaseService`：

```csharp
public partial interface IConfigService : IBaseService
{
    /// <summary>
    /// 获取配置值
    /// </summary>
    Task<int> GetConfigValueAsync();
}
```

### 命名建议

| 服务类型 | 命名示例 |
|----------|----------|
| 实体服务 | `IUserService`、`IClassService` |
| 配置服务 | `IConfigService`、`IAppSettingsService` |
| 统计服务 | `IStatisticsService`、`IReportService` |
| 工具服务 | `IFileService`、`ICaptchaService` |

## 服务模型

服务模型用于定义方法的参数类型，详细设计规范请参考 [服务模型设计规范](service-model-design.md)。

### 命名规范

| 用途 | 命名格式 | 示例 |
|------|----------|------|
| 添加模型 | `Add{EntityName}Model` | `AddUserModel` |
| 编辑模型 | `Edit{EntityName}Model` | `EditUserModel` |
| 查询模型 | `Query{EntityName}Model` | `QueryUserModel` |
| 自定义模型 | `{Operation}{EntityName}Model` | `ResetPasswordModel` |

### 文件位置

```
{ModuleName}.Abstractions/
└── Services/
    └── Models/
        └── {EntityName}/
            └── {Operation}Model.cs  ← 服务模型
```

### 无参数方法

无参数方法不需要创建服务模型，直接在接口中定义：

```csharp
public partial interface IAssignmentTypeService
{
    /// <summary>
    /// 获取最末级任务类型列表
    /// </summary>
    Task<List<AssignmentTypeLeafListDTO>> GetLeafListAsync();
}
```

## 禁止操作

- **不要**在 MGC 文件夹下编写任何代码
- **不要**忘记使用 `partial` 关键字

## 正确操作

- 所有自定义服务接口放在 `Abstractions/Services/` 目录
- 服务模型放在 `Services/Models/{EntityName}/` 目录
- 自定义方法可以直接定义在 `partial interface` 中
- 服务模型设计请参考 [服务模型设计规范](service-model-design.md)
