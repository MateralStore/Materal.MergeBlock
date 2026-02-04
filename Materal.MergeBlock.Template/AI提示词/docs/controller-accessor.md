# ControllerAccessor 说明

本文档介绍 ControllerAccessor 的概念、用途和使用方法。

## 1. 什么是 ControllerAccessor

**ControllerAccessor** 是框架为每个控制器自动生成的访问器类，用于**服务间跨模块调用**。

### 与 Controller 的关系

| 类型 | 用途 | 生成方式 | 位置 |
|------|------|----------|------|
| `Controller` | 处理 HTTP 请求 | 有实体时自动生成 | `MGC/Controllers/` 或 `Application/Controllers/` |
| `ControllerAccessor` | 跨模块服务调用 | 代码生成器自动生成 | `MGC/ControllerAccessors/` |

**核心区别**：
- `Controller` 负责处理 HTTP 请求，返回 `ResultModel` 包装的结果
- `ControllerAccessor` 实现 `IController` 接口，直接调用服务层方法，返回原始类型

### 文件示例

```csharp
// 控制器 - 处理 HTTP 请求
public class UserController : MainController<IUserService>
{
    [HttpGet]
    public Task<ResultModel<UserDTO>> GetUserAsync(Guid id)
    {
        return DefaultService.GetInfoAsync(id);
    }
}

// 控制器访问器 - 供其他模块调用（通过 HTTP）
public class UserControllerAccessor : BaseControllerAccessor, IUserController
{
    public override string ProjectName => "YourProject";
    public override string ModuleName => "User";

    public async Task<ResultModel<UserDTO>> GetUserAsync(Guid id)
    {
        // 通过 HTTP 调用本模块的控制器
        return await HttpHelper.SendAsync<IUserController, ResultModel<UserDTO>>(
            ProjectName, ModuleName, nameof(GetUserAsync), [], id);
    }
}
```

**关键点**：
- 继承 `BaseControllerAccessor`
- 实现对应的 Controller 接口（如 `IUserController`）
- 通过 `HttpHelper.SendAsync` 发起 HTTP 调用到本模块
- 返回 `ResultModel<T>` 而不是原始类型
- 定义 `ProjectName` 和 `ModuleName` 用于 HTTP 路由

## 2. 使用场景

当**其他模块**需要调用本模块的 API 时：

```csharp
// 其他模块的服务实现
public class OrderServiceImpl(
    IUserController userController)  // 注入 IController 接口
{
    public async Task<OrderDTO> CreateOrderAsync(CreateOrderModel model)
    {
        // 通过 Controller 调用用户模块的 API
        ResultModel<UserDTO> result = await userController.GetUserAsync(model.UserID);
        if (result.Code != ResultCode.Success)
        {
            throw new Exception(result.Message);
        }
        UserDTO user = result.Data;
        // ...
    }
}
```

**关键点**：
- 跨模块调用时，注入 `IController` 接口（如 `IUserController`）
- 框架自动返回 `UserControllerAccessor` 实例
- 调用方法返回 `ResultModel<T>`
- 需要检查 `result.Code` 判断是否成功
- 通过 `result.Data` 获取实际数据

**同一模块内**：直接调用 Service，不经过 Controller

## 3. 工作原理

1. **注册**：`ServiceCollectionExtensions.AddMainControllerAccessors()` 在应用启动时注册所有 ControllerAccessor
2. **注入**：跨模块调用时注入 `IController` 接口，框架返回对应的 `UserControllerAccessor`
3. **调用**：通过 HTTP 发起跨模块请求

```csharp
// ServiceCollectionExtensions.cs（自动生成）
public static void AddMainControllerAccessors(this IServiceCollection services)
{
    services.TryAddSingleton<IAdminController, AdminControllerAccessor>();
    services.TryAddSingleton<IUserController, UserControllerAccessor>();
    services.TryAddSingleton<IAuthController, AuthControllerAccessor>();
    // ...
}

// 同一模块内：直接调用 Service
public class OrderServiceImpl(IUserService userService)
{
    // userService 实际是 UserServiceImpl 实例
}

// 其他模块：注入 Controller 接口
public class OrderServiceImpl(IUserController userController)
{
    // userController 实际是 UserControllerAccessor 实例
}

## 4. 禁止操作

- ❌ 不要直接注入 `Controller` 类（如 `UserController`）
- ❌ 不要在 `ControllerAccessor` 中添加业务逻辑
- ❌ 不要修改 `MGC/ControllerAccessors/` 下的代码（会被覆盖）

## 5. 正确操作

- ✅ 跨模块调用时，通过 `IController` 接口注入（如 `IUserController`）
- ✅ 同一模块内，直接调用 Service 接口（如 `IUserService`）
- ✅ 业务逻辑由服务层处理

## 6. 与直接调用服务的区别

| 方式 | 返回类型 | 异常处理 | 适用场景 |
|------|----------|----------|----------|
| 直接调用 Service | 原始类型（如 `Task<T>`） | 服务层异常 | 同一模块内 |
| 调用 Controller | `ResultModel<T>` | 通过 `Code` 判断 | 跨模块调用 |

**说明**：
- 同一模块内：直接注入 Service 接口（如 `IUserService`）
- 跨模块调用：注入 Controller 接口（如 `IUserController`），框架返回 `UserControllerAccessor`

## 7. 认证与授权

ControllerAccessor 会继承控制器的认证特性：

- 标记 `[AllowAnonymous]` 的方法可以匿名访问
- 未标记的方法需要有效的 JWT Token

```csharp
// ControllerAccessor（自动生成）
public class AuthControllerAccessor : BaseControllerAccessor, IAuthController
{
    public override string ProjectName => "ZhiTu";
    public override string ModuleName => "Main";

    // 登录允许匿名访问
    public async Task<ResultModel<LoginResultDTO>> LoginAsync(LoginRequestModel requestModel)
        => await HttpHelper.SendAsync<IAuthController, ResultModel<LoginResultDTO>>(
            ProjectName, ModuleName, nameof(LoginAsync), [], requestModel);

    // 获取用户信息需要认证
    public async Task<ResultModel<LoginResultDTO>> GetLoginUserInfoAsync()
        => await HttpHelper.SendAsync<IAuthController, ResultModel<LoginResultDTO>>(
            ProjectName, ModuleName, nameof(GetLoginUserInfoAsync), []);
}
```

**注意**：`HttpHelper` 会自动携带当前请求的 Token，认证信息会传递到目标控制器。
