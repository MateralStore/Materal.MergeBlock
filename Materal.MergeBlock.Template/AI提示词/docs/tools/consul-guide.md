# Consul 服务注册与发现工具

## 概述

`Materal.Utils.Consul` 是一个 Consul 服务注册与发现工具库，提供服务注册、服务发现、健康检查等功能，基于官方 `Consul.NET` 客户端封装。

## 安装

```bash
dotnet add package Materal.Utils.Consul
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

### 依赖包

- `Materal.Utils`（基础工具包）
- `Consul`（官方客户端）

## 配置模型

### ConsulConfig - Consul 配置

服务注册的核心配置类，包含 Consul 地址、服务地址和健康检查配置。

```csharp
using Materal.Utils.Consul.ConfigModels;

ConsulConfig config = new()
{
    Enable = true,
    ServiceName = "MyApiService",
    Tags = ["v1", "production"],
    ConsulUrl = new()
    {
        IsSSL = false,
        Host = "127.0.0.1",
        Port = 8500
    },
    ServiceUrl = new()
    {
        IsSSL = false,
        Host = "localhost",
        Port = 5000
    },
    Health = new()
    {
        Interval = 30,
        Url = new()
        {
            IsSSL = false,
            Host = "localhost",
            Port = 5000,
            Path = "/api/Health"
        }
    }
};
```

**属性说明**：

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enable | bool | true | 是否启用服务注册 |
| ServiceName | string | "MyService" | 服务名称 |
| Tags | string[] | [] | 服务标签 |
| ConsulUrl | HttpUrlModel | - | Consul 服务器地址 |
| ServiceUrl | HttpUrlModel | - | 本地服务地址 |
| Health | HealthConfig | - | 健康检查配置 |

### HealthConfig - 健康检查配置

```csharp
using Materal.Utils.Consul.ConfigModels;

HealthConfig healthConfig = new()
{
    Interval = 30,          // 检查间隔（秒）
    Url = new()
    {
        IsSSL = false,
        Host = "localhost",
        Port = 5000,
        Path = "/api/Health"
    }
};
```

**属性说明**：

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Interval | int | 30 | 健康检查间隔（秒） |
| Url | HttpUrlModel | - | 健康检查端点地址 |

### ConsulServiceModel - 服务信息模型

从 Consul 查询到的服务信息。

```csharp
using Materal.Utils.Consul.Models;

ConsulServiceModel serviceModel = new()
{
    ID = "service-id",
    Service = "MyApiService",
    Tags = ["v1"],
    Address = "localhost",
    Port = 5000,
    Datacenter = "dc1"
};
```

**属性说明**：

| 属性 | 类型 | 说明 |
|------|------|------|
| ID | string? | 服务唯一标识 |
| Service | string? | 服务名称 |
| Tags | string[]? | 服务标签 |
| Port | int | 服务端口 |
| Address | string? | 服务地址 |
| SocketPath | string? | Socket路径 |
| EnableTagOverride | bool? | 启用标签重写 |
| Datacenter | string? | 数据中心 |

## 服务接口

### IConsulService - 服务注册与发现接口

通过依赖注入获取服务实例：

```csharp
using Materal.Utils.Consul;

public class MyService(IConsulService consulService)
{
    private readonly IConsulService _consulService = consulService;
}
```

**服务注册方法**：

| 方法 | 说明 |
|------|------|
| RegisterConsulConfigAsync | 注册并保存 Consul 配置 |
| RegisterConsulAsync | 注册服务到 Consul |
| RegisterAllConsulAsync | 注册所有已配置的服务 |

**服务注销方法**：

| 方法 | 说明 |
|------|------|
| UnregisterConsulAsync | 从 Consul 注销服务 |
| UnregisterAllConsulAsync | 注销所有已注册的服务 |

**服务发现方法**：

| 方法 | 说明 |
|------|------|
| GetServiceInfoAsync | 查询单个服务信息 |
| GetServiceListAsync | 查询服务列表 |
| HasNode | 检查节点是否存在 |

**配置管理方法**：

| 方法 | 说明 |
|------|------|
| ChangeConsulConfigAsync(ConsulConfig, ConsulConfig) | 根据旧配置更改 |
| ChangeConsulConfigAsync(string, ConsulConfig) | 根据服务名称更改 |
| ChangeConsulConfigAsync(Guid, ConsulConfig) | 根据节点ID更改 |
| ChangeConsulConfigAsync(ConsulScope?, ConsulConfig) | 根据ConsulScope更改 |

### ConsulScope - 服务作用域

`ConsulScope` 是服务注册与健康检查的核心管理类，由 `IConsulService` 的 `RegisterConsulConfigAsync` 方法返回。

```csharp
using Materal.Utils.Consul;

