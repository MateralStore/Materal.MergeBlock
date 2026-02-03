# 控制器实现规范

本文档描述了在 Materal.MergeBlock 项目中实现控制器（Controller）时需要遵循的规范。

> **重要提示**：本项目使用的映射器是 **Materal.Utils.AutoMapper**，不是 AutoMapper 库。两者 API 不同，AutoMapper 的使用经验在本项目中**不适用**。
>
> 相关文档：
> - [控制器设计规范](controller-design.md) - 控制器接口设计
> - [请求模型设计规范](request-model-design.md) - 请求模型设计
> - [编码规范](coding-style.md) - 通用编码规范

## 控制器基类

### 继承关系

```
MergeBlockController (基类)
    ↓
MergeBlockController<TService>
    ↓
MergeBlockController<TAdd, TEdit, TQuery, TAddModel, TEditModel, TQueryModel, TDTO, TListDTO, TService>
```

### MergeBlockController 基类

`MergeBlockController` 位于 `Materal.MergeBlock.Web.Abstractions.Controllers` 命名空间，提供以下属性和方法：

```csharp
public abstract class MergeBlockController : ControllerBase
{
    /// <summary>
    /// 自动映射
    /// </summary>
    protected IMapper Mapper { get; }

    /// <summary>
    /// 获得客户端 IP
    /// </summary>
    protected string GetClientIP();

    /// <summary>
    /// 绑定 LoginUserID
    /// </summary>
    protected void BindLoginUserID(object model);
}
```

### 常用方法说明

| 成员 | 类型 | 说明 |
|------|------|------|
| `Mapper` | 属性 | Materal.Utils.AutoMapper 映射器 |
| `GetClientIP()` | 方法 | 获取客户端 IP 地址 |
| `BindLoginUserID(model)` | 方法 | 自动将当前登录用户 ID 绑定到模型的 `[LoginUserID]` 属性 |

### 带服务的泛型基类

```csharp
public abstract class MergeBlockController<TService> : MergeBlockController
    where TService : IBaseService
{
    /// <summary>
    /// 服务对象
    /// </summary>
    protected TService DefaultService { get; }
}
```

### 完整 CRUD 基类

```csharp
public abstract class MergeBlockController<...> : MergeBlockController<TService>
{
    /// <summary>
    /// 添加
    /// </summary>
    [HttpPost]
    public virtual Task<ResultModel<Guid>> AddAsync(TAddRequestModel requestModel);

    /// <summary>
    /// 修改
    /// </summary>
    [HttpPut]
    public virtual Task<ResultModel> EditAsync(TEditRequestModel requestModel);

    /// <summary>
    /// 删除
    /// </summary>
    [HttpDelete]
    public virtual Task<ResultModel> DeleteAsync(Guid id);

    /// <summary>
    /// 获取详情
    /// </summary>
    [HttpGet]
    public virtual Task<ResultModel<TDTO>> GetInfoAsync(Guid id);

    /// <summary>
    /// 获取列表
    /// </summary>
    [HttpPost]
    public virtual Task<CollectionResultModel<TListDTO>> GetListAsync(TQueryRequestModel requestModel);
}
```

## 基本规范

### 命名规范

| 项目 | 规范 | 示例 |
|------|------|------|
| 控制器类 | `{EntityName}Controller` | `UserController`、`ClassScheduleController` |
| 文件位置 | `Application/Controllers/` | `UserController.cs` |
| 命名空间 | `{ProjectName}.{ModuleName}.Application.Controllers` | `ProjectName.ModuleName.Application.Controllers` |

### 必须使用 partial

控制器实现类必须使用 `partial` 关键字：

```csharp
namespace ProjectName.ModuleName.Application.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
public partial class UserController : MergeBlockController<IUserService>
{
    // 自定义方法
}
```

### 继承选择

根据控制器用途选择合适的基类：

| 场景 | 基类 | 说明 |
|------|------|------|
| 标准 CRUD | `MergeBlockController<..., ..., ..., TService>` | 继承后 CRUD 方法是 `virtual`，需使用 `override` 重写 |
| 自定义服务 | `MergeBlockController<TService>` | 无 CRUD 方法，需自行实现 |
| 完全自定义 | `MergeBlockController` | 只有基础功能，无服务关联 |

## CRUD 方法实现模式

### 标准 CRUD 实现

当需要自定义 CRUD 方法时，可重写基类虚方法（需继承完整 CRUD 基类）：

