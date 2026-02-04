# 跨域中间件

## 概述

Materal.MergeBlock 框架提供的跨域（Cross-Origin Resource Sharing, CORS）中间件，基于 ASP.NET Core 内置的 CORS 功能实现。该中间件为应用程序提供宽松的跨域配置，允许所有来源、请求头和方法的跨域请求。

## 安装

```bash
# 启动项目（Web.Host/Web.API 等）
dotnet add package Materal.MergeBlock.Cors
```

### 包说明

| 包名 | 说明 |
|------|------|
| `Materal.MergeBlock.Cors` | 包含跨域模块注册和中间件实现 |

## 模块依赖

`CorsModule` 依赖于 `WebModule`。

## 配置策略

该中间件使用**宽松的跨域策略**，默认配置如下：

```csharp
options.AddDefaultPolicy(builder =>
{
    builder.SetIsOriginAllowed(_ => true)   // 允许所有来源
           .AllowAnyHeader()                // 允许所有请求头
           .AllowAnyMethod()                // 允许所有 HTTP 方法
           .AllowCredentials();             // 允许携带凭据（Cookie/Authorization 头）
});
```

### 策略说明

| 配置项 | 说明 |
|--------|------|
| `SetIsOriginAllowed(_ => true)` | 允许任意域名/端口的跨域请求 |
| `AllowAnyHeader()` | 允许任意请求头 |
| `AllowAnyMethod()` | 允许任意 HTTP 方法（GET、POST、PUT、DELETE 等） |
| `AllowCredentials()` | 允许浏览器发送凭据信息（Cookie、Authorization 头等） |

## 使用方法

### 1. 简单使用（推荐）

只需安装 NuGet 包，框架会自动注册和启用跨域中间件：

```bash
dotnet add package Materal.MergeBlock.Cors
```

### 2. 在模块中引入

如果需要自定义模块依赖：

```csharp
[DependsOn(typeof(CorsModule))]
public class YourModule() : MergeBlockModule("你的模块")
{
    // 模块代码
}
```

### 3. 服务层使用

跨域配置对服务层透明，无需额外代码。服务方法可以正常处理来自不同源的请求。

### 4. 控制器使用

控制器无需特殊配置即可处理跨域请求：

```csharp
[ApiController]
[Route("api/[controller]")]
public class YourController : ControllerBase
{
    [HttpGet("data")]
    public IActionResult GetData()
    {
        // 来自不同域的请求可以正常访问此接口
        return Ok(new { Message = "跨域请求成功" });
    }
}
```

## 工作原理

```
客户端请求（跨域）
    ↓
Origin: https://example.com
    ↓
CorsMiddleware（CorsModule）
    ↓
检查 CORS 策略
    ↓
响应头添加：
- Access-Control-Allow-Origin: *
- Access-Control-Allow-Headers: *
- Access-Control-Allow-Methods: *
- Access-Control-Allow-Credentials: true
    ↓
继续处理请求
```

## 响应头说明

启用跨域后，响应会自动包含以下头：

| 响应头 | 值 | 说明 |
|--------|-----|------|
| `Access-Control-Allow-Origin` | `*` 或请求的 Origin | 允许访问的来源 |
| `Access-Control-Allow-Headers` | `*` | 允许的请求头 |
| `Access-Control-Allow-Methods` | `*` | 允许的 HTTP 方法 |
| `Access-Control-Allow-Credentials` | `true` | 允许携带凭据 |

## 自定义配置

### 自定义跨域策略

如果需要更严格的跨域控制，可以在模块中覆盖默认配置：

```csharp
[DependsOn(typeof(CorsModule))]
public class YourModule() : MergeBlockModule("你的模块")
{
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddCors(options =>
        {
            options.AddPolicy("CustomPolicy", builder =>
            {
                builder.WithOrigins("https://trusted-domain.com")
                       .WithHeaders("Content-Type", "Authorization")
                       .WithMethods("GET", "POST")
                       .AllowCredentials();
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // 移除默认策略，使用自定义策略
        AdvancedContext advancedContext = context.ServiceProvider.GetRequiredService<AdvancedContext>();
        if (advancedContext.App is IApplicationBuilder app)
        {
            app.UseCors("CustomPolicy");
        }
    }
}
```

### 禁用跨域

如需完全禁用跨域，不引入 `Materal.MergeBlock.Cors` 包即可。

## 注意事项

1. **安全性**：`AllowCredentials()` 配合 `AllowAnyOrigin` 在某些浏览器中可能不被接受。如需同时使用凭据和通配符，请使用 `WithOrigins` 明确指定允许的域名。

2. **预检请求**：浏览器会先发送 OPTIONS 预检请求，中间件会自动处理。

3. **顺序问题**：跨域中间件应在路由（Routing）和授权（Authorization）之前配置：
   ```
   UseCors → UseAuthentication → UseAuthorization → MapControllers
   ```

4. **生产环境**：生产环境建议使用明确的域名列表而非通配符。

## 与其他中间件集成

### 配合 JWT 认证

```csharp
[DependsOn(typeof(CorsModule))]
[DependsOn(typeof(AuthorizationModule))]
public class YourModule() : MergeBlockModule("你的模块")
{
    // 两个模块会自动按正确顺序配置
}
```

### 配合异常处理

```csharp
[DependsOn(typeof(CorsModule))]
[DependsOn(typeof(ExceptionInterceptorModule))]
public class YourModule() : MergeBlockModule("你的模块")
{
    // 跨域应在异常处理之前配置
}
```

## 常见问题

### Q: 跨域请求失败怎么办？

1. 确认已安装 `Materal.MergeBlock.Cors` 包
2. 检查浏览器控制台错误信息
3. 确认请求头不包含不被允许的自定义头
4. 检查 `AllowCredentials` 与来源设置是否冲突

### Q: 如何只允许特定域名？

参考 [自定义跨域策略](#自定义跨域策略) 部分，使用 `WithOrigins` 指定域名。

### Q: 为什么预检请求（OPTIONS）返回 204 但实际请求失败？

这通常是请求头或请求方法未在 CORS 策略中声明。检查：
- 请求头是否在 `WithHeaders` 或 `AllowAnyHeader` 中
- 请求方法是否在 `WithMethods` 或 `AllowAnyMethod` 中
