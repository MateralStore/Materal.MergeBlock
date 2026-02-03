# 请求模型设计规范

本文档描述了在 Materal.MergeBlock 项目中设计请求模型（RequestModel）时需要遵循的规范。

> **相关文档**：
> - [控制器设计规范](controller-design.md) - 控制器接口设计
> - [服务模型设计规范](service-model-design.md) - 服务层模型设计

## 基本规范

### 命名规范

| 用途 | 命名格式 | 示例 |
|------|----------|------|
| 添加请求 | `Add{EntityName}RequestModel` | `AddUserRequestModel` |
| 编辑请求 | `Edit{EntityName}RequestModel` | `EditUserRequestModel` |
| 查询请求 | `Query{EntityName}RequestModel` | `QueryUserRequestModel` |
| 自定义请求 | `{Operation}{EntityName}RequestModel` | `ChangePasswordRequestModel` |
| 取消/操作请求 | `{Operation}{EntityName}RequestModel` | `CancelClassScheduleRequestModel` |

### 文件位置

```
{ModuleName}.Abstractions/
└── RequestModel/
    └── {EntityName}/
        └── {Operation}RequestModel.cs  ← 请求模型
```

### 必须使用 partial

所有请求模型必须使用 `partial class`：

```csharp
public partial class ChangePasswordRequestModel { }
```

## RequestModel 与 ServiceModel 的关系

### 配合 [MapperController] 使用

当 Service 方法标记了 `[MapperController]` 特性时，必须同步创建一份与 ServiceModel 属性**完全一致**的 RequestModel：

```csharp
// Service 模型 - Services/Models/EntityName/ChangePasswordModel.cs
namespace ProjectName.ModuleName.Abstractions.Services.Models.EntityName;

/// <summary>
/// 修改密码模型
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

// 请求模型 - RequestModel/EntityName/ChangePasswordRequestModel.cs
namespace ProjectName.ModuleName.Abstractions.RequestModel.EntityName;

/// <summary>
/// 修改密码请求模型
/// </summary>
public partial class ChangePasswordRequestModel
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

**重要规则**：
- RequestModel 的属性必须与 ServiceModel **完全一致**（名称、类型、特性）
- 框架自动完成映射，无需手动转换

**特殊情况**：当需要从控制器层获取特殊参数（如客户端 IP 地址、设备信息等）时，这些参数只在 ServiceModel 中定义，由控制器实现赋值给服务模型后传递给 Service：

```csharp
// Service 模型 - 包含 IP 地址字段
public partial class LoginModel
{
    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "账号为空")]
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码为空")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 客户端IP（控制器层赋值，不在 RequestModel 中定义）
    /// </summary>
    public string? ClientIP { get; set; }
}

// 请求模型 - 只包含前端传递的参数
public partial class LoginRequestModel
{
    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "账号为空")]
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码为空")]
    public string Password { get; set; } = string.Empty;
}

// Controller 实现中赋值
[HttpPost]
public async Task<ResultModel<TokenDTO>> LoginAsync(LoginRequestModel model)
{
    LoginModel serviceModel = new()
    {
        Account = model.Account,
        Password = model.Password,
        ClientIP = GetClientIP(HttpContext)  // 控制器层赋值
    };
    return await DefaultService.LoginAsync(serviceModel);
}
```

### 无需 RequestModel 的情况

以下情况**不需要**创建 RequestModel：

| 情况 | 说明 |
|------|------|
| 无参数方法 | Service 方法不需要传入参数时 |
| 标准 CRUD | 框架自动生成的 CRUD 控制器已包含 |
| 仅返回数据 | `Task<T>` 返回类型且无参数 |

```csharp
// 无参数方法 - 不需要 RequestModel
public partial interface IEntityNameService
{
    [MapperController(MapperType.Post)]
    Task<List<EntityNameListDTO>> GetListAsync();  // 无需 RequestModel
}
```

## 何时需要 RequestModel

### 必须创建 RequestModel 的场景

| 场景 | 说明 |
|------|------|
| 使用 `[MapperController]` 的方法 | 需要接收 HTTP 请求参数 |
| 自定义控制器方法 | 手动创建的 Controller 接口 |
| 文件上传接口 | 需要使用 `IFormFile` |
| 复杂参数接口 | 参数包含多个字段 |

### 示例：标准添加请求

```csharp
namespace ProjectName.ModuleName.Abstractions.RequestModel.EntityName;

/// <summary>
/// 添加实体请求模型
/// </summary>
public partial class AddEntityNameRequestModel
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称为空")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 代码
    /// </summary>
    [Required(ErrorMessage = "代码为空")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
}
```

### 示例：查询请求

```csharp
namespace ProjectName.ModuleName.Abstractions.RequestModel.EntityName;

/// <summary>
/// 查询实体请求模型
/// </summary>
public partial class QueryEntityNameRequestModel
{
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public EntityNameStatus? Status { get; set; }
}
```

### 示例：操作请求

```csharp
namespace ProjectName.ModuleName.Abstractions.RequestModel.EntityName;

