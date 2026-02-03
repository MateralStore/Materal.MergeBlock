# 编码规范

本文档定义了项目的 C# 编码标准，遵循微软官方 [C# 编码约定](https://learn.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/coding-conventions)。

## 1 命名规范

### 1.1 命名约定概述

| 类型 | 约定 | 示例 |
|------|------|------|
| 类、结构体 | PascalCase | `UserService`, `OrderController` |
| 接口 | PascalCase，以 `I` 开头 | `IUserService`, `IOrderRepository` |
| 方法 | PascalCase | `GetUserAsync`, `CreateOrder` |
| 属性 | PascalCase | `UserName`, `IsActive` |
| 字段（私有） | camelCase | `_logger`, `_userRepository` |
| 常量 | PascalCase | `MaxRetryCount` |
| 参数 | camelCase | `userID`, `orderRequest` |
| 局部变量 | camelCase | `result`, `userList` |

### 1.2 命名规则

```csharp
// ✅ 正确示例
public class UserService
{
    private readonly ILogger<UserService> _logger;
    public const int MaxPageSize = 100;

    public async Task<UserDto?> GetUserAsync(Guid userID)
    {
        UserDto? result = await _repository.FirstOrDefaultAsync(userID);
        return result;
    }
}

// ❌ 错误示例
public class userService
{
    private ILogger<UserService> logger;  // 字段应为私有
    public const int max_page_size = 100;  // 常量应为 PascalCase

    public async Task<UserDto> getuser(Guid UserID)  // 方法和参数命名错误
    {
        UserDto result = await repository.FirstOrDefaultAsync(UserID);  // 缺少 await
        return result;
    }
}
```

### 1.3 命名建议

- **避免缩写**：使用 `UserService` 而不是 `UsrSvc`
- **避免匈牙利命名**：使用 `userID` 而不是 `iUserID`
- **使用 ID 而非 Id**：`ID` 是全大写的缩写形式（如 HTTP、XML），保持一致性能提高代码可读性
- **使用有意义的名称**：变量名应清晰表达其用途
- **布尔值使用 Is/Has/Can 前缀**：`IsActive`, `HasPermission`, `CanDelete`

## 2 代码格式

### 2.1 缩进与空格

```csharp
// ✅ 正确示例
public async Task<IActionResult> CreateUserAsync(CreateUserRequest request)
{
    if (request == null) throw new {ProjectName}Exception("请求不能为空", StatusCodes.Status400BadRequest);

    UserDto user = await _userService.CreateAsync(request);
    return Ok(user);
}

// ❌ 错误示例（省略大括号时，throw 必须与 if 同行）
public async Task<IActionResult> CreateUserAsync(CreateUserRequest request)
{
    if (request == null)
        throw new {ProjectName}Exception("请求不能为空", StatusCodes.Status400BadRequest);
    // ^ throw 必须与 if 写在同一行

    UserDto user = await _userService.CreateAsync(request);
    return Ok(user);
}

// ❌ 错误示例（格式混乱）
public async Task<IActionResult>CreateUserAsync(CreateUserRequest request){
        if(request==null){throw new {ProjectName}Exception("请求不能为空", StatusCodes.Status400BadRequest);}
    UserDto user=await _userService.CreateAsync(request);
    return Ok(user);}
```

### 2.2 大括号风格

使用 Allman 风格（左大括号单独一行）：

```csharp
// ✅ 正确：使用文件命名空间（C# 10+）
namespace {ProjectName}.{ModuleName}.Application;

public class UserService
{
    // ...
}

// ❌ 错误（K&R 风格）
public class UserService {
    // ...
}
```

### 2.3 每行语句数

一行只写一条语句：

```csharp
// ✅ 正确
User user = await GetUserAsync(userID);
user.LastLoginTime = DateTime.UtcNow;
await _repository.UpdateAsync(user);

// ❌ 错误
User user = await GetUserAsync(userID); user.LastLoginTime = DateTime.UtcNow; await _repository.UpdateAsync(user);
```

### 2.4 空行规范

- 方法之间空一行
- 方法内部逻辑分组之间空一行
- 不同代码块之间空一行

```csharp
public async Task<UserDto> ProcessOrderAsync(OrderRequest request)
{
    // 验证参数
    if (request == null) throw new {ProjectName}Exception("请求不能为空", StatusCodes.Status400BadRequest);

    // 获取用户
    User user = await _userService.GetAsync(request.UserID);
    if (user == null) throw new UserNotFoundException(request.UserID);

    // 处理订单
    Order order = await _orderService.CreateAsync(user, request.Items);

    return _mapper.Map<UserDto>(order);
}
```

