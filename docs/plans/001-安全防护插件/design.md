# Materal.MergeBlock.Shield 安全防护插件设计

## 背景

使用 MMB 框架编写的系统遭遇接口探测类攻击。攻击者当前没有获取到业务数据，但会通过大量请求不存在的接口来判断系统接口边界。由于不存在的接口会返回 404，而存在但需要认证的接口通常返回 401，这会让攻击者通过状态码差异推断接口是否存在。

本设计新增 `Materal.MergeBlock.Shield` 类库，作为 MMB 的独立安全防护模块，通过 MMB 模块机制注册中间件，对可疑接口探测行为进行识别和临时拦截。

## 目标

- 提高接口枚举和路径猜测的攻击成本。
- 对短时间内大量触发 API 404 的来源进行临时拦截。
- 保持认证、授权、访问日志等现有模块职责清晰。
- 第一版优先支持单实例内存计数，后续可扩展 Redis 等分布式存储。
- 避免影响静态资源、Swagger、健康检查、前端路由和正常预检请求。

## 非目标

- 不替代身份认证和权限控制。
- 不实现完整 WAF 能力。
- 不对所有 404 做无差别拦截。
- 第一版不解决多实例之间的封禁状态同步。
- 第一版不实现复杂 Bot 识别、验证码、人机校验等能力。

## 模块边界

新增类库：

- `Materal.MergeBlock.Shield`

建议职责划分：

- `Materal.MergeBlock.Authorization`：继续负责认证与授权。
- `Materal.MergeBlock.AccessLog`：继续负责访问日志记录。
- `Materal.MergeBlock.Shield`：负责异常访问行为识别、临时拦截和安全策略执行。

## 核心策略

第一版策略为“基于 404 的接口探测拦截”：

1. 请求进入中间件时，先判断当前来源是否已被临时封禁。
2. 如果已封禁，直接返回配置的拦截状态码。
3. 如果未封禁，继续执行后续 ASP.NET Core 管道。
4. 后续响应返回后，如果状态码为 404，且请求路径命中监控范围，则累计该来源的 404 次数。
5. 如果同一来源在滑动时间窗口内达到阈值，则写入临时封禁状态。
6. 拦截和封禁事件应输出日志，便于审计和后续分析。

默认建议：

- 监控路径前缀：`/api`
- 时间窗口：60 秒
- 404 阈值：10 次
- 封禁时长：10 分钟
- 拦截状态码：429

拦截状态码可配置。若系统希望降低攻击者对防护机制的感知，可以配置为 401；若希望语义准确并便于监控识别，可以使用 429。

## 配置设计

新增 `ShieldOptions`，配置节建议为 `MergeBlock:Shield`。

建议字段：

| 字段 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `Enable` | `bool` | `true` | 是否启用 Shield 中间件 |
| `MonitorPathPrefixes` | `string[]` | `["/api"]` | 需要监控 404 的路径前缀 |
| `ExcludePathPrefixes` | `string[]` | `[]` | 排除路径前缀，例如 Swagger、健康检查 |
| `WhiteListIPs` | `string[]` | `[]` | IP 白名单 |
| `WindowSeconds` | `int` | `60` | 404 统计时间窗口 |
| `NotFoundLimit` | `int` | `10` | 时间窗口内允许的 404 次数 |
| `BlockedSeconds` | `int` | `600` | 达到阈值后的临时封禁时间 |
| `BlockedStatusCode` | `int` | `429` | 被拦截时返回的 HTTP 状态码 |
| `TrackAuthenticatedRequests` | `bool` | `false` | 是否统计已认证请求 |

示例：

```json
{
  "MergeBlock": {
    "Shield": {
      "Enable": true,
      "MonitorPathPrefixes": ["/api"],
      "ExcludePathPrefixes": ["/swagger", "/health"],
      "WhiteListIPs": ["127.0.0.1"],
      "WindowSeconds": 60,
      "NotFoundLimit": 10,
      "BlockedSeconds": 600,
      "BlockedStatusCode": 429,
      "TrackAuthenticatedRequests": false
    }
  }
}
```

## 关键组件

### ShieldModule

`ShieldModule` 继承 `MergeBlockModule`，负责注册配置、存储服务和中间件。

建议依赖：

- `WebModule`

职责：

- 绑定 `ShieldOptions`。
- 注册内存缓存或 Shield 状态存储。
- 注册 `ShieldMiddleware`。
- 在 `OnPreApplicationInitialization` 阶段通过 `UseMiddleware<ShieldMiddleware>()` 注入管道。
- 不在 `OnApplicationInitialization` 阶段注册中间件，避免与 `WebModule.MapControllers()` 的执行顺序竞争。

### ShieldMiddleware

中间件负责核心请求处理。

请求前：

- 判断功能是否启用。
- 判断路径是否应被监控。
- 判断 IP 是否在白名单。
- 判断请求来源是否处于临时封禁期。
- 已封禁时直接返回 `BlockedStatusCode`。

请求后：

- 如果响应状态码为 404，且符合统计条件，则累计来源的 404 次数。
- 达到阈值后，将来源写入封禁状态。
- 输出安全日志。

### IShieldStore

抽象 Shield 状态存储，避免中间件直接依赖具体缓存实现。

建议能力：

- 查询来源当前是否已处于封禁状态。
- 原子记录一次 404 命中，并在达到阈值时写入封禁状态。
- 返回当前窗口内命中次数、是否新触发封禁、封禁过期时间等结果，供中间件写日志和决定后续行为。

