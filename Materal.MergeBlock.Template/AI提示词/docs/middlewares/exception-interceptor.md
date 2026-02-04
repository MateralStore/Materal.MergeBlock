# 异常处理中间件

## 概述

Materal.MergeBlock 框架提供的全局异常处理中间件，用于统一捕获和处理 Web API、后台服务以及流式输出过程中发生的异常。该中间件会将异常信息记录到日志，并根据配置决定是否向客户端返回详细错误信息。

## 安装

```bash
# 启动项目（Web.Host/Web.API 等）
dotnet add package Materal.MergeBlock.ExceptionInterceptor
```

### 包说明

| 包名 | 安装位置 | 说明 |
|------|----------|------|
| `Materal.MergeBlock.ExceptionInterceptor` | 启动项目 | 包含异常处理中间件、过滤器、模块注册 |

**注意**：`ExceptionInterceptorModule` 已在包中注册，**只需在启动项目安装此包即可**。

## 模块依赖

`ExceptionInterceptorModule` 依赖于 `WebModule`。

## 配置

在 `appsettings.json` 中添加异常处理配置：

```json
{
    "Exception": {
        "ShowException": false,
        "ErrorMessage": "服务出错了！请联系管理员。"
    }
}
```

### 配置项说明

| 配置项 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `ShowException` | bool | `false` | 是否向客户端显示异常详情，生产环境建议设为 `false` |
| `ErrorMessage` | string | `"服务出错了！请联系管理员。"` | 隐藏异常详情时返回给客户端的通用错误消息 |

### 配置类

```csharp
[Options("Exception")]
public class ExceptionOptions : IOptions
{
    public static string ConfigKey { get; } = "Exception";
    public bool ShowException { get; set; } = false;
    public string ErrorMessage { get; set; } = "服务出错了！请联系管理员。";
}
```

## 核心组件

### GlobalExceptionFilter

全局异常过滤器，用于捕获 MVC 控制器 Action 中发生的异常。

**处理规则：**

| 异常类型 | 处理方式 | HTTP 状态码 |
|----------|----------|-------------|
| `MergeBlockModuleException` | 返回 JSON 失败结果 | 200 |
| `ValidationException` | 返回 JSON 失败结果 | 200 |
| `HttpCodeException` | 返回 JSON 失败结果，设置状态码 | 异常指定的 HTTP 状态码 |
| 其他异常 | 记录详细日志，返回通用错误消息 | 500 |

**异常信息记录：**

当发生未处理的异常时，会记录以下信息到日志：
- 异常消息
- 控制器名称
- Action 名称
- 客户端 IP 地址
- 登录用户 ID（如果有）
- 请求参数内容

```csharp
public class GlobalExceptionFilter(IOptionsMonitor<ExceptionOptions> exceptionConfig, ILogger<GlobalExceptionFilter>? logger = null) : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        Exception exception = context.Exception;
        if (exception is MergeBlockModuleException or ValidationException)
        {
            if (exception is AggregateException aggregateException)
            {
                exception = aggregateException.InnerException ?? exception;
            }
            context.Result = new JsonResult(ResultModel.Fail(exception.Message));
            return;
        }
        else if (exception is HttpCodeException httpCodeException)
        {
            context.Result = new JsonResult(ResultModel.Fail(httpCodeException.Message));
            context.HttpContext.Response.StatusCode = httpCodeException.HttpCode;
            return;
        }
        // 记录详细日志
        string errorMessage = await GetErrorMessageAsync(context, exception);
        logger?.LogError(exception, errorMessage);
        // 返回通用错误消息
        string message = exceptionConfig.CurrentValue.ShowException ? exception.Message : exceptionConfig.CurrentValue.ErrorMessage;
        context.Result = new JsonResult(ResultModel.Fail(message));
        context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
    }
}
```

### StreamingExceptionMiddleware

流式输出异常处理中间件，专门用于处理 `IAsyncEnumerable` 流式响应过程中发生的异常。

**为什么需要单独的中间件？**

当使用流式响应（如 SSE 或流式 JSON）时，HTTP 响应头和部分内容已经发送给客户端，此时无法再修改 HTTP 状态码。因此，流式异常只能通过在响应流中写入特殊格式的错误信息来通知客户端。

**错误格式：**

```
[!!STREAM_ERROR_START!!]{"Code":500,"Message":"错误消息"}[!!STREAM_ERROR_END!!]
```