### 2.5 单行语句省略大括号

当 `if` 语句只有一行，且该行是 `throw`、`return`、`continue` 或 `break` 时，可以省略大括号：

```csharp
// ✅ 正确：单行 throw/return 可省略大括号
if (request == null) throw new {ProjectName}Exception("请求不能为空", StatusCodes.Status400BadRequest);

if (user == null) throw new {ProjectName}Exception("用户不存在", StatusCodes.Status404NotFound);

// ✅ 正确：单行 return 可直接返回
public bool IsValid(string input) => !string.IsNullOrEmpty(input);

// ❌ 错误：多行语句必须使用大括号
if (request == null)
    throw new {ProjectName}Exception("请求不能为空", StatusCodes.Status400BadRequest);
    _logger.LogWarning("收到空请求");  // 这行永远不会执行

// ❌ 错误：非 throw/return 语句应使用大括号
if (isLoading)
    _isLoading = true;  // 这行看起来像被 if 包裹，但实际上没有
```

> **注意**：当省略大括号时，`throw`/`return` 必须与 `if` 写在同一行。

## 3 注释规范

### 3.1 注释原则

- **解释为什么，而非做什么**：代码本身应清晰表达逻辑
- **复杂算法必须注释**：说明算法思路和关键步骤
- **业务逻辑必须注释**：解释业务规则和决策依据

### 3.2 XML 文档注释

公开 API 必须添加 XML 文档注释：

```csharp
/// <summary>
/// 创建新用户
/// </summary>
/// <param name="request">用户创建请求</param>
/// <returns>创建的用户信息</returns>
/// <exception cref="{ProjectName}Exception">当 request 为 null 时抛出</exception>
/// <exception cref="{ProjectName}Exception">当邮箱已存在时抛出</exception>
public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
{
    // ...
}
```

### 3.3 行内注释

```csharp
// ✅ 正确
// 使用 BCrypt 进行密码加密，安全性高于 MD5
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

// ❌ 错误（解释做什么，而非为什么）
// 设置密码为哈希值
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
```

### 3.4 注释符号与空格

```csharp
// ✅ 正确（符号后有空格）
// 这是正确的注释格式

// ❌ 错误（符号后无空格）
//这是不正确的注释格式
```

## 4 最佳实践

### 4.1 使用语言特性

```csharp
// ✅ 正确：使用 ?. 和 ?[] 避免空引用异常
string userName = user?.Name ?? "Unknown";
Role? firstRole = user?.Roles?[0];

// ✅ 正确：使用模式匹配
if (user is User { IsActive: true } activeUser)
{
    // ...
}

// ✅ 正确：使用范围索引
T lastItem = list[^1];
Range subList = list[1..^1];
```

### 4.2 异步编程

```csharp
// ✅ 正确
public async Task<UserDto> GetUserAsync(Guid id)
{
    return await _repository.FirstOrDefaultAsync(id);
}

// ✅ 正确：直接返回 Task，不使用 await
public Task<UserDto> GetUserAsync(Guid id)
{
    return _repository.FirstOrDefaultAsync(id);
}

// ❌ 错误：使用 .Result 或 .Wait
UserDto user = _service.GetUserAsync(id).Result;
```

### 4.3 异常处理

所有自定义异常应继承自 `{ProjectName}Exception`，框架会自动捕获并处理：

```csharp
// ✅ 正确：使用 {ProjectName}Exception 及其子类
public class UserNotFoundException : {ProjectName}Exception
{
    public Guid UserID { get; }

    public UserNotFoundException(Guid userID)
        : base($"用户不存在: {userID}")
    {
        UserID = userID;
    }
}

// ✅ 正确：直接抛出 {ProjectName}Exception
if (user == null) throw new {ProjectName}Exception("用户不存在", StatusCodes.Status404NotFound);

// ❌ 错误：不要使用 ArgumentNullException 等系统异常
if (request == null) throw new ArgumentNullException(nameof(request));

// ❌ 错误：不要返回 BadRequest，抛出异常让框架处理
// return BadRequest("用户不存在");  // 错误
throw new UserNotFoundException(userID);  // 正确
```

> **说明**：`{ProjectName}Exception` 定义在 `Core` 项目中，框架会自动捕获并返回统一的错误响应格式。