public class ConsulHostedService(IHostApplicationLifetime lifetime, IConsulService consulService)
{
    private readonly IConsulService _consulService = consulService;
    private ConsulScope? _consulScope;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        ConsulConfig config = new()
        {
            Enable = true,
            ServiceName = "MyApiService",
            ConsulUrl = new() { Host = "127.0.0.1", Port = 8500 },
            ServiceUrl = new() { Host = "localhost", Port = 5000 },
            Health = new() { Interval = 30, Url = new() { Port = 5000, Path = "/api/Health" } }
        };
        _consulScope = await _consulService.RegisterConsulConfigAsync(config);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consulScope != null)
        {
            await _consulScope.UnregisterConsulAsync();
        }
    }
}
```

**ConsulScope 属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| IsRegister | bool | 是否已注册 |
| Config | ConsulConfig | 服务配置 |
| NodeID | Guid | 节点唯一标识 |

**ConsulScope 方法**：

| 方法 | 说明 |
|------|------|
| ChangeConfigAsync | 更改配置 |
| RegisterConsulAsync | 注册服务 |
| UnregisterConsulAsync | 注销服务 |
| GetServiceInfoAsync | 获取服务信息 |
| GetServiceListAsync | 获取服务列表 |

## 服务注册与注销

### 注册单个服务

使用 `ConsulScope`（推荐）：

```csharp
using Materal.Utils.Consul;
using Materal.Utils.Consul.ConfigModels;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMateralConsulUtils();
    }
}

public class ConsulHostedService(IHostApplicationLifetime lifetime, IConsulService consulService)
{
    private readonly IConsulService _consulService = consulService;
    private ConsulScope? _consulScope;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        ConsulConfig config = new()
        {
            Enable = true,
            ServiceName = "MyApiService",
            ConsulUrl = new() { Host = "127.0.0.1", Port = 8500 },
            ServiceUrl = new() { Host = "localhost", Port = 5000 },
            Health = new() { Interval = 30, Url = new() { Port = 5000, Path = "/api/Health" } }
        };
        _consulScope = await _consulService.RegisterConsulConfigAsync(config);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consulScope != null)
        {
            await _consulScope.UnregisterConsulAsync();
        }
    }
}
```

使用 `RegisterConsulAsync` 直接注册：

```csharp
using Materal.Utils.Consul;
using Materal.Utils.Consul.ConfigModels;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMateralConsulUtils();
    }
}

public class ConsulHostedService(IHostApplicationLifetime lifetime, IConsulService consulService)
{
    private readonly IConsulService _consulService = consulService;
    private ConsulConfig? _consulConfig;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _consulConfig = new ConsulConfig
        {
            Enable = true,
            ServiceName = "MyApiService",
            ConsulUrl = new() { Host = "127.0.0.1", Port = 8500 },
            ServiceUrl = new() { Host = "localhost", Port = 5000 },
            Health = new() { Interval = 30, Url = new() { Port = 5000, Path = "/api/Health" } }
        };

