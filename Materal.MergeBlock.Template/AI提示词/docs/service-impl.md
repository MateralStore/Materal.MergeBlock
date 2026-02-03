# 服务实现规范

本文档描述了在 Materal.MergeBlock 项目中实现服务（ServiceImpl）时需要遵循的规范。

> **重要提示**：本项目使用的映射器是 **Materal.Utils.AutoMapper**，不是 AutoMapper 库。两者 API 不同，AutoMapper 的使用经验在本项目中**不适用**。
>
> 相关文档：
> - [服务设计规范](service-design.md) - 服务接口设计
> - [服务模型设计规范](service-model-design.md) - 服务模型设计
> - [编码规范](coding-style.md) - 通用编码规范

## 2 Materal.Utils.AutoMapper 说明

### 2.1 重要说明

> **警告**：本项目使用的映射器是 `Materal.Utils.AutoMapper`，**不是** AutoMapper 库。
>
> 两者 API 完全不同，AutoMapper 的使用经验（如 `CreateMap<T1, T2>()`、`ReverseMap`、`ForMember` 等）在本项目中**不适用**。

### 2.2 IMapper 接口

`Materal.Utils.AutoMapper` 的 `IMapper` 接口只有两个方法：

```csharp
public interface IMapper
{
    /// <summary>
    /// 映射单个对象
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="source">源对象</param>
    /// <returns>映射后的目标对象</returns>
    T Map<T>(object source);

    /// <summary>
    /// 映射到已有对象
    /// </summary>
    /// <param name="source">源对象</param>
    /// <param name="target">目标对象</param>
    void Map(object source, object target);
}
```

### 2.3 核心特点

| 特性 | 说明 |
|------|------|
| 单个对象映射 | `Mapper.Map<T>(object source)` - 注意参数是 `object` |
| 列表映射 | 自动支持 `List<T>`、`ICollection<T>` 等集合类型 |
| 已有对象映射 | `Mapper.Map(source, target)` - 将源对象映射到目标对象 |
| 反向映射 | 通过 `CreateMap` 的 `reverseMap` 参数支持 |
| 自动配置 | 代码生成器会自动配置大部分映射规则 |

### 2.4 使用方式

**单个对象映射**：

```csharp
// 实体转 DTO
UserDTO dto = Mapper.Map<UserDTO>(user);

// 源参数类型是 object，不需要显式转换
// 会自动创建目标类型实例并执行映射
```

**映射到已有对象**：

```csharp
// DTO 转实体（合并到已有对象）
Mapper.Map(model, domainFromDB);

// 常用于 EditAsync 中更新已有实体
domainFromDB.Nickname = model.Nickname;
Mapper.Map(model, domainFromDB);
```

**列表映射**：

```csharp
// 自动支持 List、ICollection 等集合类型
List<UserDTO> dtoList = Mapper.Map<List<UserDTO>>(userList);
ICollection<UserDTO> dtoCollection = Mapper.Map<ICollection<UserDTO>>(userCollection);
```

**元组返回**：

```csharp
(List<UserDTO> data, RangeModel rangeInfo) = await DefaultRepository.RangeAsync(expression, model);
List<UserDTO> result = Mapper.Map<List<UserDTO>>(data);
```

### 2.5 自定义 Profile

通过继承 `Profile` 并重写构造方法来定义映射规则：

```csharp
namespace {ProjectName}.{ModuleName}.Application.AutoMapperProfile;

/// <summary>
/// 用户映射配置
/// </summary>
public class UserProfile : Profile
{
    public UserProfile()
    {
        // 正向映射
        CreateMap<User, UserDTO>(
            map: (mapper, source, target) =>
            {
                // 自定义映射逻辑
                target.FullName = $"{source.LastName}{source.FirstName}";
            },
            reverseMap: (mapper, dto, user) =>
            {
                // 反向映射（D2O -> O2D）
                user.LastName = dto.FullName.Substring(0, 1);
                user.FirstName = dto.FullName.Substring(1);
            },
            useDefaultMapper: false
        );
    }
}
```

**CreateMap 参数说明**：

| 参数 | 类型 | 说明 |
|------|------|------|
| `map` | `Action<IMapper, T1, T2>` | 正向映射函数（T1 -> T2） |
| `reverseMap` | `Action<IMapper, T2, T1>?` | 反向映射函数（T2 -> T1），可选 |
| `useDefaultMapper` | `bool` | 是否使用默认映射规则，默认为 `true` |

### 2.6 禁止操作

- **不要**使用 AutoMapper 的 `CreateMap<T1, T2>()` 静态方法
- **不要**使用 AutoMapper 的 `Profile` 基类（命名空间不同）
- **不要**使用 `ReverseMap()` 扩展方法
- **不要**使用 `ForMember()` 配置条件映射
- **不要**使用 `ConvertUsing()` 自定义转换

### 2.7 正确操作

- 使用 `Mapper.Map<T>(object source)` 进行对象映射
- 使用 `Mapper.Map(source, target)` 进行对象合并
- 列表/集合映射直接使用 `Mapper.Map<List<T>>()`
- 仅当需要自定义映射逻辑时才编写 Profile

## 3 基类继承体系

### 3.1 继承关系图

```
BaseServiceImpl (非泛型基类)
    ↓
BaseServiceImpl<TUnitOfWork> (工作单元)
    ↓
BaseServiceImpl<TRepository, TDomain, TUnitOfWork> (默认仓储)
    ↓
BaseServiceImpl<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO, TRepository, TDomain, TUnitOfWork>
```

### 3.2 带视图的继承关系

```
BaseServiceImpl<主实体泛型参数>
    ↓
BaseServiceImpl<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO, TRepository, TViewRepository, TDomain, TViewDomain, TUnitOfWork>
```

### 3.3 基类源码结构

**BaseServiceImpl** - 非泛型基类，提供基础属性：

```csharp
public abstract class BaseServiceImpl : IBaseService
{
    public Guid LoginUserID { get; set; }
    protected IMapper Mapper { get; }
}
```

**BaseServiceImpl<TUnitOfWork>** - 工作单元支持：

```csharp
public abstract class BaseServiceImpl<TUnitOfWork> : BaseServiceImpl
    where TUnitOfWork : IMergeBlockUnitOfWork
{
    protected TUnitOfWork UnitOfWork { get; }
}
```