接口粒度应避免让中间件拆分执行“查询计数、增加计数、判断阈值、写入封禁”多步逻辑。即使第一版使用内存存储，也需要考虑并发请求；后续扩展 Redis 存储时，该语义应能映射为 Lua 脚本、事务或其他原子操作。

第一版实现：

- `MemoryShieldStore`

后续扩展：

- `RedisShieldStore`
- 数据库持久化审计记录
- 接入网关或 WAF 黑名单

## 来源识别

第一版以 IP 作为来源标识，只读取 `HttpContext.Connection.RemoteIpAddress`，不在 Shield 内部直接解析 `X-Forwarded-For`、`X-Real-IP` 等代理头。

部署在反向代理之后时，应由宿主应用先正确配置 ASP.NET Core `ForwardedHeadersMiddleware` 及可信代理/可信网络，使 `RemoteIpAddress` 已经是可信的真实客户端地址。Shield 只消费最终的 `RemoteIpAddress`。

注意：不能无条件信任 `X-Forwarded-For`，否则攻击者可以伪造 IP 绕过封禁。后续如果要在 Shield 内置代理头解析能力，必须同时提供 `TrustedProxies` 或 `TrustedNetworks` 等可信代理配置，否则不实现该能力。

## 路径与请求过滤

以下请求默认不参与统计：

- `OPTIONS` 请求。
- 未命中 `MonitorPathPrefixes` 的路径。
- 命中 `ExcludePathPrefixes` 的路径。
- IP 白名单请求。
- 已认证请求，除非 `TrackAuthenticatedRequests` 为 `true`。

推荐默认排除：

- `/swagger`
- `/health`
- `/favicon.ico`
- 静态资源路径

## 中间件顺序

`ShieldMiddleware` 需要能在请求进入时提前拦截，也需要在 `await next()` 后观察最终状态码。因此它应注册在可以包住后续 Web 管道的位置。

已确认 MMB 模块会按依赖关系排序，并分别遍历执行所有模块的 `OnPreApplicationInitialization`、`OnApplicationInitialization`、`OnPostApplicationInitialization`。`WebModule` 在 `OnPreApplicationInitialization` 中启用请求缓冲，在 `OnApplicationInitialization` 中执行 `MapControllers()`。

因此 `ShieldModule` 应声明依赖 `WebModule`，并在 `OnPreApplicationInitialization` 注册 `ShieldMiddleware`。这样 Shield 会位于 `WebModule` 请求缓冲之后、`MapControllers()` 之前，既能在请求进入时提前拦截，也能在 `await next()` 后观察控制器路由产生的 404。

如果未来 MMB 提供显式中间件优先级机制，可再将 Shield 迁移到优先级机制；第一版不修改 MMB 模块排序规则。

## 日志与审计

Shield 应记录两类事件：

- 404 探测命中：来源、路径、User-Agent、当前窗口命中次数。
- 临时封禁：来源、触发阈值、封禁时长、首次命中时间、最后命中时间。

日志级别建议：

- 普通命中：`Information` 或 `Debug`
- 达到封禁：`Warning`

第一版只要求写入应用日志。后续可与 `Materal.MergeBlock.AccessLog` 或独立安全审计存储集成。

## 错误处理

- Shield 自身异常不应导致业务请求失败。
- 存储异常时应记录日志并放行请求。
- 配置非法时使用默认值或在启动阶段给出明确异常，具体策略在实现阶段确定。
- 如果响应已经开始写入，不再修改状态码。

## 测试计划

建议新增针对 `Materal.MergeBlock.Shield` 的单元测试或集成测试：

- 未达到 404 阈值时不拦截。
- 达到阈值后进入临时封禁。
- 封禁过期后恢复访问。
- 白名单 IP 不统计、不封禁。
- 排除路径不统计。
- 非监控路径不统计。
- `OPTIONS` 请求不统计。
- 已认证请求默认不统计。
- 配置 `BlockedStatusCode` 后返回对应状态码。

## 兼容性与风险

潜在风险：

- 单纯按 IP 封禁可能误伤 NAT、企业出口、校园网或移动网络用户。
- 阈值过低会影响正常用户，阈值过高会降低防护效果。
- 多实例部署时，内存存储只对单实例生效。
- 如果反向代理真实 IP 配置不正确，可能导致所有请求被识别为同一来源。

缓解措施：

- 默认阈值保持保守。
- 支持白名单和排除路径。
- 日志先行，便于观察真实访问模式后调整阈值。
- 后续支持 Redis 分布式存储。
- 明确代理部署下的真实 IP 配置要求。

## 后续扩展

- Redis 分布式封禁状态。
- IP 黑名单和灰名单。
- 按 User-Agent、Token、租户、账号等维度联合判断。
- 敏感路径扫描识别，例如 `/admin`、`/.env`、`/wp-login.php`。
- 对高频请求进行限速而非直接封禁。
- 管理接口查看当前封禁列表和解除封禁。
- 与网关、WAF 或防火墙联动。

## 验收标准

- 引用 `Materal.MergeBlock.Shield` 并启用模块后，API 404 探测会被统计。
- 同一来源在配置窗口内达到阈值后会被临时拦截。
- 白名单、排除路径、非监控路径不会触发封禁。
- Shield 不影响正常认证和授权流程。
- Shield 不影响 Swagger、健康检查、静态资源和 `OPTIONS` 预检请求。
- 日志能反映 404 命中和封禁事件。
