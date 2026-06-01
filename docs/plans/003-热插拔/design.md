# Materal.MergeBlock 热插拔设计草案

## 背景

Materal.MergeBlock 当前已经具备插件化和模块化基础。框架可以从应用根目录、`Plugins` 目录和配置文件读取插件，使用独立的 `PluginLoadContext` 加载程序集，扫描带有 `MergeBlockAssemblyAttribute` 的程序集，并通过 `IMergeBlockModule` 完成服务注册和应用初始化。

这说明 MergeBlock 已经不是传统的单体启动模型，而是具备“启动期插件装配”的基础。但当前插件加载、依赖注入、MVC ApplicationPart、模块初始化和后台任务启动都发生在应用启动阶段。主 `IServiceProvider` 构建完成后，现有设计不能直接把新的模块服务注册进主容器，也不能安全地从运行中的 ASP.NET Core 管道中移除 Controller、DbContext、HostedService、SignalR Hub 或静态事件引用。

因此，MergeBlock 热插拔是现实的，但需要分阶段演进。第一阶段应先把现有插件机制整理成稳定的运行时模型，第二阶段支持运行时加载和启用，第三阶段再支持可验证的安全卸载。

## 目标

- 让 MergeBlock 支持在运行时发现、加载、启用、停用和卸载插件。
- 保留现有 `IMergeBlockModule` 启动期模块模型，避免破坏既有应用。
- 引入清晰的插件生命周期，覆盖加载、启动、停止、卸载和失败回滚。
- 支持插件依赖排序、依赖版本检查和依赖插件的联动停用。
- 支持插件级服务容器，避免依赖主容器运行时可变。
- 支持 Web、后台任务、调度任务、配置、日志、审计等常见插件能力。
- 支持卸载后验证 `AssemblyLoadContext` 是否真正可回收。
- 提供管理接口或管理服务，允许查看插件状态、启用、停用、重新加载和诊断失败原因。
- 先保证单进程热插拔正确，再扩展到多实例部署和分布式协调。

## 非目标

- 不承诺第一版支持所有现有 `IMergeBlockModule` 无改造热卸载。
- 不把主 `IServiceCollection` 设计成运行时可变容器。
- 不默认允许插件在运行时修改主机级 Kestrel、日志 Provider、认证 Scheme 等不可安全回滚的设置。
- 不默认自动执行数据库迁移。
- 不默认允许不可信来源上传并执行程序集。
- 不在第一版实现跨机器插件分发、灰度发布和分布式一致性。
- 不把热插拔和业务租户隔离混为一套机制。

## 现状判断

现有设计中已经可复用的能力：

- `PluginAccessor` 负责从多个来源发现插件。
- `Plugin` 负责创建 `PluginLoadContext` 并加载插件程序集。
- `PluginLoadContext` 已经继承 `AssemblyLoadContext`，并支持 `isCollectible`。
- `PluginManager` 已经支持插件依赖排序、模块加载、服务导出和 Options 配置。
- `ModuleLoader` 已经支持根据 `DependsOnAttribute` 查找模块依赖并生成 `ModuleDescriptor`。
- `MergeBlockContext` 已经保存插件、模块描述符和 MergeBlock 程序集列表。

现有设计中的关键限制：

- 插件加载发生在 `AddMergeBlockCore` 阶段，服务容器随后被 `Build()` 固化。
- `ExposeServices` 直接把插件服务注册到主 `IServiceCollection`，运行时无法安全撤销。
- Web 模块在启动期调用 `AddControllers().AddApplicationPart(...)` 和 `MapControllers()`，运行时新增或移除 Controller 需要额外刷新 MVC ActionDescriptor 或改用动态路由。
- 模块只有初始化生命周期，没有停止和卸载生命周期。
- 后台任务、Timer、静态事件、SignalR Hub、EF Core DbContext 等资源没有插件级释放约定。
- `ModuleLoader.ModuleDescriptors` 是全局静态列表，运行时增删需要并发控制和快照机制。

## 术语

- 插件包：部署到磁盘上的一个插件目录，包含程序集、配置、manifest 和依赖文件。
- 插件实例：运行时加载后的插件对象，包含加载上下文、服务容器、模块、状态和资源引用。
- 插件状态：插件从发现到卸载之间的状态，例如 Discovered、Loaded、Starting、Running、Stopping、Stopped、Failed、Unloading、Unloaded。
- 启动期插件：应用启动时加载并参与主服务容器构建的插件。
- 运行时插件：应用启动后由热插拔运行时加载，并在插件级容器中运行的插件。
- 可卸载插件：没有被主容器、静态引用、后台线程、路由缓存等对象持有，停止后可释放 `AssemblyLoadContext` 的插件。