```csharp
namespace ProjectName.ModuleName.Application.Controllers;

/// <summary>
/// 班级控制器
/// </summary>
public partial class ClassScheduleController : MergeBlockController<AddClassScheduleRequestModel, EditClassScheduleRequestModel, QueryClassScheduleRequestModel, AddClassScheduleModel, EditClassScheduleModel, QueryClassScheduleModel, ClassScheduleDTO, ClassScheduleListDTO, IClassScheduleService>
{
    /// <summary>
    /// 添加
    /// </summary>
    [HttpPost]
    public override async Task<ResultModel<Guid>> AddAsync(AddClassScheduleRequestModel requestModel)
    {
        AddClassScheduleModel model = Mapper.Map<AddClassScheduleModel>(requestModel);
        BindLoginUserID(model);
        Guid result = await DefaultService.AddAsync(model);
        return ResultModel<Guid>.Success(result, "添加成功");
    }

    /// <summary>
    /// 修改
    /// </summary>
    [HttpPut]
    public override async Task<ResultModel> EditAsync(EditClassScheduleRequestModel requestModel)
    {
        EditClassScheduleModel model = Mapper.Map<EditClassScheduleModel>(requestModel);
        BindLoginUserID(model);
        await DefaultService.EditAsync(model);
        return ResultModel.Success("修改成功");
    }

    /// <summary>
    /// 删除
    /// </summary>
    [HttpDelete]
    public override async Task<ResultModel> DeleteAsync([Required(ErrorMessage = "唯一标识为空")] Guid id)
    {
        await DefaultService.DeleteAsync(id);
        return ResultModel.Success("删除成功");
    }

    /// <summary>
    /// 获取详情
    /// </summary>
    [HttpGet]
    public override async Task<ResultModel<ClassScheduleDTO>> GetInfoAsync([Required(ErrorMessage = "唯一标识为空")] Guid id)
    {
        ClassScheduleDTO result = await DefaultService.GetInfoAsync(id);
        return ResultModel<ClassScheduleDTO>.Success(result, "查询成功");
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    [HttpPost]
    public override async Task<CollectionResultModel<ClassScheduleListDTO>> GetListAsync(QueryClassScheduleRequestModel requestModel)
    {
        QueryClassScheduleModel model = Mapper.Map<QueryClassScheduleModel>(requestModel);
        (List<ClassScheduleListDTO> result, RangeModel rangeInfo) = await DefaultService.GetListAsync(model);
        return CollectionResultModel<ClassScheduleListDTO>.Success(result, rangeInfo, "查询成功");
    }
}
```

**注意**：继承完整 CRUD 基类时，方法需使用 `override` 关键字重写基类 `virtual` 方法。

### 标准流程

CRUD 方法的标准处理流程：

```
1. 接收 RequestModel 参数
2. 使用 Mapper.Map 映射为 ServiceModel
3. 调用 BindLoginUserID(model) 绑定登录用户ID（如需要）
4. 调用 Service 层方法
5. 包装返回结果为 ResultModel 或 CollectionResultModel
```

## 自定义方法实现

### 无参数方法

当方法不需要传入参数时：

```csharp
/// <summary>
/// 获取配置信息
/// </summary>
[HttpGet]
public async Task<ResultModel<ConfigDTO>> GetConfigAsync()
{
    ConfigDTO result = await DefaultService.GetConfigAsync();
    return ResultModel<ConfigDTO>.Success(result, "获取成功");
}
```

### 带参数方法

当方法需要传入参数时：

```csharp
/// <summary>
/// 获取未使用教室列表
/// </summary>
[HttpPost]
public async Task<ResultModel<List<ClassroomListDTO>>> GetUnusedClassroomListAsync(TimeQuantumRequestModel requestModel)
{
    List<ClassListDTO> result = await DefaultService.GetUnusedClassroomListAsync(requestModel.MinDateTime, requestModel.MaxDateTime);
    return ResultModel<List<ClassroomListDTO>>.Success(result, "查询成功");
}
```

### 特殊参数获取

当需要获取客户端 IP、登录用户 ID 等特殊参数时：

```csharp
/// <summary>
/// 获取用户配置
/// </summary>
[HttpGet]
public async Task<ResultModel<UserConfigDTO>> GetInfoAsync()
{
    Guid userID = this.GetLoginUserID();  // 获取登录用户ID
    UserConfigDTO result = await DefaultService.GetInfoAsync(userID);
    return ResultModel<UserConfigDTO>.Success(result, "获取成功");
}

/// <summary>
/// 记录登录日志
/// </summary>
[HttpPost]
public async Task<ResultModel> RecordLoginAsync(LoginRequestModel requestModel)
{
    LoginModel model = new()
    {
        Account = requestModel.Account,
        Password = requestModel.Password,
        ClientIP = GetClientIP()  // 获取客户端IP
    };
    await DefaultService.LoginAsync(model);
    return ResultModel.Success("登录成功");
}
```