```csharp
// ✅ 正确：使用 using 语句确保资源释放
using var connection = new SqlConnection(_connectionString);
IEnumerable<User> users = await connection.QueryAsync<User>("SELECT * FROM Users");
```

### 4.4 依赖注入

```csharp
// ✅ 正确：使用主构造函数注入（C# 12+）
public class UserService(IUserRepository userRepository, ILogger<UserService> logger)
    : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<UserService> _logger = logger;
}

// ❌ 错误：避免服务定位器模式
public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public UserService(IServiceProvider serviceProvider)
    {
        _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    }
}
```

### 4.5 日志记录

```csharp
// ✅ 正确：使用结构化日志
_logger.LogInformation(
    "用户 {UserID} 在 {Time} 登录成功",
    userID,
    DateTime.UtcNow);

// ✅ 正确：使用日志级别
_logger.LogDebug("查询参数: {@Request}", request);
_logger.LogWarning("请求频率过高: {IP}", ipAddress);
_logger.LogError(ex, "处理请求时发生错误: {Path}", path);
```

### 4.6 常量与只读字段

```csharp
// ✅ 正确
public class OrderService
{
    private const int MaxRetryCount = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

    public async Task<bool> ProcessOrderAsync(Order order)
    {
        for (int attempt = 1; attempt <= MaxRetryCount; attempt++)
        {
            try
            {
                await ProcessAsync(order);
                return true;
            }
            catch (Exception ex) when (attempt < MaxRetryCount)
            {
                _logger.LogWarning(ex, "第 {Attempt} 次重试", attempt);
                await Task.Delay(_retryDelay * attempt);
            }
        }
        return false;
    }
}
```

## 5 项目特定规范

### 5.1 文件组织

```
{ProjectName}.{ModuleName}/
├── {ProjectName}.{ModuleName}.Abstractions/
│   ├── Domain/           ← 实体定义
│   ├── Enums/            ← 枚举定义
│   ├── DTO/              ← 自定义 DTO
│   ├── RequestModel/     ← 自定义请求模型
│   ├── Services/         ← 服务接口
│   │   └── Models/       ← 自定义服务模型
│   ├── Controllers/      ← 控制器接口
│   ├── Events/           ← 事件总线中的事件定义
│   └── MGC/              ← 自动生成，不要修改
├── {ProjectName}.{ModuleName}.Application/
│   ├── Services/         ← 服务实现
│   ├── Controllers/      ← 控制器实现
│   ├── AutoMapperProfile/← 自动映射配置
│   ├── ScheduledTasks/   ← 定时任务
│   ├── EventHandlers/    ← 事件处理器
│   └── MGC/              ← 自动生成，不要修改
└── {ProjectName}.{ModuleName}.Repository/
    ├── Migrations/       ← 迁移文件，不要修改
    ├── Repositories/     ← 自定义仓储实现
    └── MGC/              ← 自动生成，不要修改
```

### 5.2 文件命名规则

- **类文件**：与类名同名，如 `UserService.cs`
- **接口文件**：以 `I` 开头，如 `IUserService.cs`
- **枚举文件**：与枚举名同名，如 `OrderStatus.cs`
- **一个文件一个类**：避免在一个文件中定义多个类

### 5.3 Using 指令排序

使用 IDE 代码清理工具（Ctrl+K, Ctrl+D）自动排序，按字母顺序排列：

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using {ProjectName}.Abstractions.Repositories;
using {ProjectName}.Abstractions.Services;
```

> **提示**：在 Visual Studio 中使用 `Ctrl+K, Ctrl+D` 或 `Ctrl+E, D` 自动格式化代码，IDE 会自动对 using 指令进行排序。

## 6 代码审查清单

在提交代码前，请检查以下项目：

- [ ] 代码能够通过编译（`dotnet build`）
- [ ] 所有公开 API 都有 XML 文档注释
- [ ] 变量和方法的命名清晰、有意义
- [ ] 遵循统一的缩进和格式规范
- [ ] 复杂逻辑有必要的注释说明
- [ ] 异常处理正确，使用了适当的日志记录
- [ ] 异步方法正确使用 `async`/`await`
- [ ] 没有硬编码的值，使用配置或常量代替
- [ ] 代码没有警告（Warning）

## 7 参考资料

- [微软 C# 编码约定](https://learn.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET 命名指南](https://learn.microsoft.com/zh-cn/dotnet/standard/design-guidelines/naming-guidelines)
- [C# 编程指南](https://learn.microsoft.com/zh-cn/dotnet/csharp/programming-guide/)