## 设计原则

- 主容器稳定，插件容器可变。主机只注册热插拔运行时和稳定代理，不在运行时修改主容器。
- 先启用，后卸载。运行时加载比运行时卸载简单，应先实现加载和停用，再实现严格卸载。
- 插件能力显式声明。插件不能隐式修改任意主机能力，必须通过 manifest 和生命周期接口声明自己需要的能力。
- 卸载必须可验证。调用 `Unload()` 不代表卸载成功，必须用 `WeakReference` 和 GC 验证。
- 默认保守。对 Controller、DbContext、HostedService、认证、全局中间件等高风险能力设置更严格的接入方式。
- 失败可回滚。插件启动失败时，应撤销已注册的动态路由、后台任务、调度任务和插件状态。
- 兼容现有模块。现有模块仍可作为启动期插件使用，热插拔能力通过新增接口逐步增强。

## 分阶段路线

### 阶段一：启动期插件规范化

目标是把当前插件模型整理成后续热插拔可以复用的基础。

范围：

- 增加插件 manifest 约定。
- 整理插件发现、依赖排序、版本校验和状态记录。
- 引入插件生命周期状态机，但仍在启动期执行。
- 将现有 `PluginManager` 的职责拆分为插件发现、加载、模块解析、生命周期协调。
- 为现有启动期插件保留兼容路径。
- 增加插件加载失败的清晰错误信息和诊断日志。

阶段一完成后，插件仍需要重启应用才能生效，但插件包格式、状态和依赖模型已经稳定。

### 阶段二：运行时加载和启用

目标是在应用运行中加载新插件，并允许插件暴露受控能力。

范围：

- 新增 `IPluginRuntime`，负责加载、启用、停用和状态查询。
- 为运行时插件创建独立 `AssemblyLoadContext` 和插件级 `IServiceProvider`。
- 支持插件级服务解析、日志、配置和 Options。
- 支持插件动态任务、插件调度任务和插件声明式 Web Endpoint。
- 支持文件监听或管理 API 触发加载。
- 支持启动失败回滚。

阶段二可以实现“不重启加载新插件”，但卸载只要求业务能力停用，不要求 ALC 必须被回收。

### 阶段三：安全卸载和重新加载

目标是插件停用后可以释放程序集文件，并允许同名插件重新部署新版本。

范围：

- 增加停止和卸载生命周期。
- 对插件请求进行计数和排空。
- 停止插件后台任务、调度任务、Timer、Channel、事件订阅和外部连接。
- 移除动态路由和插件元数据。
- Dispose 插件级服务容器。
- 调用 `AssemblyLoadContext.Unload()` 并用 `WeakReference` 验证。
- 卸载失败时进入 `UnloadFailed` 状态，并给出引用泄漏诊断。

阶段三完成后，MergeBlock 才能称为具备完整热插拔能力。

### 阶段四：分布式热插拔

目标是在多实例部署下协调插件版本和启停状态。

范围：

- 插件包存储和分发。
- 多实例状态同步。
- 灰度启用和回滚。
- 插件健康检查和流量切换。
- 插件操作审计。

阶段四不进入第一轮实现范围。

## 总体架构

建议将热插拔拆成以下层次：

```text
管理入口
  -> IPluginRuntime
  -> PluginCatalog
  -> PluginLoader
  -> PluginLifecycleCoordinator
  -> PluginDependencyGraph
  -> PluginInstance
       -> PluginLoadContext
       -> PluginServiceProvider
       -> PluginModules
       -> PluginResources
  -> 能力适配器
       -> Web Endpoint Adapter
       -> Background Task Adapter
       -> Oscillator Adapter
       -> Configuration Adapter
       -> Health Check Adapter
```

主应用只持有 `IPluginRuntime`、插件注册表、插件状态和稳定的能力代理。插件自己的服务、控制器、任务和资源都应尽量保存在 `PluginInstance` 内。

## 建议项目结构

热插拔会触及 MergeBlock 核心，但仍建议把稳定抽象和管理能力拆出来：