        await _consulService.RegisterConsulAsync(_consulConfig);
        // 或根据节点ID注册（Guid重载）
        // await _consulService.RegisterConsulAsync(nodeID);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consulConfig != null)
        {
            await _consulService.UnregisterConsulAsync(_consulConfig);
        }
    }
}
```

### 注册所有服务

```csharp
// 注册所有已配置但未注册的服务
await _consulService.RegisterAllConsulAsync();
```

### 注销服务

```csharp
// 根据配置注销
await _consulService.UnregisterConsulAsync(consulConfig);

// 根据节点ID注销（Guid重载）
await _consulService.UnregisterConsulAsync(nodeID);

// 注销所有已注册的服务
await _consulService.UnregisterAllConsulAsync();
```

### 更改服务配置

```csharp
// 根据旧配置更新
await _consulService.ChangeConsulConfigAsync(oldConfig, newConfig);

// 根据服务名称更新
await _consulService.ChangeConsulConfigAsync("MyApiService", newConfig);

// 根据节点ID更新
await _consulService.ChangeConsulConfigAsync(nodeID, newConfig);
```

## 服务发现

### 查询单个服务

```csharp
using Materal.Utils.Consul;
using Materal.Utils.Consul.ConfigModels;

// 根据配置查询
ConsulServiceModel? service = await _consulService.GetServiceInfoAsync(config);

// 根据节点ID查询
ConsulServiceModel? service = await _consulService.GetServiceInfoAsync(nodeID);

// 根据Consul地址查询
ConsulServiceModel? service = await _consulService.GetServiceInfoAsync("http://127.0.0.1:8500");

// 带筛选条件查询
ConsulServiceModel? service = await _consulService.GetServiceInfoAsync(config,
    m => m.Tags != null && m.Tags.Contains("v1"));
```

### 查询服务列表

```csharp
using Materal.Utils.Consul;
using Materal.Utils.Consul.ConfigModels;

// 查询所有服务
List<ConsulServiceModel> services = await _consulService.GetServiceListAsync(config);

// 带筛选条件查询
List<ConsulServiceModel> services = await _consulService.GetServiceListAsync(config,
    m => m.Service == "MyApiService");

// 根据Consul地址查询
List<ConsulServiceModel> services = await _consulService.GetServiceListAsync("http://127.0.0.1:8500");

// 根据节点ID查询（Guid重载）
List<ConsulServiceModel> services = await _consulService.GetServiceListAsync(nodeID);
```

### 检查节点是否存在

```csharp
bool hasNode = _consulService.HasNode(nodeID);
```

## 健康检查

健康检查由 `ConsulScope` 内部管理，包含以下功能：

1. **自动注册**：服务启动时自动注册到 Consul
2. **定时检查**：按配置的间隔发送健康检查请求
3. **自动重注册**：健康检查失败后自动重新注册服务
4. **优雅注销**：服务停止时自动注销

健康检查请求会验证服务是否仍在 Consul 注册列表中：

```csharp
private async Task<bool> SendHealthRequestAsync()
{
    ConsulServiceModel? service = await GetServiceInfoAsync(
        m => m.ID is not null && m.ID == NodeID.ToString());
    return service is not null;
}
```

### 自定义健康检查端点

实现一个健康检查端点：

```csharp
using Microsoft.AspNetCore.Mvc;

namespace MyApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
```

## 完整使用示例

### ASP.NET Core 集成

```csharp
using Materal.Utils.Consul;
using Materal.Utils.Consul.ConfigModels;

namespace MyApiService;

public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddControllers();
                services.AddMateralConsulUtils();
                services.AddHostedService<ConsulHostedService>();
            })
            .Build()
            .Run();
    }
}