**BaseServiceImpl<TRepository, TDomain, TUnitOfWork>** - 默认仓储支持：

```csharp
public abstract class BaseServiceImpl<TRepository, TDomain, TUnitOfWork> : BaseServiceImpl<TUnitOfWork>
{
    protected TRepository DefaultRepository { get; }
}
```

**BaseServiceImpl<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO, TRepository, TDomain, TUnitOfWork>** - 完整 CRUD 支持：

```csharp
public abstract class BaseServiceImpl<...> : BaseServiceImpl<TRepository, TDomain, TUnitOfWork>
    , IBaseService<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO>
{
    // CRUD 方法实现
}
```

## 4 可用的属性和方法

### 4.1 基类提供的属性

| 属性 | 类型 | 说明 | 来源 |
|------|------|------|------|
| `LoginUserID` | Guid | 当前登录用户 ID | BaseServiceImpl |
| `Mapper` | IMapper | Materal.Utils.AutoMapper 映射器 | BaseServiceImpl |
| `UnitOfWork` | TUnitOfWork | 工作单元 | BaseServiceImpl<TUnitOfWork> |
| `DefaultRepository` | TRepository | 主实体默认仓储 | BaseServiceImpl<TRepository, TDomain, TUnitOfWork> |
| `DefaultViewRepository` | TViewRepository | 视图默认仓储 | 带视图的基类 |

### 4.2 基类提供的虚方法

基类提供多个虚方法供重写，以介入 CRUD 流程：

#### 添加相关

| 虚方法 | 返回类型 | 用途 |
|--------|----------|------|
| `AddAsync(TAddModel model)` | `Task<Guid>` | 添加入口，已包含映射逻辑 |
| `AddAsync(TDomain domain, TAddModel model)` | `Task<Guid>` | 添加时介入，可进行规则验证、数据补充 |

#### 编辑相关

| 虚方法 | 返回类型 | 用途 |
|--------|----------|------|
| `EditAsync(TEditModel model)` | `Task` | 编辑入口，已包含查询和映射逻辑 |
| `EditAsync(TDomain domainFromDB, TEditModel model)` | `Task` | 编辑时介入，可进行规则验证 |

#### 删除相关

| 虚方法 | 返回类型 | 用途 |
|--------|----------|------|
| `DeleteAsync(Guid id)` | `Task` | 删除入口 |
| `DeleteAsync(TDomain domain)` | `Task` | 删除时介入，可进行级联删除 |

#### 获取详情相关

| 虚方法 | 返回类型 | 用途 |
|--------|----------|------|
| `GetInfoAsync(TDomain domain)` | `Task<TDTO>` | 实体转 DTO 前介入 |
| `GetInfoAsync(TDTO dto)` | `Task<TDTO>` | 最终钩子，可处理返回的 DTO |

#### 获取列表相关

| 虚方法 | 返回类型 | 用途 |
|--------|----------|------|
| `GetListAsync(TQueryModel model)` | `Task<(List<TListDTO>, RangeModel)>` | 列表查询入口 |
| `GetListAsync(Expression, TQueryModel, ...)` | `Task<(List<TListDTO>, RangeModel)>` | 可添加额外查询条件 |
| `GetListAsync(List<TListDTO>, RangeModel, TQueryModel)` | `Task<(List<TListDTO>, RangeModel)>` | 最终钩子，处理返回结果 |

#### 缓存相关

| 虚方法 | 返回类型 | 用途 |
|--------|----------|------|
| `ClearCacheAsync(object targetRepository)` | `Task` | 自定义缓存清理逻辑 |
| `ClearCacheAsync()` | `Task` | 清理默认仓储缓存 |

### 4.3 可重写的默认排序逻辑

```csharp
/// <summary>
/// 获得默认排序信息
/// </summary>
protected (Expression<Func<T, object>> orderExpression, SortOrder sortOrder) GetDefaultOrderInfo<T>(TQueryModel model)
    where T : class, IDomain
{
    // 优先级：
    // 1. 如果模型中指定了排序（通过 FilterModel 的 SortPropertyName）
    // 2. 如果是 IIndexDomain，使用 Index 排序
    // 3. 默认使用 CreateTime 倒序
}
```

### 4.4 仓储可用方法

`DefaultRepository` 继承自 `IRepository<TDomain>` 接口，提供了丰富的查询方法。以下是常用方法：

#### 存在性检查

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `ExistedAsync(Expression<Func<TDomain, bool>> expression)` | `Task<bool>` | 根据表达式判断是否存在 |
| `ExistedAsync(Guid id)` | `Task<bool>` | 根据主键判断是否存在（Guid 类型主键） |
| `ExistedAsync(FilterModel filterModel)` | `Task<bool>` | 根据 FilterModel 判断是否存在 |

#### 计数

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `CountAsync(Expression<Func<TDomain, bool>> expression)` | `Task<int>` | 根据表达式统计数量 |
| `CountAsync(FilterModel filterModel)` | `Task<int>` | 根据 FilterModel 统计数量 |

#### 获取单条

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `FirstAsync(Guid id)` | `Task<TDomain>` | 根据主键获取（无则抛异常） |
| `FirstAsync(Expression<Func<TDomain, bool>> expression)` | `Task<TDomain>` | 根据表达式获取第一条（无则抛异常） |
| `FirstOrDefaultAsync(Guid id)` | `Task<TDomain?>` | 根据主键获取或默认 |
| `FirstOrDefaultAsync(Expression<Func<TDomain, bool>> expression)` | `Task<TDomain?>` | 根据表达式获取第一条或默认 |

#### 查询多条

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `FindAsync(Expression<Func<TDomain, bool>> expression)` | `Task<List<TDomain>>` | 根据表达式查找所有匹配项 |
| `FindAsync(Expression, Expression<Func<TDomain, object>>, SortOrder)` | `Task<List<TDomain>>` | 带排序的查找 |
| `FindAsync(FilterModel filterModel)` | `Task<List<TDomain>>` | 根据 FilterModel 查找 |

#### 范围查询（返回指定数量）

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `RangeAsync(Expression, long skip, long take)` | `Task<(List<TDomain>, RangeModel)>` | 范围查询 |
| `RangeAsync(Expression, RangeRequestModel)` | `Task<(List<TDomain>, RangeModel)>` | 使用 RangeRequestModel |
| `RangeAsync(Expression, Expression, SortOrder, RangeRequestModel)` | `Task<(List<TDomain>, RangeModel)>` | 带排序的范围查询 |