```text
Materal.MergeBlock.Abstractions
  PluginManifest
  PluginState
  PluginLifecycleContext
  IHotPlugModule
  IPluginRuntime
  IPluginEndpointContributor
  IPluginBackgroundTask

Materal.MergeBlock
  PluginRuntime
  PluginCatalog
  PluginLoader
  PluginLifecycleCoordinator
  PluginDependencyGraph
  PluginInstance
  PluginUnloadVerifier

Materal.MergeBlock.Web
  PluginEndpointDataSource
  PluginRouteManager
  PluginRequestTracker

Materal.MergeBlock.HotPlug.Web
  插件管理 API
  插件状态查询 API
  插件启停 API
```

`Materal.MergeBlock.HotPlug.Web` 只承载管理接口，不承担核心加载逻辑。核心加载逻辑仍应在 `Materal.MergeBlock` 中，否则无法与现有启动流程和 `MergeBlockContext` 协调。

## 插件包格式

运行时插件目录建议采用版本化目录，避免 DLL 文件锁和半更新状态：

```text
Plugins
  DemoPlugin
    1.0.0
      plugin.json
      DemoPlugin.Application.dll
      DemoPlugin.Abstractions.dll
      appsettings.plugin.json
      runtimes
      resources
    current.json
```

`current.json` 指向当前启用版本。上传或复制新版本时先进入临时目录，校验通过后再切换版本指针。

manifest 示例：

```json
{
  "Name": "DemoPlugin",
  "DisplayName": "Demo 插件",
  "Version": "1.0.0",
  "EntryAssembly": "DemoPlugin.Application",
  "StartModule": "DemoPlugin.Application.DemoPluginModule",
  "PluginType": ["Module", "Service", "Web", "BackgroundTask"],
  "Dependencies": [
    {
      "Name": "CorePlugin",
      "VersionRange": "[1.0.0,2.0.0)"
    }
  ],
  "IsCollectible": true,
  "AllowRuntimeLoad": true,
  "AllowRuntimeUnload": true
}
```

manifest 职责：

- 明确插件名称、版本和入口模块。
- 明确依赖插件和版本范围。
- 明确插件能力类型。
- 明确是否允许运行时加载和卸载。
- 为后续签名、哈希、权限、兼容性检查保留扩展字段。

## 生命周期设计

建议新增可选接口，不直接修改 `IMergeBlockModule`，避免破坏现有模块：

```csharp
public interface IHotPlugModule
{
    Task OnPluginLoadingAsync(PluginLifecycleContext context);
    Task OnPluginLoadedAsync(PluginLifecycleContext context);
    Task OnPluginStartingAsync(PluginLifecycleContext context);
    Task OnPluginStartedAsync(PluginLifecycleContext context);
    Task OnPluginStoppingAsync(PluginLifecycleContext context);
    Task OnPluginStoppedAsync(PluginLifecycleContext context);
    Task OnPluginUnloadingAsync(PluginLifecycleContext context);
    Task OnPluginUnloadedAsync(PluginLifecycleContext context);
}
```

现有 `IMergeBlockModule` 继续用于启动期模块。运行时插件如果只实现 `IMergeBlockModule`，框架可以加载模块元数据，但不应默认把它的服务注册进主容器。要获得完整热插拔能力，插件应实现热插拔接口或使用热插拔专用贡献器。

插件状态流转：

```text
Discovered
  -> Loading
  -> Loaded
  -> Starting
  -> Running
  -> Stopping
  -> Stopped
  -> Unloading
  -> Unloaded
```

异常状态：

```text
LoadingFailed
StartingFailed
StoppingFailed
UnloadFailed
```

状态流转必须串行化。同一个插件同一时间只能执行一个生命周期操作。

## 依赖注入设计

主 `IServiceProvider` 构建后不可变，因此热插拔不能依赖运行时修改主容器。

推荐设计：

- 主容器注册 `IPluginRuntime`、插件状态存储、插件路由管理器和稳定代理。
- 每个运行时插件创建自己的 `ServiceCollection` 和 `ServiceProvider`。
- 插件服务可以依赖主容器中明确暴露的稳定服务，例如日志、配置、认证上下文、事件总线抽象。
- 主应用访问插件能力时，通过 `PluginInstance.ServiceProvider` 或插件能力代理解析。
- 插件停用时先停止入口能力，再 Dispose 插件级 `ServiceProvider`。

服务可见性建议：