public class ConsulHostedService(IHostApplicationLifetime lifetime, IConsulService consulService)
    : IHostedService
{
    private readonly IConsulService _consulService = consulService;
    private ConsulConfig? _consulConfig;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _consulConfig = new ConsulConfig
        {
            Enable = true,
            ServiceName = "MyApiService",
            Tags = ["v1", "production"],
            ConsulUrl = new()
            {
                Host = "127.0.0.1",
                Port = 8500
            },
            ServiceUrl = new()
            {
                Host = "localhost",
                Port = 5000
            },
            Health = new()
            {
                Interval = 30,
                Url = new()
                {
                    Host = "localhost",
                    Port = 5000,
                    Path = "/api/Health"
                }
            }
        };

        await _consulService.RegisterConsulAsync(_consulConfig);
        // 或根据节点ID注册（Guid重载）
        // await _consulService.RegisterConsulAsync(nodeID);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consulConfig != null)
        {
            await _consulService.UnregisterConsulAsync(_consulConfig);
        }
    }
}
```

### 服务发现与调用

```csharp
using Materal.Utils.Consul;
using Materal.Utils.Consul.ConfigModels;
using Materal.Utils.Network.Http;

namespace MyApiService.Services;

public class ServiceDiscoveryService(IHttpHelper httpHelper, IConsulService consulService)
{
    private readonly IHttpHelper _httpHelper = httpHelper;
    private readonly IConsulService _consulService = consulService;

    /// <summary>
    /// 发现服务并调用
    /// </summary>
    public async Task<string> CallServiceAsync(string targetServiceName)
    {
        ConsulConfig config = new()
        {
            ServiceName = targetServiceName,
            ConsulUrl = new() { Host = "127.0.0.1", Port = 8500 }
        };

        ConsulServiceModel? service = await _consulService.GetServiceInfoAsync(config);
        if (service == null) throw new {ProjectName}Exception($"服务 {targetServiceName} 未找到", StatusCodes.Status404NotFound);

        string url = $"http://{service.Address}:{service.Port}/api/data";
        return await _httpHelper.SendGetAsync(url);
    }

    /// <summary>
    /// 获取所有可用服务实例
    /// </summary>
    public async Task<List<ConsulServiceModel>> GetAllServicesAsync()
    {
        ConsulConfig config = new()
        {
            ServiceName = "MyApiService",
            ConsulUrl = new() { Host = "127.0.0.1", Port = 8500 }
        };

        return await _consulService.GetServiceListAsync(config);
    }
}
```

### 动态服务配置更新

```csharp
using Materal.Utils.Consul;
using Materal.Utils.Consul.ConfigModels;

namespace MyApiService.Services;

public class DynamicConfigService(IConsulService consulService)
{
    private readonly IConsulService _consulService = consulService;
    private Guid _nodeID;

    /// <summary>
    /// 动态更新服务配置
    /// </summary>
    /// <param name="newPort">新端口号</param>
    public async Task UpdateServiceConfigAsync(int newPort)
    {
        ConsulConfig newConfig = new()
        {
            Enable = true,
            ServiceName = "MyApiService",
            ConsulUrl = new() { Host = "127.0.0.1", Port = 8500 },
            ServiceUrl = new() { Host = "localhost", Port = newPort },
            Health = new() { Interval = 30, Url = new() { Port = newPort, Path = "/api/Health" } }
        };

        _nodeID = await _consulService.ChangeConsulConfigAsync(_nodeID, newConfig);
    }
}
```

## 异常处理

### MateralConsulException

自定义异常类型，用于处理 Consul 操作错误：

```csharp
using Materal.Utils.Consul;

try
{
    ConsulServiceModel? service = await _consulService.GetServiceInfoAsync(nodeID);
}
catch (MateralConsulException ex)
{
    Console.WriteLine($"Consul错误: {ex.Message}");
}
```

## 日志记录

该库使用 Microsoft.Extensions.Logging 进行日志记录，日志类别为 `ConsulUtils`：

```csharp
// 在日志中可以看到以下信息：
// - 服务注册/注销状态
// - 健康检查结果
// - 重试操作日志
```

## 最佳实践

1. **使用托管服务**：在 `IHostedService` 中管理注册和注销
2. **优雅关闭**：实现 `IHostedService` 或使用 `IHostApplicationLifetime` 确保服务停止时正确注销
3. **健康检查**：实现健康检查端点，确保 Consul 可以监控服务状态
4. **配置验证**：在注册前验证配置有效性
5. **错误处理**：捕获 `MateralConsulException` 进行错误处理