```csharp
public class StreamingExceptionMiddleware(RequestDelegate next, IOptionsMonitor<ExceptionOptions> exceptionConfig, ILogger<StreamingExceptionMiddleware>? logger = null)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (context.Response.HasStarted)
        {
            // 响应已开始，无法修改状态码，只能记录日志并写入错误信息
            string message;
            if (ex is MergeBlockModuleException or ValidationException or HttpCodeException)
            {
                message = ex.Message;
            }
            else
            {
                message = ex.GetErrorMessage();
            }
            logger?.LogError(ex, "流式输出过程中发生异常");
            if (!exceptionConfig.CurrentValue.ShowException)
            {
                message = exceptionConfig.CurrentValue.ErrorMessage;
            }
            ResultModel resultModel = ResultModel.Fail(message);
            string errorJson = resultModel.ToJson();
            await context.Response.WriteAsync($"\n[!!STREAM_ERROR_START!!]{errorJson}[!!STREAM_ERROR_END!!]\n");
        }
    }
}
```

### ExceptionInterceptorHostedServiceDecorator

后台服务异常装饰器，用于捕获 `IHostedService` 启动和运行过程中发生的异常。

```csharp
public class ExceptionInterceptorHostedServiceDecorator(ILogger<ExceptionInterceptorHostedServiceDecorator>? logger = null) : IHostedServiceDecorator
{
    public async Task<bool> OnExceptionAsync(CancellationToken cancellationToken, Exception exception)
    {
        if (exception is MergeBlockModuleException or ValidationException)
        {
            logger?.LogError(exception, exception.Message);
        }
        else
        {
            logger?.LogCritical(exception, exception.Message);
        }
        return await Task.FromResult(true);
    }
}
```

## 使用方法

### 1. 基础使用

安装 NuGet 包后，无需额外配置，中间件会自动注册：

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 添加 MergeBlock 模块
builder.Services.AddMergeBlockModules();
// 或手动注册模块（推荐显式注册，便于管理依赖）
// builder.Services.AddMergeBlockModule<ExceptionInterceptorModule>();

var app = builder.Build();

// 初始化模块（会自动注册中间件和过滤器）
app.InitializeModules();

app.Run();
```

### 2. 自定义错误消息

在 `appsettings.json` 中配置自定义错误消息：

```json
{
    "Exception": {
        "ShowException": false,
        "ErrorMessage": "系统维护中，请稍后再试。"
    }
}
```

### 3. 开发环境显示详细异常

开发环境可以启用详细异常显示：

```json
{
    "Exception": {
        "ShowException": true,
        "ErrorMessage": "服务出错了！请联系管理员。"
    }
}
```

**注意**：生产环境务必将 `ShowException` 设为 `false`，避免泄露敏感信息。

### 4. 使用 HttpCodeException 抛出带状态码的异常

在服务层或控制器中，可以使用 `HttpCodeException` 抛出带有自定义 HTTP 状态码的异常：

```csharp
public class UserServiceImpl : IUserService
{
    public async Task<UserDto> GetUserAsync(Guid id)
    {
        User? user = await _repository.GetAsync(id);
        if (user == null)
        {
            throw new HttpCodeException(HttpStatusCode.NotFound, $"用户不存在：{id}");
        }
        return _mapper.Map<UserDto>(user);
    }
}
```

### 5. 使用 ValidationException 进行参数验证

```csharp
public class UserServiceImpl : IUserService
{
    public async Task CreateUserAsync(CreateUserRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            throw new ValidationException("用户名不能为空");
        }
        if (request.Password.Length < 6)
        {
            throw new ValidationException("密码长度不能少于6位");
        }
        // ...
    }
}
```

### 6. 流式响应异常处理

使用流式响应时，客户端需要解析错误标记：

```csharp
// 服务器端
[HttpGet("stream")]
public async IAsyncEnumerable<int> StreamData([FromQuery] int count)
{
    for (int i = 0; i < count; i++)
    {
        if (i == 5)
        {
            throw new Exception("模拟流式异常");
        }
        yield return i;
        await Task.Delay(100);
    }
}
```

```csharp
// 客户端解析
async Task ConsumeStream()
{
    using var response = await httpClient.GetAsync("api/test/stream");
    using var stream = await response.Content.ReadAsStreamAsync();
    using var reader = new StreamReader(stream);

    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        if (line.StartsWith("[!!STREAM_ERROR_START!!]"))
        {
            // 解析流式错误
            string errorJson = line
                .Replace("[!!STREAM_ERROR_START!!]", "")
                .Replace("[!!STREAM_ERROR_END!!]", "");
            var error = JsonSerializer.Deserialize<ResultModel>(errorJson);
            Console.WriteLine($"流式错误：{error?.Message}");
            break;
        }
        Console.WriteLine($"收到数据：{line}");
    }
}
```

## 异常处理流程

```
客户端请求
    ↓
UseMiddleware<StreamingExceptionMiddleware>（流式异常处理）
    ↓
MVC 控制器处理请求
    ↓
