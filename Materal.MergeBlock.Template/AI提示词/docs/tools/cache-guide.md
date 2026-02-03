# 缓存工具使用指南

## 概述

`Materal.Utils.Caching` 提供内存缓存功能，`Materal.Utils.Redis` 提供 Redis 分布式缓存功能。

## 安装

### 内存缓存

```bash
dotnet add package Materal.Utils.Caching
```

### Redis 缓存

```bash
dotnet add package Materal.Utils.Redis
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

### 依赖包

- `Materal.Utils`（基础工具包）
- `StackExchange.Redis`（Redis 工具包，仅 Redis 模块需要）

## 内存缓存

### ICacheHelper 接口

内存缓存管理器，提供两种过期策略：滑动过期和绝对过期。

```csharp
using Materal.Utils.Caching;
```

#### 注册服务

```csharp
// 在 Program.cs 中注册
builder.Services.AddMateralCachingUtils();
```

#### 设置缓存（滑动过期）

只要有访问就会延后过期时间：

```csharp
ICacheHelper cacheHelper = serviceProvider.GetRequiredService<ICacheHelper>();

// 按小时设置
cacheHelper.SetBySliding("key", "value", 1);

// 按自定义时间单位设置
cacheHelper.SetBySliding("key", "value", 30, DateTimeUnit.Minute);

// 按绝对时间设置
cacheHelper.SetBySliding("key", "value", DateTime.Now.AddMinutes(30));

// 按 TimeSpan 设置
cacheHelper.SetBySliding("key", "value", TimeSpan.FromMinutes(30));
```

#### 设置缓存（绝对时间）

在指定时间后过期，无论是否访问：

```csharp
ICacheHelper cacheHelper = serviceProvider.GetRequiredService<ICacheHelper>();

// 按小时设置
cacheHelper.SetByAbsolute("key", "value", 1);

// 按自定义时间单位设置
cacheHelper.SetByAbsolute("key", "value", 30, DateTimeUnit.Minute);

// 按绝对时间设置
cacheHelper.SetByAbsolute("key", "value", DateTime.Now.AddMinutes(30));

// 按 TimeSpan 设置
cacheHelper.SetByAbsolute("key", "value", TimeSpan.FromMinutes(30));
```

#### 获取缓存

```csharp
ICacheHelper cacheHelper = serviceProvider.GetRequiredService<ICacheHelper>();

// 获取缓存，不存在则抛出异常
string value1 = cacheHelper.Get<string>("key");
int value2 = cacheHelper.Get<int>("number");

// 获取缓存，不存在返回 null
string? value3 = cacheHelper.GetOrDefault<string>("key");
int? value4 = cacheHelper.GetOrDefault<int>("number");
```

#### 移除和清空缓存

```csharp
ICacheHelper cacheHelper = serviceProvider.GetRequiredService<ICacheHelper>();

// 移除单个缓存
cacheHelper.Remove("key");

// 清空所有缓存
cacheHelper.Clear();

// 检查键是否存在
bool exists = cacheHelper.KeyAny("key");

// 获取所有缓存键
List<string> allKeys = cacheHelper.GetCacheKeys();
```

## Redis 缓存

### RedisConfigModel - 配置模型

```csharp
using Materal.Utils.Redis;

var config = new RedisConfigModel
{
    Host = "127.0.0.1",
    Port = "6379",
    Password = "your_password"
};

// 自动生成的连接字符串
string connectionString = config.ConnectionString; // "127.0.0.1:6379"
```

### RedisHelper - Redis 操作

获取 Redis 数据库连接：

```csharp
using Materal.Utils.Redis;
using StackExchange.Redis;

var config = new RedisConfigModel
{
    Host = "127.0.0.1",
    Port = "6379"
};

// 获取数据库
IDatabase db = RedisHelper.GetDb(config);

// 获取订阅者
ISubscriber subscriber = RedisHelper.GetSubscriber(config);
```

#### 基本操作

```csharp
IDatabase db = RedisHelper.GetDb("127.0.0.1:6379");

// 字符串操作
await db.StringSetAsync("key", "value");
string? value = await db.StringGetAsync("key");

// 设置过期时间
await db.StringSetAsync("key", "value", TimeSpan.FromMinutes(5));

// 哈希操作
await db.HashSetAsync("hashKey", "field", "value");
string? hashValue = await db.HashGetAsync("hashKey", "field");

// 列表操作
await db.ListLeftPushAsync("listKey", "item");
string? item = await db.ListRightPopAsync("listKey");

// 集合操作
await db.SetAddAsync("setKey", "member");
bool exists = await db.SetContainsAsync("setKey", "member");

// 有序集合操作
await db.SortedSetAddAsync("sortedKey", "member", 1.0);
```

#### 分布式锁（使用 RedisManager）

推荐使用 `RedisManager` 获取分布式锁：

```csharp
using Materal.Utils.Redis;

IDatabase db = RedisHelper.GetDb("127.0.0.1:6379");
string lockName = "distributed_lock";
TimeSpan expiration = TimeSpan.FromSeconds(30);

// 非阻塞锁（推荐）
RedisLock? redisLock = await db.GetNonBlockingLockAsync(lockName, expiration);
if (redisLock is not null)
{
    try
    {
        // 持有锁期间执行的代码
        await DoSomethingAsync();
    }
    finally
    {
        await redisLock.DisposeAsync();
    }
}