/// <summary>
/// 取消操作请求模型
/// </summary>
public partial class CancelEntityNameRequestModel
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    [Required(ErrorMessage = "唯一标识为空")]
    public Guid ID { get; set; }

    /// <summary>
    /// 取消原因
    /// </summary>
    [Required(ErrorMessage = "取消原因为空")]
    public string CancelReason { get; set; } = string.Empty;
}
```

## 为什么 RequestModel 不使用查询特性

**查询特性（如 `[Equal]`、`[Contains]` 等）只在 ServiceModel 中使用**，RequestModel 中不需要也不能使用。

**原因**：

1. **职责分离**：查询特性用于控制仓储层的查询逻辑生成，属于服务层和数据访问层的实现细节，不应暴露到表现层。

2. **框架设计**：`FilterModel` 的查询特性通过 `GetSearchExpression<T>()` 方法生成 Lambda 表达式，这些方法只在 Service 层使用仓储的 `Find(FilterModel)`、`Paging(PageRequestModel)` 等方法时生效。

3. **避免混淆**：RequestModel 负责接收 HTTP 请求数据，ServiceModel 负责服务层的数据传递和查询逻辑，两者职责不同。

**正确做法**：

```csharp
// Service 模型 - 定义查询特性（用于仓储查询）
namespace ProjectName.ModuleName.Abstractions.Services.Models.EntityName;

public partial class QueryEntityNameModel : FilterModel
{
    [Equal]
    public string? Code { get; set; }

    [Contains]
    public string? Name { get; set; }
}

// 请求模型 - 只接收前端数据，无查询特性
namespace ProjectName.ModuleName.Abstractions.RequestModel.EntityName;

public partial class QueryEntityNameRequestModel
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}
```

> **说明**：查询特性的详细说明请参考 [服务模型设计规范](service-model-design.md)。

## 特殊类型

### 文件上传

文件上传接口需要使用 `IFormFile`，**必须在 RequestModel 中定义**，不能使用 ServiceModel：

```csharp
namespace ProjectName.ModuleName.Abstractions.RequestModel.File;

/// <summary>
/// 上传文件请求模型
/// </summary>
public partial class UploadFileRequestModel
{
    /// <summary>
    /// 文件
    /// </summary>
    [Required(ErrorMessage = "请选择要上传的文件")]
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// 文件分类
    /// </summary>
    [Required(ErrorMessage = "文件分类为空")]
    public string Category { get; set; } = string.Empty;
}
```

**注意**：
- `IFormFile` 是 ASP.NET Core 的 HTTP 特定类型，只能用于 RequestModel
- ServiceModel 中不能使用 `IFormFile`

### 批量操作

批量操作通过 `List<Guid>` 传递 ID 列表：

```csharp
namespace ProjectName.ModuleName.Abstractions.RequestModel.EntityName;

/// <summary>
/// 批量删除请求模型
/// </summary>
public partial class BatchDeleteRequestModel
{
    /// <summary>
    /// 唯一标识列表
    /// </summary>
    [Required(ErrorMessage = "请选择要删除的数据")]
    [MinLength(1, ErrorMessage = "至少需要选择一个")]
    public List<Guid> IDs { get; set; } = new();
}
```

## 验证特性

### 常用验证特性

| 特性 | 说明 | 示例 |
|------|------|------|
| `[Required]` | 必填字段 | `[Required(ErrorMessage = "名称为空")]` |
| `[StringLength]` | 字符串长度 | `[StringLength(50)]` |
| `[Range]` | 数值范围 | `[Range(0, 100)]` |
| `[MinLength]` | 最小长度 | `[MinLength(1, ErrorMessage = "至少需要一个")]` |
| `[MaxLength]` | 最大长度 | `[MaxLength(100)]` |
| `[EmailAddress]` | 邮箱格式 | `[EmailAddress]` |
| `[RegularExpression]` | 正则验证 | `[RegularExpression(@"^\d+$")]` |

## 文件放置总结

| 类型 | 位置 |
|------|------|
| RequestModel | `{ModuleName}.Abstractions/RequestModel/{EntityName}/{Name}RequestModel.cs` |

### 实际路径示例

```csharp
// 添加实体请求模型
// 位置: ProjectName.ModuleName.Abstractions/RequestModel/EntityName/AddEntityNameRequestModel.cs

// 查询实体请求模型
// 位置: ProjectName.ModuleName.Abstractions/RequestModel/EntityName/QueryEntityNameRequestModel.cs

// 修改密码请求模型
// 位置: ProjectName.ModuleName.Abstractions/RequestModel/EntityName/ChangePasswordRequestModel.cs
```

## 禁止操作

- ❌ 不要在 MGC 文件夹下编写任何代码
- ❌ 不要忘记使用 `partial` 关键字
- ❌ 不要在 RequestModel 中包含确认密码等前端验证逻辑
- ❌ ServiceModel 中不要使用 `IFormFile` 等 HTTP 特定类型
- ❌ 不要在 RequestModel 中使用查询特性（如 `[Equal]`、`[Contains]` 等）

## 正确操作

- RequestModel 放在 `RequestModel/{EntityName}/` 目录
- RequestModel 使用 `partial class`
- 为每个属性添加 XML 文档注释
- 使用 `[Required]`、`[StringLength]` 等验证特性
- RequestModel 属性必须与对应的 ServiceModel 完全一致（特殊情况除外）