```text
主服务 -> 可被插件读取的稳定服务
插件服务 -> 只在插件内部和插件代理中解析
插件服务 -> 默认不反向注册到主容器
```

如果某些插件确实需要向主应用暴露服务，应通过显式能力注册完成，例如 `IPluginCommand`、`IPluginEndpointContributor`、`IPluginBackgroundTask`，而不是直接把任意服务加入主容器。

## Web 能力设计

当前 Web 模块使用 MVC Controller 和 ApplicationPart。启动期使用没有问题，但运行时热插拔需要谨慎。

推荐分两层支持：

### 第一层：动态 Endpoint

运行时插件优先通过 `IPluginEndpointContributor` 声明 Endpoint：

```csharp
public interface IPluginEndpointContributor
{
    Task ContributeAsync(PluginEndpointContributionContext context);
}
```

框架在主应用启动时注册一个 `PluginEndpointDataSource`。运行时插件启用时向该 DataSource 添加 Endpoint，停用时移除 Endpoint，并触发路由变更通知。

优点：

- 不依赖 MVC Controller 缓存刷新。
- Endpoint 属于插件实例，移除时更容易释放引用。
- 适合管理接口、轻量 API、Webhook 和插件命令。

限制：

- 需要插件按 Endpoint 方式声明 API。
- 与现有 Controller 编程模型不同。

### 第二层：MVC Controller 热加载

如果要支持 Controller 运行时热加载，需要处理：

- 动态维护 `ApplicationPartManager`。
- 实现 `IActionDescriptorChangeProvider` 触发 MVC 重新发现 Action。
- 移除 ApplicationPart 后清理 MVC 缓存。
- 确保 Endpoint、ActionDescriptor、Filter、ModelBinder 不持有插件程序集引用。
- 卸载前排空正在执行的请求。

此能力应放到第二阶段后半或第三阶段，不应作为第一版热插拔的核心路径。

## 后台任务与调度任务

插件不应把 `IHostedService` 直接注册到主容器，因为主 Host 不会在运行时自动启动新注册的 HostedService，也无法按插件粒度停止。

建议新增插件任务抽象：

```csharp
public interface IPluginBackgroundTask
{
    Task StartAsync(PluginTaskContext context, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
```

`IPluginRuntime` 在插件启动时创建任务实例并启动，在插件停用时按反向顺序停止。

调度任务建议通过 Oscillator 适配器注册：

- 插件启动时注册插件任务。
- 插件停止时停止并移除插件任务。
- 每个任务绑定插件 ID 和版本。
- 任务执行中应持有插件请求计数，卸载前等待任务结束或取消。

## 配置设计

运行时插件配置不应依赖主配置重新构建。

建议：

- 插件包内允许包含 `appsettings.plugin.json`。
- 插件配置合并顺序为默认值、插件包配置、主应用插件覆盖配置。
- 插件级服务容器注册自己的 Options。
- 主应用只保存插件管理配置，例如启用状态、版本指针、是否允许自动加载。

主应用配置示例：

```json
{
  "MergeBlock": {
    "HotPlug": {
      "Enable": true,
      "PluginRootPath": "Plugins",
      "AutoLoadOnStartup": true,
      "WatchPluginFolder": false,
      "VerifyUnload": true
    }
  }
}
```

## 插件发现与文件监听

插件发现来源：

- 应用启动时扫描 `Plugins` 目录。
- 管理 API 上传或登记插件包。
- 配置文件显式声明插件。
- 文件监听发现新插件包。

文件监听默认不建议开启自动启用。更安全的默认策略是：

1. 文件监听发现新包。
2. 插件进入 `Discovered` 状态。
3. 校验 manifest、依赖、版本和哈希。
4. 管理员或管理服务显式启用。

如果开启自动启用，必须先加载到隔离上下文并完成健康检查，失败时进入隔离状态，不影响运行中插件。

## 停用与卸载流程

停用流程：

```text
接收停用请求
  -> 检查是否有其他运行中插件依赖它
  -> 禁止新请求进入插件
  -> 等待正在执行的请求和任务排空
  -> 触发 OnPluginStoppingAsync
  -> 移除动态 Endpoint
  -> 停止后台任务和调度任务
  -> 取消事件订阅和外部连接
  -> 触发 OnPluginStoppedAsync
  -> 状态改为 Stopped
```

卸载流程：