// 阻塞锁（会一直等待直到获得锁）
using (await db.GetBlockingLockAsync(lockName, expiration))
{
    // 持有锁期间执行的代码
    await DoSomethingAsync();
}
```

### HashValue<T> - 哈希值模型

用于存储哈希表中的键值对：

```csharp
using Materal.Utils.Redis;

var hashEntry = new HashValue<UserInfo>
{
    Key = "user:1001",
    Value = new UserInfo { Name = "张三", Age = 25 }
};
```

### RedisLock - 分布式锁封装

提供更便捷的锁使用方式：

```csharp
using Materal.Utils.Redis;
using StackExchange.Redis;

IDatabase db = RedisHelper.GetDb("127.0.0.1:6379");
string lockName = "distributed_lock";

using (await db.LockDatabaseAsync(lockName, TimeSpan.FromSeconds(30)))
{
    // 持有锁期间执行的代码
    await DoSomethingAsync();
}
```

## DateTimeUnit 枚举

用于指定时间单位：

```csharp
public enum DateTimeUnit
{
    YearUnit = 0,        // 年
    MonthUnit = 1,       // 月
    DayUnit = 2,         // 日
    HourUnit = 3,        // 时
    MinuteUnit = 4,      // 分
    SecondUnit = 5,      // 秒
    MillisecondUnit = 6  // 毫秒
}
```

## 完整示例

### 内存缓存服务

```csharp
using Materal.Utils.Caching;

namespace Example
{
    public class CacheService(ICacheHelper cacheHelper)
    {
        /// <summary>
        /// 获取用户信息（带缓存）
        /// </summary>
        /// <param name="userID">用户唯一标识</param>
        /// <returns>缓存的用户信息，未找到时返回 null</returns>
        public async Task<UserInfo?> GetUserAsync(Guid userID)
        {
            string cacheKey = $"user:{userID}";
            UserInfo? cachedUser = cacheHelper.GetOrDefault<UserInfo>(cacheKey);
            if (cachedUser is not null) return cachedUser;

            UserInfo? user = await FetchUserFromDatabaseAsync(userID);
            if (user is not null)
            {
                cacheHelper.SetByAbsolute(cacheKey, user, CacheExpirationMinutes);
            }
            return user;
        }

        private const int CacheExpirationMinutes = 5;

        /// <summary>
        /// 清除用户缓存
        /// </summary>
        /// <param name="userID">用户唯一标识</param>
        public void InvalidateUserCache(Guid userID)
        {
            string cacheKey = $"user:{userID}";
            cacheHelper.Remove(cacheKey);
        }

        private async Task<UserInfo?> FetchUserFromDatabaseAsync(Guid userID)
        {
            // 从数据库获取用户信息
            return null;
        }
    }

    public class UserInfo
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
```

### Redis 缓存服务

```csharp
using Materal.Utils.Redis;
using StackExchange.Redis;

namespace Example
{
    public class RedisCacheService(IDatabase db)
    {
        private readonly IDatabase _db = db;
        private const string UserCachePrefix = "user:";

        /// <summary>
        /// 缓存用户信息
        /// </summary>
        /// <param name="user">用户信息</param>
        public async Task CacheUserAsync(UserInfo user)
        {
            if (user == null) throw new {ProjectName}Exception("用户信息不能为空");
            string key = $"{UserCachePrefix}{user.ID}";
            await _db.StringSetAsync(key, user.ToJson(), TimeSpan.FromMinutes(30));
        }

        /// <summary>
        /// 获取缓存的用户信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>缓存的用户信息</returns>
        public async Task<UserInfo?> GetCachedUserAsync(Guid userID)
        {
            string key = $"{UserCachePrefix}{userID}";
            string? json = await _db.StringGetAsync(key);
            return json?.JsonToObject<UserInfo>();
        }

        /// <summary>
        /// 使用分布式锁更新用户信息
        /// </summary>
        /// <param name="user">用户信息</param>
        public async Task UpdateUserWithLockAsync(UserInfo user)
        {
            if (user == null) throw new {ProjectName}Exception("用户信息不能为空");
            string lockName = $"lock:update_user:{user.ID}";
            using (await _db.GetBlockingLockAsync(lockName, TimeSpan.FromSeconds(10)))
            {
                await UpdateUserAsync(user);
            }
        }

        private Task UpdateUserAsync(UserInfo user)
        {
            // 更新数据库
            return Task.CompletedTask;
        }
    }

    public class UserInfo
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
```

## 使用场景对比

| 场景 | 推荐方案 |
|------|----------|
| 单机应用临时缓存 | `ICacheHelper`（内存缓存） |
| 多实例应用共享缓存 | `RedisHelper`（Redis） |
| 需要分布式锁 | `RedisManager` + `GetNonBlockingLockAsync` |
| 缓存热点数据 | 两者均可，根据访问量选择 |
| 缓存会话数据 | `RedisHelper`（支持跨进程） |

## 注意事项

1. **内存缓存**适合存储少量、临时的数据
2. **Redis 缓存**适合分布式环境和持久化缓存场景
3. 使用 `GetOrDefault` 可以避免键不存在时抛出异常
4. 分布式锁应设置合理的过期时间，避免死锁
5. 大对象建议序列化为 JSON 后存储