#### 分页查询

| 方法 | 返回类型 | 说明 |
|------|----------|------|
| `PagingAsync(PageRequestModel)` | `Task<(List<TDomain>, PageModel)>` | 分页查询 |
| `PagingAsync(Expression, long pageIndex, long pageSize)` | `Task<(List<TDomain>, PageModel)>` | 简单分页 |
| `PagingAsync(Expression, Expression, SortOrder, PageRequestModel)` | `Task<(List<TDomain>, PageModel)>` | 带排序的分页 |

**使用示例**：

```csharp
// 存在性检查
bool existed = await DefaultRepository.ExistedAsync(m => m.Account == account);

// 根据主键获取
User? user = await DefaultRepository.FirstOrDefaultAsync(id);

// 表达式查询
List<User> activeUsers = await DefaultRepository.FindAsync(m => m.Status == UserStatus.Active);

// 带排序查询
List<User> users = await DefaultRepository.FindAsync(
    m => m.Status == UserStatus.Active,
    m => m.CreateTime,
    SortOrder.Descending);

// 范围查询
(List<User> users, RangeModel rangeInfo) = await DefaultRepository.RangeAsync(
    expression, model.Skip, model.Take);

// 分页查询
(PageModel pageModel) = new PageModel { PageIndex = 1, PageSize = 10 };
(List<User> users, PageModel pageInfo) = await DefaultRepository.PagingAsync(expression, pageIndex, pageSize);
```

## 5 基本规范

### 5.1 命名规范

| 项目 | 规范 | 示例 |
|------|------|------|
| 服务实现类 | `{EntityName}ServiceImpl` | `UserServiceImpl` |
| 文件位置 | `Application/Services/` | `UserServiceImpl.cs` |
| 命名空间 | `{ProjectName}.{ModuleName}.Application.Services` | `{ProjectName}.{ModuleName}.Application.Services` |

### 5.2 必须使用 partial

服务实现类必须使用 `partial` 关键字，以支持代码生成器：

```csharp
namespace {ProjectName}.{ModuleName}.Application.Services;

/// <summary>
/// 用户服务实现
/// </summary>
public partial class UserServiceImpl : IUserService
{
    // 自定义方法
}
```

### 5.3 依赖注入

使用 C# 12+ 主构造函数注入：

```csharp
public partial class UserServiceImpl(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ILogger<UserServiceImpl>? logger = null)
{
    // 自定义方法
}
```

**注入顺序建议**：
1. 主实体仓储（通过 `DefaultRepository` 访问，无需注入）
2. 自定义仓储
3. 其他服务
4. ILogger（可选）

### 5.4 如何让框架自动将服务注入到容器中

## 6 配置获取规范

### 6.1 使用 IOptionsMonitor<T> 获取配置

**推荐方式**：使用 `IOptionsMonitor<T>` 获取应用程序配置，支持配置热更新。

```csharp
using Microsoft.Extensions.Options;

public partial class AssignmentServiceImpl(
    IOptionsMonitor<ApplicationConfig> optionsMonitor)
{
    /// <summary>
    /// 应用程序配置（只读当前值）
    /// </summary>
    private ApplicationConfig AppConfig => optionsMonitor.CurrentValue;

    /// <summary>
    /// 修改任务置顶类型
    /// </summary>
    public async Task EditTopTypeAsync(EditTopTypeModel model)
    {
        // 使用配置
        int normalTopCost = AppConfig.ReadingBeans.NormalTopCost;
        int priorityTopCost = AppConfig.ReadingBeans.PriorityTopCost;
    }
}
```

**配置类定义**：

```csharp
namespace {ProjectName}.{ModuleName}.Application;

[Options("Main")]
public class ApplicationConfig
{
    /// <summary>
    /// 阅豆配置
    /// </summary>
    public ReadingBeansConfig ReadingBeans { get; set; } = new();
}

public class ReadingBeansConfig
{
    public int NormalTopCost { get; set; } = 20;
    public int PriorityTopCost { get; set; } = 50;
}
```

**appsettings.json 配置**：

```json
{
  "Main": {
    "ReadingBeans": {
      "NormalTopCost": 20,
      "PriorityTopCost": 50
    }
  }
}
```

**不推荐的方式**：直接使用 `IConfiguration.GetValue()`

```csharp
// 不推荐：硬编码配置路径字符串，易出错且不支持类型安全
int normalTopCost = configuration.GetValue<int>("Main:ReadingBeans:NormalTopCost");
```

### 6.2 完整更新 vs 部分更新

**完整更新（PUT）**：替换整个对象，使用 AutoMapper 映射所有属性。

```csharp
/// <summary>
/// 完整更新（代码生成器自动生成）
/// </summary>
protected override async Task EditAsync(Assignment domainFromDB, EditAssignmentModel model)
{
    // 使用 AutoMapper 映射整个对象
    Mapper.Map(model, domainFromDB);
    await base.EditAsync(domainFromDB, model);
}
```

**部分更新（PATCH）**：只更新传入的字段，需要手动赋值。

```csharp
/// <summary>
/// 部分更新（手动实现）
/// </summary>
public async Task EditNormalFieldsAsync(EditNormalFieldsModel model)
{
    Assignment assignment = await DefaultRepository.FirstOrDefaultAsync(model.AssignmentID)
        ?? throw new {ProjectName}Exception("任务不存在");

    // 只更新非 null 的字段
    if (model.Title != null)
    {
        assignment.Title = model.Title;
    }
    if (model.Link != null)
    {
        assignment.Link = model.Link;
    }
    if (model.QRCodeFileID.HasValue)
    {
        assignment.QRCodeFileID = model.QRCodeFileID.Value;
    }

    // 可能还有其他业务逻辑（如减少修改次数）
    assignment.EditCount -= 1;

    UnitOfWork.RegisterEdit(assignment);
    await UnitOfWork.CommitAsync();
}
```

**判断标准**：

| 场景 | 更新方式 | 说明 |
|------|----------|------|
| 替换整个对象的所有属性 | 完整更新 | 使用 AutoMapper |
| 只更新部分字段 | 部分更新 | 手动赋值，null 表示不更新 |
| 需要条件判断才更新 | 部分更新 | 业务逻辑判断 |
| 有自增/自减操作 | 部分更新 | 如 EditCount -= 1 |