```text
插件必须处于 Stopped
  -> 触发 OnPluginUnloadingAsync
  -> Dispose 插件级 ServiceProvider
  -> 清理 PluginInstance 中的 Type、Assembly、Delegate、MethodInfo 引用
  -> 清理运行时缓存和模块描述符快照
  -> 调用 PluginLoadContext.Unload()
  -> 多轮 GC
  -> 用 WeakReference 验证 ALC 是否释放
  -> 成功则进入 Unloaded，失败则进入 UnloadFailed
```

卸载失败常见原因：

- 静态事件未取消订阅。
- Timer、Task、Thread、Channel 仍在运行。
- 单例服务被主容器持有。
- MVC ActionDescriptor 或 Endpoint 持有插件类型。
- 日志 Scope、缓存、表达式树、反射缓存持有插件类型。
- 插件把自己的类型写入全局静态集合。

框架应在卸载失败时输出诊断信息，但无法保证定位所有引用来源。

## 依赖管理

插件依赖需要覆盖插件级依赖和程序集级依赖。

插件级依赖：

- 使用 manifest 声明。
- 加载前做拓扑排序。
- 停用时先停用依赖它的插件，再停用被依赖插件。
- 不允许停用仍被运行中插件依赖的插件，除非使用级联停用。

程序集级依赖：

- `Materal.MergeBlock.Abstractions`、公共模型和公共接口应从默认上下文加载。
- 插件私有依赖从插件目录加载。
- 插件之间共享程序集应通过依赖插件暴露，不能靠随机探测目录。
- 不同插件允许使用不同版本的私有依赖，但公共抽象版本必须兼容主应用。

## 安全设计

热插拔本质上是运行时执行新代码，安全边界必须明确。

建议默认策略：

- 只允许从可信插件目录加载。
- 插件包必须有 manifest。
- 生产环境启用插件哈希校验或签名校验。
- 管理 API 必须经过认证和高权限授权。
- 插件操作必须写审计日志。
- 插件默认不能修改主机级认证、授权、Kestrel、日志 Provider 和全局异常处理。
- 插件默认不能读取其他插件的私有配置。
- 插件上传目录和运行目录分离，防止半写入文件被加载。
- 插件失败后进入隔离状态，不自动反复重试。

## 管理 API

建议 `Materal.MergeBlock.HotPlug.Web` 提供管理接口。

能力：

- 查询插件列表和状态。
- 查询插件 manifest、版本、依赖和健康状态。
- 加载插件。
- 启用插件。
- 停用插件。
- 卸载插件。
- 重新加载插件。
- 查询最近一次失败原因。
- 查询卸载验证结果。

接口示例：

```text
GET    /MergeBlockHotPlugAPI/Plugins
GET    /MergeBlockHotPlugAPI/Plugins/{name}
POST   /MergeBlockHotPlugAPI/Plugins/{name}/load
POST   /MergeBlockHotPlugAPI/Plugins/{name}/start
POST   /MergeBlockHotPlugAPI/Plugins/{name}/stop
POST   /MergeBlockHotPlugAPI/Plugins/{name}/unload
POST   /MergeBlockHotPlugAPI/Plugins/{name}/reload
```

所有变更型接口都应记录操作人、操作时间、来源 IP、目标插件、目标版本和执行结果。

## 日志与观测

热插拔至少需要记录以下事件：

- 插件发现。
- manifest 读取和校验。
- 依赖检查。
- 程序集加载。
- 服务容器创建。
- 生命周期开始和结束。
- 动态 Endpoint 注册和移除。
- 后台任务启动和停止。
- 停用排空耗时。
- ALC 卸载验证结果。
- 失败原因和回滚结果。

建议指标：

- 当前运行插件数。
- 插件加载耗时。
- 插件启动耗时。
- 插件停止耗时。
- 插件卸载成功率。
- 插件请求数和活跃请求数。
- 插件任务运行数。
- 插件异常数。

## 错误处理和回滚

加载失败：

- 插件进入 `LoadingFailed`。
- 不注册任何 Endpoint。
- 不启动后台任务。
- 保留失败原因供查询。

启动失败：

- 停止已经启动的插件任务。
- 移除已经注册的 Endpoint。
- Dispose 插件级服务容器。
- 插件进入 `StartingFailed`。

停用失败：

- 插件进入 `StoppingFailed`。
- 默认不继续卸载。
- 允许管理员强制停用，但强制停用可能导致请求失败或资源泄漏。

卸载失败：