GlobalExceptionFilter 全局异常拦截
    ├─ MergeBlockModuleException / ValidationException → 返回业务失败
    ├─ HttpCodeException → 返回对应状态码
    └─ 其他异常 → 记录日志，返回 500
    ↓
响应给客户端
```

### 后台服务异常流程

```
IHostedService 启动/运行
    ↓
ExceptionInterceptorHostedServiceDecorator 装饰
    ↓
OnExceptionAsync 捕获异常
    ├─ MergeBlockModuleException / ValidationException → LogError
    └─ 其他异常 → LogCritical
    ↓
服务继续运行
```

## 最佳实践

### 1. 业务异常使用 MergeBlockModuleException

对于需要向用户返回错误信息的业务异常，使用 `MergeBlockModuleException`：

```csharp
throw new MergeBlockModuleException("用户名已存在");
```

### 2. 参数验证使用 ValidationException

对于参数验证失败的情况，使用 `ValidationException`：

```csharp
if (id == Guid.Empty)
{
    throw new ValidationException("ID 不能为空");
}
```

### 3. HTTP 状态码异常使用 HttpCodeException

当需要设置特定 HTTP 状态码时，使用 `HttpCodeException`：

```csharp
throw new HttpCodeException(HttpStatusCode.Forbidden, "没有操作权限");
```

### 4. 生产环境隐藏异常详情

生产环境务必配置：

```json
{
    "Exception": {
        "ShowException": false,
        "ErrorMessage": "系统繁忙，请稍后再试。"
    }
}
```

### 5. 日志记录最佳实践

中间件会自动记录异常详情到日志，包括：
- 异常堆栈
- 控制器/Action 名称
- 客户端 IP
- 登录用户 ID
- 请求参数

建议配置日志级别为 `Error` 或 `Warning` 以捕获这些异常日志。

## 扩展自定义

### 1. 自定义异常类型

可以继承 `MergeBlockModuleException` 或 `HttpCodeException` 创建自定义异常：

```csharp
public class BusinessException : MergeBlockModuleException
{
    public BusinessException(string message) : base(message) { }
}

// 使用
throw new BusinessException("自定义业务错误");
```

### 2. 自定义错误响应格式

如果需要自定义错误响应格式，可以创建自定义过滤器替换 `GlobalExceptionFilter`：

```csharp
public class CustomExceptionFilter : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        // 自定义错误响应格式
        var customResult = new
        {
            Success = false,
            Code = 500,
            Message = GetErrorMessage(context.Exception),
            Timestamp = DateTime.UtcNow
        };
        context.Result = new JsonResult(customResult);
    }
}
```

在模块中替换默认过滤器：

```csharp
[DependsOn(typeof(ExceptionInterceptorModule))]
public class YourModule() : MergeBlockModule("你的模块")
{
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.RemoveAll<GlobalExceptionFilter>();
        context.Services.AddSingleton<GlobalExceptionFilter, CustomExceptionFilter>();
    }
}
```

### 3. 自定义流式错误标记

如果需要修改流式错误的标记格式，可以创建自定义中间件：

```csharp
public class CustomStreamingExceptionMiddleware(RequestDelegate next, IOptionsMonitor<ExceptionOptions> exceptionConfig, ILogger<CustomStreamingExceptionMiddleware>? logger = null)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (context.Response.HasStarted)
        {
            string message = exceptionConfig.CurrentValue.ShowException ? ex.Message : exceptionConfig.CurrentValue.ErrorMessage;
            var errorResult = new { error = message };
            string errorJson = errorResult.ToJson();
            // 自定义标记格式
            await context.Response.WriteAsync($"\n<ERROR>{errorJson}</ERROR>\n");
            logger?.LogError(ex, "流式输出过程中发生异常");
        }
    }
}
```

## 常见问题

### Q1: 异常被捕获但返回 200 状态码？

这是正常行为。`MergeBlockModuleException` 和 `ValidationException` 被视为业务异常，框架返回 200 状态码和业务失败响应。只有系统异常才会返回 500 状态码。

### Q2: 流式响应中的错误如何识别？

流式响应中的错误使用 `[!!STREAM_ERROR_START!!]` 和 `[!!STREAM_ERROR_END!!]` 标记包裹。客户端需要检测这些标记并解析其中的 JSON 数据。

### Q3: 如何跳过异常处理？

目前不支持跳过异常处理。所有异常都会被统一处理，这符合最佳实践。

### Q4: 生产环境异常信息会泄露吗？

不会。当 `ShowException = false`（默认值）时，所有异常都会返回配置的 `ErrorMessage`，不会泄露堆栈等敏感信息。

### Q5: 后台服务异常会阻止服务启动吗？

`ExceptionInterceptorHostedServiceDecorator` 会捕获异常并记录日志，但不会阻止服务启动或继续运行。这确保了单个后台服务的异常不会影响其他服务。