## 7 依赖注入

框架使用 `Materal.Extensions.DependencyInjection` 包实现自动注入功能。通过以下方式标记类，框架会自动将其注册到 DI 容器中：

**方式一：实现生命周期接口**

实现以下接口之一即可自动注册，接口泛型参数指定要暴露的服务类型：

```csharp
/// <summary>
/// 瞬态服务 - 每次请求创建新实例
/// </summary>
public interface ITransientDependency : IRegisterType { }
public interface ITransientDependency<T> : ITransientDependency, IRegisterType<T> { }
public interface ITransientDependency<T, T2> : ITransientDependency, IRegisterType<T, T2> { }
public interface ITransientDependency<T, T2, T3> : ITransientDependency, IRegisterType<T, T2, T3> { }
public interface ITransientDependency<T, T2, T3, T4> : ITransientDependency, IRegisterType<T, T2, T3, T4> { }

/// <summary>
/// 作用域服务 - 每次请求创建新实例（默认）
/// </summary>
public interface IScopedDependency : IRegisterType { }
public interface IScopedDependency<T> : IScopedDependency, IRegisterType<T> { }
public interface IScopedDependency<T, T2> : IScopedDependency, IRegisterType<T, T2> { }
public interface IScopedDependency<T, T2, T3> : IScopedDependency, IRegisterType<T, T2, T3> { }
public interface IScopedDependency<T, T2, T3, T4> : IScopedDependency, IRegisterType<T, T2, T3, T4> { }

/// <summary>
/// 单例服务 - 全局唯一实例
/// </summary>
public interface ISingletonDependency : IRegisterType { }
public interface ISingletonDependency<T> : ISingletonDependency, IRegisterType<T> { }
public interface ISingletonDependency<T, T2> : ISingletonDependency, IRegisterType<T, T2> { }
public interface ISingletonDependency<T, T2, T3> : ISingletonDependency, IRegisterType<T, T2, T3> { }
public interface ISingletonDependency<T, T2, T3, T4> : ISingletonDependency, IRegisterType<T, T2, T3, T4> { }
```

**方式二：使用 `[Dependency]` 特性**

```csharp
/// <summary>
/// 依赖注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DependencyAttribute(params Type[] serviceTypes) : ExposeServicesAttribute(serviceTypes)
{
    /// <summary>
    /// 服务生命周期（默认 Scoped）
    /// </summary>
    public virtual ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
    /// <summary>
    /// 注册模式（默认 TryAdd）
    /// </summary>
    public ServiceRegisterMode RegisterMode { get; set; } = ServiceRegisterMode.TryAdd;
    /// <summary>
    /// 服务键标识
    /// </summary>
    public object? Key { get; set; }
}

/// <summary>
/// 将此实例暴露为指定的类型
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class ExposeServicesAttribute(params Type[] serviceTypes) : Attribute
{
    /// <summary>
    /// 暴露的服务类型
    /// </summary>
    public Type[] ServiceTypes { get; } = serviceTypes ?? Type.EmptyTypes;
}

/// <summary>
/// 服务注册模式
/// </summary>
public enum ServiceRegisterMode
{
    /// <summary>尝试添加（已存在则跳过）</summary>
    TryAdd,
    /// <summary>添加（替换已存在）</summary>
    Add,
    /// <summary>替换</summary>
    Replace
}
```

**使用示例**：

```csharp
/// <summary>
/// 通过实现接口自动注册（推荐）
/// </summary>
public class UserServiceImpl : IUserService, ISingletonDependency<IUserService>
{
    // 自动注册为 ISingletonDependency，暴露类型为 IUserService
}

/// <summary>
/// 通过特性指定多个暴露类型
/// </summary>
[Dependency(typeof(IUserService), typeof(IOrderService))]
public class UserServiceImpl : IUserService, IOrderService
{
    // 同时暴露为 IUserService 和 IOrderService
}

/// <summary>
/// 自定义生命周期和注册模式
/// </summary>
[Dependency(typeof(IUserService), Lifetime = ServiceLifetime.Transient, RegisterMode = ServiceRegisterMode.Replace)]
public class UserServiceImpl : IUserService
{
    // 使用 Transient 生命周期，并替换已存在的服务
}
```

## 8 CRUD 方法实现模式

### 8.1 AddAsync 实现

#### 模式一：数据补充

在添加时设置默认值或补充数据：

```csharp
/// <summary>
/// 添加用户
/// </summary>
/// <param name="domain"></param>
/// <param name="model"></param>
/// <returns></returns>
protected override Task<Guid> AddAsync(User domain, AddUserModel model)
{
    // 数据补充
    domain.AddUserID = LoginUserID;
    domain.Enable = true;
    domain.CreateTime = DateTime.UtcNow;
    return base.AddAsync(domain, model);
}
```

#### 模式二：业务规则验证

在添加前验证业务规则：

```csharp
/// <summary>
/// 添加用户
/// </summary>
/// <param name="domain"></param>
/// <param name="model"></param>
/// <returns></returns>
protected override async Task<Guid> AddAsync(User domain, AddUserModel model)
{
    // 验证账号是否已存在
    if (await DefaultRepository.ExistedAsync(m => m.Account == model.Account))
    {
        throw new {ProjectName}Exception("账号已存在");
    }
    return await base.AddAsync(domain, model);
}
```

#### 模式三：复杂业务逻辑

涉及多个步骤的复杂添加逻辑：

```csharp
/// <summary>
/// 发布任务
/// </summary>
/// <param name="domain"></param>
/// <param name="model"></param>
/// <returns></returns>
protected override async Task<Guid> AddAsync(Assignment domain, AddAssignmentModel model)
{
    // 验证业务规则
    AssignmentRequirement requirement = await requirementRepository.FirstOrDefaultAsync(model.RequirementID)
        ?? throw new {ProjectName}Exception("任务需求不存在");
    User user = await userRepository.FirstOrDefaultAsync(LoginUserID)
        ?? throw new {ProjectName}Exception("用户不存在");

    if (user.ReadingBeans < requirement.PublishPrice)
    {
        throw new {ProjectName}Exception($"阅豆不足，需要{requirement.PublishPrice}阅豆");
    }

    // 使用服务扣减资源（封装了扣减+记录明细的逻辑）
    await readingBeansDetailService.ChangeReadingBeansAsync(
        user,
        -requirement.PublishPrice,
        ReadingBeansSource.DelegationConsumption,
        requirement.ID,
        $"发布任务消耗：{requirement.Name}"
    );

    // 补充数据
    domain.PublisherID = LoginUserID;
    domain.PublishTime = DateTime.UtcNow;

    return await base.AddAsync(domain, model);
}
```