- 插件进入 `UnloadFailed`。
- 插件文件可能仍被占用。
- 允许继续运行旧版本或提示需要重启进程。

## 兼容现有模块

现有 `IMergeBlockModule` 模块默认仍按启动期方式工作。为了支持热插拔，应逐步引导新插件使用以下模式：

- 服务注册放入插件级容器。
- Web API 使用动态 Endpoint 或明确支持热插拔的 Controller 模型。
- 后台任务实现插件任务抽象，不直接注册 `IHostedService`。
- 调度任务通过插件适配器注册和移除。
- 所有静态事件、Timer、外部连接都在停止阶段释放。
- 不把插件 Type、Assembly、Delegate 长期写入主应用静态集合。

对于无法满足这些要求的插件，可以标记为：

```json
{
  "AllowRuntimeLoad": false,
  "AllowRuntimeUnload": false
}
```

这类插件只能作为启动期插件使用。

## 一期范围建议

第一轮实现建议控制范围，避免一次性承诺完整热卸载。

建议一期包含：

- 插件 manifest。
- 插件状态模型。
- `IPluginRuntime` 抽象。
- 插件发现和依赖排序重构。
- 插件级 `AssemblyLoadContext`。
- 插件级 `ServiceProvider`。
- 插件生命周期接口。
- 动态 Endpoint 第一版。
- 插件后台任务第一版。
- 插件状态查询 API。
- 手动加载、启动、停止 API。
- 卸载验证工具，但不把卸载成功作为所有插件的硬性要求。

一期不包含：

- MVC Controller 热卸载。
- 主容器运行时服务注册。
- 自动数据库迁移。
- 分布式插件同步。
- 插件市场和远程包仓库。
- 不可信插件沙箱。

## 测试计划

单元测试：

- manifest 解析和默认值。
- 插件依赖拓扑排序。
- 循环依赖检测。
- 缺失依赖检测。
- 版本范围匹配。
- 插件状态流转合法性。
- 加载失败状态记录。
- 启动失败回滚。
- 停用时按反向顺序停止任务。
- 卸载验证成功和失败路径。

集成测试：

- 启动期插件仍按现有方式加载。
- 运行时加载一个只包含服务的插件。
- 运行时加载一个动态 Endpoint 插件，并能访问接口。
- 停用插件后新请求无法进入插件 Endpoint。
- 正在执行的插件请求能被计数并在停用时排空。
- 插件后台任务能启动和停止。
- 插件 A 依赖插件 B 时，B 不能在 A 运行中被直接停用。
- 插件启动失败不会污染路由和任务列表。
- 可卸载插件停止后 ALC 能被 GC 回收。

手工验证：

- 替换插件版本后可以重新加载。
- 卸载失败时能看到明确诊断日志。
- 管理 API 权限控制有效。
- 插件目录半写入文件不会被加载。

## 风险与缓解

主 DI 容器不可变：

- 使用插件级容器和主容器代理，避免运行时修改主容器。

MVC Controller 热卸载复杂：

- 一期优先支持动态 Endpoint，Controller 热加载作为增强能力。

插件卸载不彻底：

- 引入生命周期释放约定和 ALC 验证，把不可卸载插件标记为需要重启。

插件误伤主应用：

- 限制插件可修改的主机能力，插件失败后隔离。

多实例状态不一致：

- 第一版只承诺单实例，后续再引入分布式状态和发布协调。

依赖版本冲突：

- 公共抽象由主上下文加载，插件私有依赖由插件上下文加载，manifest 做版本约束。

安全风险：

- 默认只加载可信目录，管理接口强授权，插件包做哈希或签名校验。

## 验收标准

- 现有启动期插件加载行为不被破坏。
- 插件 manifest 能描述名称、版本、入口模块、依赖和热插拔能力。
- 应用运行中可以手动加载并启动一个运行时插件。
- 运行时插件可以拥有独立服务容器。
- 运行时插件可以注册并移除动态 Endpoint。
- 运行时插件可以启动并停止插件级后台任务。
- 插件依赖顺序正确，缺失依赖和循环依赖会给出明确错误。
- 插件启动失败会回滚已注册资源。
- 插件停用后不再接受新请求。
- 可卸载插件停止后能够通过 ALC 回收验证。
- 不可卸载插件会进入明确状态，并提示需要重启或修复引用泄漏。
- 所有插件启停和失败事件都有日志或审计记录。