## 映射器使用

### 重要说明

> **警告**：本项目使用的映射器是 `Materal.Utils.AutoMapper`，**不是** AutoMapper 库。
>
> API 不同：
> - ❌ 不使用 `.Adapt<T>()`（AutoMapper API）
> - ✅ 使用 `Mapper.Map<T>(source)`（Materal.Utils.AutoMapper API）

### 映射单个对象

```csharp
// RequestModel 转 ServiceModel
AddClassScheduleModel model = Mapper.Map<AddClassScheduleModel>(requestModel);

// Entity 转 DTO
UserDTO dto = Mapper.Map<UserDTO>(user);
```

### 映射列表

```csharp
// 自动支持 List、ICollection 等集合类型
List<UserDTO> dtoList = Mapper.Map<List<UserDTO>>(userList);
ICollection<RoleDTO> dtoCollection = Mapper.Map<ICollection<RoleDTO>>(roleCollection);
```

## LoginUserID 绑定

### 自动绑定

使用 `BindLoginUserID` 方法自动将当前登录用户 ID 绑定到 ServiceModel 的 `[LoginUserID]` 属性：

```csharp
/// <summary>
/// 添加
/// </summary>
[HttpPost]
public ResultModel<Guid> Add(AddClassScheduleRequestModel requestModel)
{
    AddClassScheduleModel model = Mapper.Map<AddClassScheduleModel>(requestModel);
    BindLoginUserID(model);  // 自动绑定登录用户ID
    Guid result = DefaultService.Add(model);
    return ResultModel<Guid>.Success(result, "添加成功");
}
```

**ServiceModel 中的定义**：

```csharp
public partial class AddClassScheduleModel
{
    /// <summary>
    /// 创建人ID
    /// </summary>
    [LoginUserID]
    public Guid CreateUserID { get; set; }
}
```

## 文件放置总结

| 类型 | 位置 |
|------|------|
| Controller 实现 | `{ModuleName}.Application/Controllers/{EntityName}Controller.cs` |
| 项目级 Controller 基类 | `{ModuleName}.Application/{ModuleName}Controller.cs` |

### 实际路径示例

```csharp
// 用户控制器
// 位置: ProjectName.ModuleName.Application/Controllers/UserController.cs

// 项目级控制器基类
// 位置: ProjectName.ModuleName.Application/ProjectNameController.cs

// 项目级控制器基类（带服务）
// 位置: ProjectName.ModuleName.Application/ProjectNameController{TService}.cs
```

## 常见模式

### 模式一：继承项目级控制器基类

```csharp
namespace ProjectName.ModuleName.Application.Controllers;

/// <summary>
/// 项目控制器基类
/// </summary>
[Route("ProjectAPI/[controller]/[action]")]
public abstract class ProjectNameController : MergeBlockController
{
}

/// <summary>
/// 项目控制器基类（带服务）
/// </summary>
[Route("ProjectAPI/[controller]/[action]")]
public abstract class ProjectNameController<TService> : MergeBlockController<TService>
    where TService : IBaseService
{
}

// 具体控制器
public partial class UserConfigController : ProjectNameController<IUserConfigService>
{
    /// <summary>
    /// 获取配置
    /// </summary>
    [HttpGet]
    public async Task<ResultModel<ConfigDTO>> GetConfigAsync()
    {
        Guid userID = this.GetLoginUserID();
        ConfigDTO result = await DefaultService.GetConfigAsync(userID);
        return ResultModel<ConfigDTO>.Success(result, "获取成功");
    }
}
```

### 模式二：直接继承框架基类

```csharp
public partial class ClassScheduleController : MergeBlockController<IClassScheduleService>
{
    // CRUD 方法实现
}
```

## 禁止操作

- ❌ 不要在 MGC 文件夹下编写任何代码
- ❌ 不要忘记使用 `partial` 关键字
- ❌ 不要直接抛出系统异常（如 `ArgumentNullException`）
- ❌ 不要使用 AutoMapper 的 `.Adapt<T>()` 方法
- ❌ 不要在控制器层处理业务逻辑，应调用 Service 层

## 正确操作

- 控制器实现放在 `Application/Controllers/` 目录
- 遵循 [编码规范](coding-style.md) 中的命名和格式要求
- 为所有公开方法添加 XML 文档注释
- 使用 `Mapper.Map<T>()` 进行对象映射
- 使用 `BindLoginUserID(model)` 绑定登录用户 ID
- 返回结果包装为 `ResultModel` 或 `CollectionResultModel`
- 异常抛出由框架统一处理，不要 try-catch