### 8.2 EditAsync 实现

```csharp
/// <summary>
/// 编辑用户
/// </summary>
/// <param name="domainFromDB"></param>
/// <param name="model"></param>
/// <returns></returns>
protected override async Task EditAsync(User domainFromDB, EditUserModel model)
{
    // 验证业务规则
    if (model.Status == UserStatus.Locked && domainFromDB.Status != UserStatus.Locked)
    {
        // 检查是否有未完成的订单
        int pendingOrders = await orderRepository.CountAsync(m => m.UserID == model.ID && m.Status == OrderStatus.Pending);
        if (pendingOrders > 0)
        {
            throw new {ProjectName}Exception("该用户有待处理的订单，无法锁定");
        }
    }

    // 更新数据
    domainFromDB.Nickname = model.Nickname;
    domainFromDB.Status = model.Status;

    await base.EditAsync(domainFromDB, model);
}
```

### 8.3 DeleteAsync 实现

```csharp
/// <summary>
/// 删除用户（级联删除）
/// </summary>
/// <param name="domain"></param>
/// <returns></returns>
protected override async Task DeleteAsync(User domain)
{
    // 删除关联数据
    var userRoles = await userRoleRepository.FindAsync(m => m.UserID == domain.ID);
    foreach (var userRole in userRoles)
    {
        UnitOfWork.RegisterDelete(userRole);
    }

    await base.DeleteAsync(domain);
}
```

### 8.4 GetInfoAsync 实现

```csharp
/// <summary>
/// 获取用户信息（补充数据）
/// </summary>
/// <param name="dto"></param>
/// <returns></returns>
protected override async Task<UserDTO> GetInfoAsync(UserDTO dto)
{
    // 补充角色信息
    List<UserRole> userRoles = await userRoleRepository.FindAsync(m => m.UserID == dto.ID);
    List<Role> roles = await roleRepository.FindAsync(m => userRoles.Select(m => m.RoleID).Contains(m.ID));
    dto.Roles = Mapper.Map<List<RoleDTO>>(roles);

    return await base.GetInfoAsync(dto);
}
```

### 8.5 GetListAsync 实现

#### 重写查询表达式

```csharp
/// <summary>
/// 获取用户列表（添加额外查询条件）
/// </summary>
protected override async Task<(List<UserListDTO> data, RangeModel rangeInfo)> GetListAsync(
    Expression<Func<User, bool>> expression,
    QueryUserModel model,
    Expression<Func<User, object>>? orderExpression = null,
    SortOrder sortOrder = SortOrder.Descending)
{
    // 添加额外查询条件
    if (model.DepartmentID != null)
    {
        expression = expression.And(m => m.DepartmentID == model.DepartmentID);
    }
    if (model.Status != null)
    {
        expression = expression.And(m => m.Status == model.Status);
    }

    return await base.GetListAsync(expression, model, orderExpression, sortOrder);
}
```

#### 重写最终结果处理

```csharp
/// <summary>
/// 获取用户列表（补充数据）
/// </summary>
protected override async Task<(List<UserListDTO> data, RangeModel rangeInfo)> GetListAsync(
    List<UserListDTO> listDto,
    RangeModel rangeInfo,
    QueryUserModel model)
{
    // 批量补充部门信息
    List<Guid> departmentIDs = listDto.Where(m => m.DepartmentID != null).Select(m => m.DepartmentID!.Value).Distinct().ToList();
    List<Department> departments = await departmentRepository.FindAsync(m => departmentIDs.Contains(m.ID));
    Dictionary<Guid, Department> departmentDict = departments.ToDictionary(m => m.ID);

    foreach (var dto in listDto)
    {
        if (dto.DepartmentID != null && departmentDict.TryGetValue(dto.DepartmentID.Value, out Department? department))
        {
            dto.DepartmentName = department.Name;
        }
    }

    return await base.GetListAsync(listDto, rangeInfo, model);
}
```

## 9 自定义服务方法实现

### 9.1 无参数方法

```csharp
/// <summary>
/// 获取最末级任务类型列表
/// </summary>
public async Task<List<AssignmentTypeLeafListDTO>> GetLeafListAsync()
{
    List<AssignmentType> leafTypes = await DefaultRepository.FindAsync(m => m.ParentID != null && !m.EnableChildren);
    return Mapper.Map<List<AssignmentTypeLeafListDTO>>(leafTypes);
}
```

### 9.2 带参数方法

```csharp
/// <summary>
/// 重置密码
/// </summary>
public async Task ResetPasswordAsync(ResetPasswordModel model)
{
    User user = await DefaultRepository.FirstOrDefaultAsync(model.UserID)
        ?? throw new {ProjectName}Exception("用户不存在");

    // 验证旧密码
    if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
    {
        throw new {ProjectName}Exception("旧密码错误");
    }

    // 更新密码
    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
    UnitOfWork.RegisterEdit(user);
    await UnitOfWork.CommitAsync();
}
```

## 10 表达式树组合

### 10.1 基本用法

当 FilterModel 无法处理复杂过滤条件时，可以通过表达式树组合手动添加：

```csharp
public async Task<(List<ReadingBeansDetailListDTO> data, RangeModel rangeInfo)> GetListAsync(GetReadingBeansDetailListModel model)
{
    // 获取 FilterModel 生成的基础表达式
    Expression<Func<ReadingBeansDetail, bool>> expression = model.GetSearchExpression<ReadingBeansDetail>();

    // 添加特殊过滤逻辑
    if (model.QueryType == ReadingBeansDetailQueryType.Income)
    {
        // 收入：ChangeAmount > 0
        expression = expression.And(x => x.ChangeAmount > 0);
    }
    else if (model.QueryType == ReadingBeansDetailQueryType.Expenditure)
    {
        // 支出：ChangeAmount < 0
        expression = expression.And(x => x.ChangeAmount < 0);
    }

    // 设置排序并查询
    model.SortPropertyName = nameof(ReadingBeansDetail.CreateTime);
    model.IsAsc = false;

    // 范围查询
    (List<ReadingBeansDetail> details, RangeModel rangeInfo) = await DefaultRepository.RangeAsync(expression, model);

    return (Mapper.Map<List<ReadingBeansDetailListDTO>>(details), rangeInfo);
}
```

### 10.2 组合多个条件

```csharp
Expression<Func<User, bool>> expression = model.GetSearchExpression<User>();

// 组合 OR 条件
if (model.SearchText != null)
{
    expression = expression.And(m =>
        m.Account.Contains(model.SearchText) ||
        m.Nickname.Contains(model.SearchText) ||
        m.Email.Contains(model.SearchText));
}

// 组合日期范围
if (model.StartDate != null)
{
    expression = expression.And(m => m.CreateTime >= model.StartDate);
}
if (model.EndDate != null)
{
    expression = expression.And(m => m.CreateTime <= model.EndDate);
}
```

## 11 空白服务实现

### 11.1 适用场景

- 实体标记了 `[EmptyService]` 特性
- 不需要标准 CRUD 操作
- 完全自定义服务逻辑

### 11.2 实现示例

```csharp
namespace {ProjectName}.{ModuleName}.Application.Services;

/// <summary>
/// 存储文件服务实现
/// </summary>
public partial class StoredFileServiceImpl : BaseServiceImpl, IStoredFileService
{
    public async Task<FileDTO> UploadAsync(UploadModel model)
    {
        // 完全自定义实现
        byte[] fileData = await model.File.OpenReadStream().ReadAllBytesAsync();
        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.File.FileName)}";

        StoredFile storedFile = new()
        {
            ID = Guid.NewGuid(),
            FileName = model.File.FileName,
            FilePath = Path.Combine(_uploadPath, fileName),
            FileSize = fileData.Length,
            ContentType = model.File.ContentType,
            UploadTime = DateTime.UtcNow,
            UploadUserID = LoginUserID
        };

        await File.WriteAllBytesAsync(storedFile.FilePath, fileData);
        UnitOfWork.RegisterAdd(storedFile);
        await UnitOfWork.CommitAsync();

        return Mapper.Map<FileDTO>(storedFile);
    }
}
```

### 11.3 可用方法

空白服务继承简化版 `BaseServiceImpl`，可用属性和方法：

| 类型 | 名称 | 说明 |
|------|------|------|
| 属性 | `Mapper` | 映射器 |
| 属性 | `UnitOfWork` | 工作单元 |
| 属性 | `DefaultRepository` | 默认仓储（如果需要） |
| 属性 | `LoginUserID` | 登录用户 ID |
| 方法 | `ExistedAsync` | 存在性检查 |
| 方法 | `FirstAsync` / `FirstOrDefaultAsync` | 获取单条 |
| 方法 | `FindAsync` | 查询多条 |
| 方法 | `PagingAsync` / `RangeAsync` | 分页/范围查询 |

## 12 与其他服务/仓储的协作

### 12.1 优先使用现有服务

当业务涉及资源扣减时（如阅豆、积分、余额），**优先查找是否有现成的服务封装**：

```csharp
// 推荐：使用服务
await readingBeansDetailService.ChangeReadingBeansAsync(
    user,
    -amount,
    ReadingBeansSource.DelegationConsumption,
    orderID,
    "描述"
);

// 不推荐：手动操作（除非没有现成服务可用）
user.Balance -= amount;
readingBeansDetailRepository.Add(detail);
UnitOfWork.RegisterEdit(user);
await UnitOfWork.CommitAsync();
```

### 12.2 多仓储事务操作

当必须直接操作多个仓储时，使用 `UnitOfWork` 保证事务：

```csharp
public async Task TransferAsync(Guid fromID, Guid toID, decimal amount)
{
    var fromUser = await DefaultRepository.FirstOrDefaultAsync(fromID)
        ?? throw new {ProjectName}Exception("转出用户不存在");
    var toUser = UnitOfWork.GetRepository<IUserRepository>().FirstOrDefault(toID)
        ?? throw new {ProjectName}Exception("转入用户不存在");

    if (fromUser.Balance < amount)
    {
        throw new {ProjectName}Exception("余额不足");
    }

    fromUser.Balance -= amount;
    toUser.Balance += amount;

    // 两个仓储的更改会在同一个事务中提交
    UnitOfWork.RegisterEdit(fromUser);
    UnitOfWork.RegisterEdit(toUser);
    await UnitOfWork.CommitAsync();
}
```

### 12.3 调用其他模块服务

通过依赖注入调用其他模块的服务（**只允许注入仓储和服务，不允许注入控制器**）：

```csharp
public partial class StudentServiceImpl(
    IUserService userService,              // 其他模块的服务
    IEmployeeService employeeService,      // 其他模块的服务
    IClassStudentRepository classStudentRepository)
{
    public async Task<AddStudentResultDTO> AddStudentAsync(AddStudentModel model)
    {
        // 调用其他模块的服务
        AddUserRequestModel requestModel = Mapper.Map<AddUserRequestModel>(model.BaseInfo);
        requestModel.HasRoleCodes = [SystemCode.Role_Student];

        // 通过服务获取账号
        string account = await userService.GetNewAccountByDateAsync("yyyyMMdd");
        requestModel.Account = account;

        Guid userID = await userService.AddUserAsync(requestModel);

        return new AddStudentResultDTO
        {
            ID = userID,
            Account = account
        };
    }
}
```

### 12.4 工作单元

工作单元（UnitOfWork）用于管理仓储操作的事务，确保多操作原子性。

#### 12.4.1 IUnitOfWork 接口

```csharp
public interface IUnitOfWork
{
    /// <summary>DI服务</summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>提交事务</summary>
    Task CommitAsync(bool setDetached = true);

    /// <summary>同步提交事务</summary>
    void Commit(bool setDetached = true);

    /// <summary>注册新增</summary>
    void RegisterAdd<TEntity, TPrimaryKeyType>(TEntity obj)
        where TEntity : class, IEntity<TPrimaryKeyType>
        where TPrimaryKeyType : struct;

    /// <summary>尝试注册新增（返回是否成功）</summary>
    bool TryRegisterAdd<TEntity, TPrimaryKeyType>(TEntity obj)
        where TEntity : class, IEntity<TPrimaryKeyType>
        where TPrimaryKeyType : struct;

    /// <summary>注册修改</summary>
    void RegisterEdit<TEntity, TPrimaryKeyType>(TEntity obj)
        where TEntity : class, IEntity<TPrimaryKeyType>
        where TPrimaryKeyType : struct;

    /// <summary>尝试注册修改</summary>
    bool TryRegisterEdit<TEntity, TPrimaryKeyType>(TEntity obj)
        where TEntity : class, IEntity<TPrimaryKeyType>
        where TPrimaryKeyType : struct;

    /// <summary>注册删除</summary>
    void RegisterDelete<TEntity, TPrimaryKeyType>(TEntity obj)
        where TEntity : class, IEntity<TPrimaryKeyType>
        where TPrimaryKeyType : struct;

    /// <summary>尝试注册删除</summary>
    bool TryRegisterDelete<TEntity, TPrimaryKeyType>(TEntity obj)
        where TEntity : class, IEntity<TPrimaryKeyType>
        where TPrimaryKeyType : struct;

    /// <summary>获取仓储</summary>
    TRepository GetRepository<TRepository>()
        where TRepository : IRepository;
}
```

#### 12.4.2 注册操作

所有注册操作都是"延迟提交"的，只有调用 `CommitAsync` 时才会真正执行：

```csharp
// 注册新增
UnitOfWork.RegisterAdd(newUser);

// 注册修改
UnitOfWork.RegisterEdit(existingUser);

// 注册删除
UnitOfWork.RegisterDelete(userToDelete);

// 提交事务（一次性保存所有更改）
await UnitOfWork.CommitAsync();
```

#### 12.4.3 TryRegister 方法

`TryRegister` 系列方法返回 `bool` 值，表示是否注册成功：

```csharp
// 如果用户有关联订单，注册会失败
if (!UnitOfWork.TryRegisterDelete(user))
{
    throw new {ProjectName}Exception("该用户有待处理的订单，无法删除");
}
await UnitOfWork.CommitAsync();
```

#### 12.4.4 获取仓储

可以通过 `UnitOfWork` 获取其他仓储：

```csharp
// 获取用户仓储
IUserRepository userRepo = UnitOfWork.GetRepository<IUserRepository>();

// 获取角色仓储
IRoleRepository roleRepo = UnitOfWork.GetRepository<IRoleRepository>();
```

#### 12.4.5 事务特性

工作单元具有以下特性：

- **原子性**：所有注册的操作在一次 `CommitAsync` 中完成，要么全部成功，要么全部回滚
- **延迟执行**：注册操作不会立即执行，只在提交时执行
- **自动追踪**：框架会自动追踪实体的变更状态

## 13 异常处理

### 13.1 使用项目异常类

所有自定义异常应继承自 `{ProjectName}Exception`，框架会自动捕获并处理：

```csharp
// 正确：使用 {ProjectName}Exception
if (user == null)
{
    throw new {ProjectName}Exception("用户不存在");
}

// 正确：继承 {ProjectName}Exception
public class UserNotFoundException : {ProjectName}Exception
{
    public Guid UserID { get; }

    public UserNotFoundException(Guid userID)
        : base($"用户不存在: {userID}")
    {
        UserID = userID;
    }
}

// 错误：不要直接抛出系统异常
// throw new ArgumentNullException(nameof(request));

// 错误：不要在控制器层返回 BadRequest
// return BadRequest("用户不存在");
throw new {ProjectName}Exception("用户不存在");
```

### 13.2 异常传播

服务层抛出的 `{ProjectName}Exception` 会被框架自动捕获并转换为统一的错误响应，控制器层**不需要**也不应该捕获服务层的异常：

```csharp
// 服务层：只需抛出异常
public async Task<User> GetUserAsync(Guid id)
{
    User user = await DefaultRepository.FirstOrDefaultAsync(id)
        ?? throw new {ProjectName}Exception("用户不存在");
    return user;
}

// 控制器层：不要 try-catch
public async Task<UserDTO> GetUserAsync(Guid id)
{
    // 框架会自动处理异常
    return await _userService.GetUserAsync(id);
}
```

## 14 性能优化

### 14.1 避免 N+1 查询

```csharp
// 错误示例：循环中查询
foreach (var dto in result)
{
    var user = await userRepository.FirstOrDefaultAsync(dto.UserID); // N次数据库查询
}

// 正确示例：一次性查询
List<Guid> userIDs = [.. result.Select(r => r.UserID)];
List<User> allUsers = await userRepository.FindAsync(u => userIDs.Contains(u.ID));
Dictionary<Guid, User> userDict = allUsers.ToDictionary(u => u.ID);

foreach (var dto in result)
{
    dto.UserName = userDict.TryGetValue(dto.UserID, out User? user) ? user.Name : string.Empty;
}
```

### 14.2 字典缓存模式

当需要根据关联 ID 查找数据时，先批量查询再建立字典：

```csharp
// 批量查询所有相关实体
List<Guid> assignmentIDs = [.. records.Select(r => r.AssignmentID).Distinct()];
List<Assignment> assignments = await assignmentRepository.FindAsync(a => assignmentIDs.Contains(a.ID));
Dictionary<Guid, Assignment> assignmentDict = assignments.ToDictionary(a => a.ID);

// 在内存中快速查找
foreach (var record in records)
{
    if (assignmentDict.TryGetValue(record.AssignmentID, out Assignment? assignment))
    {
        dto.AssignmentTitle = assignment.Title;
    }
}
```

### 14.3 优先使用 DefaultRepository

对于主实体使用 `DefaultRepository`，仅额外注入其他仓储：

```csharp
public partial class CompletionRecordServiceImpl(
    IUserRepository userRepository,
    IAssignmentRepository assignmentRepository)
{
    public async Task GetDataAsync()
    {
        // 主实体使用 DefaultRepository
        List<CompletionRecord> records = await DefaultRepository.FindAsync(filter);

        // 其他实体通过注入的仓储访问
        List<User> users = await userRepository.FindAsync(u => u.Enabled);
    }
}
```

### 14.4 数据库层面分组统计

推荐使用自定义 SQL 在数据库层面完成分组统计：

```csharp
// 推荐：数据库层面分组统计
var groupCompletionCountMap = await completionRecordRepository.CountByGroupsAsync(
    LoginUserID, model.StartTime, model.EndTime);

// 不推荐：先查询全部再内存分组统计
var allRecords = await completionRecordRepository.FindAsync(...);
var result = allRecords
    .GroupBy(x => x.GroupID)
    .ToDictionary(g => g.Key, g => g.Count());
```

## 15 禁止操作

- **不要**在 MGC 文件夹下编写任何代码
- **不要**忘记使用 `partial` 关键字
- **不要**直接抛出系统异常（如 `ArgumentNullException`）
- **不要**在服务层处理 HTTP 响应（返回 `IActionResult` 等）
- **不要**在服务层拼接 HTTP URL（如 `/api/StoredFile/GetFile?...`）
- **不要**使用 `!` 规避 null 警告，应明确处理 null 情况
- **不要**保留未使用的方法参数

## 16 正确操作

- 服务实现放在 `Application/Services/` 目录
- 遵循 [编码规范](coding-style.md) 中的命名和格式要求
- 为所有公开方法添加 XML 文档注释
- 使用结构化日志记录关键操作
- 优先使用 `DefaultRepository` 访问主实体
- 优先使用现有服务封装业务逻辑
- 使用表达式树组合替代内存过滤
- 异常抛出由框架统一处理

## 17 代码示例

### 17.1 完整的服务实现示例

```csharp
namespace {ProjectName}.{ModuleName}.Application.Services;

/// <summary>
/// 用户服务实现
/// </summary>
public partial class UserServiceImpl : IUserService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<UserServiceImpl> _logger;

    public UserServiceImpl(
        IRoleRepository roleRepository,
        ILogger<UserServiceImpl> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    /// <summary>
    /// 添加用户（数据补充）
    /// </summary>
    protected override Task<Guid> AddAsync(User domain, AddUserModel model)
    {
        domain.AddUserID = LoginUserID;
        domain.Enable = true;
        domain.CreateTime = DateTime.UtcNow;
        domain.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        return base.AddAsync(domain, model);
    }

    /// <summary>
    /// 编辑用户（业务验证）
    /// </summary>
    protected override async Task EditAsync(User domainFromDB, EditUserModel model)
    {
        // 验证账号唯一性
        if (model.Account != domainFromDB.Account)
        {
            bool existed = await DefaultRepository.ExistedAsync(m => m.Account == model.Account && m.ID != model.ID);
            if (existed)
            {
                throw new {ProjectName}Exception("账号已存在");
            }
        }

        domainFromDB.Nickname = model.Nickname;
        domainFromDB.Account = model.Account;
        domainFromDB.UpdateTime = DateTime.UtcNow;

        await base.EditAsync(domainFromDB, model);
    }

    /// <summary>
    /// 获取用户信息（补充数据）
    /// </summary>
    protected override async Task<UserDTO> GetInfoAsync(UserDTO dto)
    {
        // 补充角色信息
        List<UserRole> userRoles = await userRoleRepository.FindAsync(m => m.UserID == dto.ID);
        List<Role> roles = await _roleRepository.FindAsync(r => userRoles.Select(m => m.RoleID).Contains(r.ID));
        dto.Roles = Mapper.Map<List<RoleDTO>>(roles);

        return await base.GetInfoAsync(dto);
    }

    /// <summary>
    /// 获取用户列表（添加查询条件）
    /// </summary>
    protected override async Task<(List<UserListDTO> data, RangeModel rangeInfo)> GetListAsync(
        Expression<Func<User, bool>> expression,
        QueryUserModel model,
        Expression<Func<User, object>>? orderExpression = null,
        SortOrder sortOrder = SortOrder.Descending)
    {
        // 添加状态筛选
        if (model.Status != null)
        {
            expression = expression.And(m => m.Status == model.Status);
        }

        return await base.GetListAsync(expression, model, orderExpression, sortOrder);
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    public async Task ResetPasswordAsync(ResetPasswordModel model)
    {
        User user = await DefaultRepository.FirstOrDefaultAsync(model.UserID)
            ?? throw new {ProjectName}Exception("用户不存在");

        if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
        {
            throw new {ProjectName}Exception("旧密码错误");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.UpdateTime = DateTime.UtcNow;

        UnitOfWork.RegisterEdit(user);
        await UnitOfWork.CommitAsync();

        _logger.LogInformation(
            "用户 {UserID} 在 {Time} 重置密码",
            model.UserID,
            DateTime.UtcNow);
    }
}
```

### 17.2 视图查询服务实现示例

```csharp
namespace {ProjectName}.{ModuleName}.Application.Services;

/// <summary>
/// 班级视图服务实现
/// </summary>
public partial class ClassServiceImpl : IClassService
{
    /// <summary>
    /// 获取班级列表（使用视图仓储）
    /// </summary>
    protected override async Task<(List<ClassListDTO> data, RangeModel rangeInfo)> GetViewListAsync(
        Expression<Func<ClassView, bool>> expression,
        QueryClassModel model,
        Expression<Func<ClassView, object>>? orderExpression = null,
        SortOrder sortOrder = SortOrder.Descending)
    {
        // 添加分类筛选
        if (model.CategoryID != null)
        {
            expression = expression.And(m => m.CategoryID == model.CategoryID);
        }

        return await base.GetViewListAsync(expression, model, orderExpression, sortOrder);
    }

    /// <summary>
    /// 获取班级列表（最终处理）
    /// </summary>
    protected override async Task<(List<ClassListDTO> data, RangeModel rangeInfo)> GetListAsync(
        List<ClassListDTO> listDto,
        RangeModel rangeInfo,
        QueryClassModel model)
    {
        // 批量补充班主任信息
        List<Guid> sponsorIDs = listDto.Select(m => m.ClassSponsorID).Distinct().ToList();
        List<Employee> sponsors = await employeeRepository.FindAsync(m => sponsorIDs.Contains(m.ID));
        Dictionary<Guid, Employee> sponsorDict = sponsors.ToDictionary(m => m.ID);

        foreach (var dto in listDto)
        {
            if (sponsorDict.TryGetValue(dto.ClassSponsorID, out Employee? sponsor))
            {
                dto.ClassSponsorName = sponsor.Name;
            }
        }

        return await base.GetListAsync(listDto, rangeInfo, model);
    }
}
```
