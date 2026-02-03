# 代码生成器指南

## 概述

Materal.MergeBlock 框架提供了强大的代码生成器，可以根据实体、枚举、服务接口自动生成 DTO、请求模型、服务层、控制器层等代码，大幅提升开发效率。

## 使用方式

### 基本命令

在模块根目录下运行：

```bash
MMB GeneratorCode
```

### 带参数运行

```bash
# 指定模块路径
MMB GeneratorCode -p E:\Project\MyProject\MyProject.User

# 或使用完整参数
MMB GeneratorCode --ProjectPath E:\Project\MyProject\MyProject.User
```

> **重要**：代码生成器只能在模块根目录（如 `MyProject.User`）下运行，不能在项目根目录或核心项目目录下运行。

## 执行流程

代码生成工具按以下顺序执行：

1. **清理**：删除所有 `MGC`（MergeBlock Generated Code）目录
2. **刷新**：重新扫描项目文件
3. **前置处理**：执行所有插件的 `BeforeExcuteAsync` 方法
4. **代码生成**：执行所有插件的 `ExcuteAsync` 方法
5. **后置处理**：执行所有插件的 `AfterExcuteAsync` 方法

## 生成的文件结构

运行代码生成器后，会在以下位置生成 `MGC` 文件夹：

```
{ProjectName}.{ModuleName}.Abstractions/
├── MGC/
│   ├── Controllers/        ← 控制器接口
│   ├── ControllerAccessors/← 控制器访问器
│   ├── DTO/                ← 数据传输对象
│   ├── Repositories/       ← 仓储接口
│   ├── RequestModel/       ← 请求模型
│   └── Services/           ← 服务接口及模型
{ProjectName}.{ModuleName}.Application/
├── MGC/
│   ├── Controllers/        ← 控制器实现
│   └── Services/           ← 服务实现
{ProjectName}.{ModuleName}.Repository/
└── MGC/
    ├── EntityConfigs/      ← 实体配置
    ├── Repositories/       ← 仓储实现
    └── {ModuleName}DBContext.cs ← 数据库上下文
```

> **重要**：`MGC` 目录下的代码会在每次运行代码生成器时被完全删除重建，**所有自定义代码都不应编写在 MGC 文件夹下**。

## 默认插件

代码生成器包含以下插件，按顺序执行：

| 插件 | 功能 |
|------|------|
| `EnumControllerGeneratorCodePlug` | 生成枚举相关的控制器代码 |
| `DTOGeneratorCodePlug` | 生成 DTO（数据传输对象）类 |
| `RequesetModelGeneratorCodePlug` | 生成请求模型类 |
| `ServicesModelGeneratorCodePlug` | 生成服务模型接口 |
| `RepositoryGeneratorCodePlug` | 生成仓储层代码 |
| `ServicesGeneratorCodePlug` | 生成服务层代码 |
| `ControllerGeneratorCodePlug` | 生成控制器代码 |
| `ControllerAccessorsGeneratorCodePlug` | 生成控制器访问器代码 |
| `ControllerMapperGeneratorCodePlug` | 生成控制器映射器代码 |

## 各插件生成内容

### 1. DTOGeneratorCodePlug

生成数据传输对象（DTO）类：

| 文件 | 说明 |
|------|------|
| `{EntityName}ListDTO.cs` | 列表DTO，继承 `IListDTO`，包含基本字段（ID、CreateTime） |
| `{EntityName}DTO.cs` | 单个DTO，继承 `{EntityName}ListDTO` 并实现 `IDTO`，包含详情字段 |
| `{EntityName}TreeListDTO.cs` | 树形列表DTO（仅树形实体） |

**跳过条件**：`[NotListDTO]`、`[NotDTO]`

### 2. RequesetModelGeneratorCodePlug

生成请求模型类：

| 文件 | 说明 | 继承 |
|------|------|------|
| `Add{EntityName}RequestModel.cs` | 添加请求模型 | `IAddRequestModel` |
| `Edit{EntityName}RequestModel.cs` | 编辑请求模型 | `IEditRequestModel` |
| `Query{EntityName}RequestModel.cs` | 查询请求模型 | `PageRequestModel, IQueryRequestModel` |
| `Query{EntityName}TreeListRequestModel.cs` | 树查询请求模型（仅树形实体） | `FilterModel` |

**跳过条件**：`[NotAdd]`、`[NotEdit]`、`[NotQuery]`

### 3. ServicesModelGeneratorCodePlug

生成服务层模型类：

| 文件 | 说明 | 继承 |
|------|------|------|
| `Add{EntityName}Model.cs` | 添加模型 | `IAddServiceModel` |
| `Edit{EntityName}Model.cs` | 编辑模型 | `IEditServiceModel` |
| `Query{EntityName}Model.cs` | 查询模型 | `PageRequestModel, IQueryServiceModel` |
| `Query{EntityName}TreeListModel.cs` | 树查询模型（仅树形实体） | `FilterModel` |

### 4. RepositoryGeneratorCodePlug

生成仓储层代码：

| 文件 | 说明 |
|------|------|
| `{EntityName}Config.cs` | 实体配置类，继承 `BaseEntityConfig<{EntityName}>` |
| `{EntityName}DBContext.cs` | 数据库上下文，包含所有 `DbSet<{EntityName}>` |
| `I{EntityName}Repository.cs` | 仓储接口 |
| `{EntityName}RepositoryImpl.cs` | 仓储实现类 |

**跳过条件**：`[NotEntityConfig]`、`[NotInDBContext]`、`[NotRepository]`

### 5. ServicesGeneratorCodePlug

生成服务层代码：

| 文件 | 说明 |
|------|------|
| `I{EntityName}Service.cs` | 服务接口，继承 `IBaseService<...>` |
| `{EntityName}ServiceImpl.cs` | 服务实现类，继承 `BaseServiceImpl<...>` |

**自动生成方法**：
- 标准 CRUD：`AddAsync`、`EditAsync`、`DeleteAsync`、`GetInfoAsync`、`GetListAsync`
- 位序实体：`ExchangeIndexAsync`（需 `[EmptyIndex]` 跳过）
- 树形实体：`ExchangeParentAsync`、`GetTreeListAsync`（需 `[EmptyTree]` 跳过）

**跳过条件**：`[NotService]`、`[EmptyService]`

### 6. ControllerGeneratorCodePlug

生成控制器层代码：

| 文件 | 说明 |
|------|------|
| `I{EntityName}Controller.cs` | 控制器接口 |
| `{EntityName}Controller.cs` | 控制器实现类 |

**自动生成 API 端点**：

| 方法 | HTTP | 路由 | 说明 |
|------|------|------|------|
| `AddAsync` | POST | `api/{entity}/add` | 添加 |
| `EditAsync` | PUT | `api/{entity}/edit` | 修改 |
| `DeleteAsync` | DELETE | `api/{entity}/delete?id={id}` | 删除 |
| `GetInfoAsync` | GET | `api/{entity}/getinfo?id={id}` | 获取单个 |
| `GetListAsync` | POST | `api/{entity}/getlist` | 获取列表 |
| `ExchangeIndexAsync` | PUT | `api/{entity}/exchangeindex` | 交换位序（位序实体） |
| `ExchangeParentAsync` | PUT | `api/{entity}/exchangeparent` | 更改父级（树形实体） |
| `GetTreeListAsync` | POST | `api/{entity}/gettreelist` | 查询树列表（树形实体） |

**跳过条件**：`[NotController]`、`[EmptyController]`

### 7. EnumControllerGeneratorCodePlug

生成枚举控制器代码，输出 `EnumsController.cs`，提供获取所有枚举值的 API。

**生成的接口**：
| 方法 | HTTP | 路由 | 说明 |
|------|------|------|------|
| `GetListAsync` | GET | `/Enums/GetList` | 获取所有枚举类型及其值 |

**枚举列表接口**：直接调用 `/Enums/GetList` 可获取所有枚举类型的完整列表，返回格式如下：

```csharp
public class EnumInfo
{
    /// <summary>
    /// 枚举类型名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 枚举值列表
    /// </summary>
    public List<EnumValue>? Values { get; set; }
}

public class EnumValue
{
    /// <summary>
    /// 值名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 值
    /// </summary>
    public int Value { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}
```

**使用场景**：客户端需要动态获取枚举选项列表时调用，无需手动编写 API。

### 8. ControllerAccessorsGeneratorCodePlug

生成控制器访问器代码，用于服务间调用。

### 9. ControllerMapperGeneratorCodePlug

根据服务方法上的 `[MapperController]` 特性生成映射控制器代码。

## 特性控制生成

### 类级别特性

| 特性 | 说明 | 受影响插件 |
|------|------|-----------|
| `[NotAdd]` | 不生成 Add 相关代码 | RequestModel, ServicesModel |
| `[NotEdit]` | 不生成 Edit 相关代码 | RequestModel, ServicesModel |
| `[NotQuery]` | 不生成 Query 相关代码 | RequestModel, ServicesModel, DTO |
| `[NotService]` | 不生成服务层代码 | Services |
| `[NotController]` | 不生成控制器层代码 | Controller |
| `[NotRepository]` | 不生成仓储层代码 | Repository |
| `[NotDTO]` | 不生成 DTO | DTO |
| `[NotListDTO]` | 不生成 ListDTO | DTO |
| `[NotEntityConfig]` | 不生成实体配置 | Repository |
| `[NotInDBContext]` | 不在 DbContext 中生成 | Repository |
| `[EmptyService]` | 生成空白服务（无 CRUD） | Services |
| `[EmptyController]` | 生成空白控制器（无 CRUD） | Controller |
| `[EmptyTree]` | 树形实体不生成树相关代码 | Services, Controller |
| `[EmptyIndex]` | 位序实体不生成位序交换代码 | Services, Controller |
| `[Cache]` | 使用缓存仓储 | Repository |

### 属性级别特性

| 特性 | 说明 | 生成影响 |
|------|------|---------|
| `[Equal]` | 等值查询 | Query 模型生成单个参数 |
| `[Contains]` | 模糊查询 | Query 模型生成参数 |
| `[Between]` | 范围查询 | Query 模型生成 Min/Max 参数 |
| `[NotAdd]` | 不可添加 | AddRequestModel/Model 不包含此属性 |
| `[NotEdit]` | 不可编辑 | EditRequestModel/Model 不包含此属性 |
| `[NotQuery]` | 不可查询 | Query 模型不包含此属性 |
| `[NotDTO]` | 不在 DTO 中暴露 | DTO 不包含此属性 |
| `[NotListDTO]` | 不在 ListDTO 中暴露 | ListDTO 不包含此属性 |
| `[LoginUserID]` | 登录用户 ID | 自动填充当前用户 ID |
| `[DTOText]` | DTO 文本 | DTO 生成 `{PropertyName}Text` 属性 |

## 标准 CRUD 接口说明

代码生成器基于 `Materal.MergeBlock` 框架，生成的服务和控制器继承以下基类和接口：

### IBaseService 接口

**命名空间**：`Materal.MergeBlock.Abstractions.Services`

```csharp
public interface IBaseService
{
    /// <summary>
    /// 登录用户ID
    /// </summary>
    Guid LoginUserID { get; set; }
}

public interface IBaseService<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO> : IBaseService
    where TAddModel : class, IAddServiceModel, new()
    where TEditModel : class, IEditServiceModel, new()
    where TQueryModel : IQueryServiceModel, new()
    where TDTO : class, IDTO
    where TListDTO : class, IListDTO
{
    /// <summary>
    /// 添加
    /// </summary>
    Task<Guid> AddAsync(TAddModel model);
    /// <summary>
    /// 修改
    /// </summary>
    Task EditAsync(TEditModel model);
    /// <summary>
    /// 删除
    /// </summary>
    Task DeleteAsync(Guid id);
    /// <summary>
    /// 获得信息
    /// </summary>
    Task<TDTO> GetInfoAsync(Guid id);
    /// <summary>
    /// 获得列表
    /// </summary>
    Task<(List<TListDTO> data, RangeModel rangeInfo)> GetListAsync(TQueryModel model);
}
```

### BaseServiceImpl 基类

**命名空间**：`Materal.MergeBlock.Application.Abstractions.Services`

`BaseServiceImpl` 是一个泛型基类，有多个重载版本：

#### 基础版本

```csharp
public abstract class BaseServiceImpl : IBaseService
{
    /// <summary>
    /// 登录用户唯一标识
    /// </summary>
    public Guid LoginUserID { get; set; }
    /// <summary>
    /// 映射器
    /// </summary>
    protected IMapper Mapper { get; set; }
}
```

#### 工作单元版本

```csharp
public abstract class BaseServiceImpl<TUnitOfWork> : BaseServiceImpl
    where TUnitOfWork : IMergeBlockUnitOfWork
{
    /// <summary>
    /// 工作单元
    /// </summary>
    protected TUnitOfWork UnitOfWork { get; }
}
```

#### 仓储版本

```csharp
public abstract class BaseServiceImpl<TRepository, TDomain, TUnitOfWork> : BaseServiceImpl<TUnitOfWork>
    where TRepository : class, IEFRepository<TDomain, Guid>, IRepository
    where TDomain : class, IDomain, new()
    where TUnitOfWork : IMergeBlockUnitOfWork
{
    /// <summary>
    /// 默认仓储
    /// </summary>
    protected TRepository DefaultRepository { get; }
}
```

#### 完整 CRUD 版本

```csharp
public abstract class BaseServiceImpl<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO, TRepository, TDomain, TUnitOfWork>
    : BaseServiceImpl<TRepository, TDomain, TUnitOfWork>
    where TAddModel : class, IAddServiceModel, new()
    where TEditModel : class, IEditServiceModel, new()
    where TQueryModel : PageRequestModel, IQueryServiceModel, new()
    where TDTO : class, IDTO
    where TListDTO : class, IListDTO
    where TRepository : class, IEFRepository<TDomain, Guid>, IRepository
    where TDomain : class, IDomain, new()
    where TUnitOfWork : IMergeBlockUnitOfWork
{
    /// <summary>
    /// 添加
    /// </summary>
    public virtual Task<Guid> AddAsync(TAddModel model);
    /// <summary>
    /// 修改
    /// </summary>
    public virtual Task EditAsync(TEditModel model);
    /// <summary>
    /// 删除
    /// </summary>
    public virtual Task DeleteAsync(Guid id);
    /// <summary>
    /// 获得信息
    /// </summary>
    public virtual Task<TDTO> GetInfoAsync(Guid id);
    /// <summary>
    /// 获得列表
    /// </summary>
    public virtual Task<(List<TListDTO> data, RangeModel rangeInfo)> GetListAsync(TQueryModel model);
}
```

**泛型参数说明**：

| 泛型参数 | 说明 |
|---------|------|
| `TAddModel` | 添加模型 |
| `TEditModel` | 编辑模型 |
| `TQueryModel` | 查询模型 |
| `TDTO` | 单个数据传输对象 |
| `TListDTO` | 列表数据传输对象 |
| `TRepository` | 仓储类型 |
| `TViewRepository` | 视图仓储类型 |
| `TDomain` | 实体类型 |
| `TViewDomain` | 视图实体类型 |
| `TUnitOfWork` | 工作单元类型 |

**受保护成员说明**：

| 成员 | 类型 | 说明 |
|------|------|------|
| `Mapper` | `IMapper` | AutoMapper 映射器 |
| `UnitOfWork` | `TUnitOfWork` | 工作单元 |
| `DefaultRepository` | `TRepository` | 默认仓储 |
| `DefaultViewRepository` | `TViewRepository` | 默认视图仓储 |

### IMergeBlockController 接口

**命名空间**：`Materal.MergeBlock.Web.Abstractions.Controllers`

```csharp
public interface IMergeBlockController { }

public interface IMergeBlockController<TAddRequestModel, TEditRequestModel, TQueryRequestModel, TDTO, TListDTO>
    : IMergeBlockController
    where TAddRequestModel : class, IAddRequestModel, new()
    where TEditRequestModel : class, IEditRequestModel, new()
    where TQueryRequestModel : IQueryRequestModel, new()
    where TDTO : class, IDTO, new()
    where TListDTO : class, IListDTO, new()
{
    /// <summary
    /// >添加
    /// </summary>
    [HttpPost]
    Task<ResultModel<Guid>> AddAsync(TAddRequestModel requestModel);
    /// <summary>
    /// 修改
    /// </summary>
    [HttpPut]
    Task<ResultModel> EditAsync(TEditRequestModel requestModel);
    /// <summary>
    /// 删除
    /// </summary>
    [HttpDelete]
    Task<ResultModel> DeleteAsync(Guid id);
    /// <summary>
    /// 获得信息
    /// </summary>
    [HttpGet]
    Task<ResultModel<TDTO>> GetInfoAsync(Guid id);
    /// <summary>
    /// 获得列表
    /// </summary>
    [HttpPost]
    Task<CollectionResultModel<TListDTO>> GetListAsync(TQueryRequestModel requestModel);
}
```

### MergeBlockController 基类

**命名空间**：`Materal.MergeBlock.Web.Abstractions.Controllers`

**类特性**：`[Route("api/[controller]/[action]"), ApiController]`

#### 基础版本

```csharp
public abstract class MergeBlockController : ControllerBase
{
    /// <summary>
    /// 自动映射
    /// </summary>
    protected IMapper Mapper { get; }
    /// <summary>
    /// 获得客户端IP
    /// </summary>
    protected string GetClientIP();
    /// <summary>
    /// 绑定LoginUserID
    /// </summary>
    protected void BindLoginUserID(object model);
}
```

#### 服务版本

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

#### 完整版本

```csharp
public abstract class MergeBlockController<TAddRequestModel, TEditRequestModel, TQueryRequestModel, TAddModel, TEditModel, TQueryModel, TDTO, TListDTO, TService>
    : MergeBlockController<TService>, IMergeBlockController<TAddRequestModel, TEditRequestModel, TQueryRequestModel, TDTO, TListDTO>
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
    /// 获得信息
    /// </summary>
    [HttpGet]
    public virtual Task<ResultModel<TDTO>> GetInfoAsync(Guid id);
    /// <summary>
    /// 获得列表
    /// </summary>
    [HttpPost]
    public virtual Task<CollectionResultModel<TListDTO>> GetListAsync(TQueryRequestModel requestModel);
}
```

**API 端点说明**：

| 路由前缀 | HTTP | 方法 | 说明 |
|---------|------|------|------|
| `api/[controller]/[action]` | POST | `AddAsync` | 添加 |
| `api/[controller]/[action]` | PUT | `EditAsync` | 修改 |
| `api/[controller]/[action]` | DELETE | `DeleteAsync` | 删除 |
| `api/[controller]/[action]` | GET | `GetInfoAsync` | 获取单个 |
| `api/[controller]/[action]` | POST | `GetListAsync` | 获取列表 |

### 视图服务支持

对于使用 `[QueryView]` 特性的实体，框架提供了视图版本的 `BaseServiceImpl`：

```csharp
public abstract class BaseServiceImpl<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO, TRepository, TViewRepository, TDomain, TViewDomain, TUnitOfWork>
    : BaseServiceImpl<TAddModel, TEditModel, TQueryModel, TDTO, TListDTO, TRepository, TDomain, TUnitOfWork>
{
    /// <summary>
    /// 默认视图仓储
    /// </summary>
    protected TViewRepository DefaultViewRepository { get; }
    /// <summary>
    /// 重写GetInfoAsync从视图查询
    /// </summary>
    public override Task<TDTO> GetInfoAsync(Guid id);
    /// <summary>
    /// 重写GetListAsync从视图查询
    /// </summary>
    public override Task<(List<TListDTO> data, RangeModel rangeInfo)> GetListAsync(TQueryModel model);
}
```

## 领域模型识别

代码生成器通过扫描以下目录来识别领域模型、服务、控制器和枚举：

| 目录 | 扫描条件 | 说明 |
|------|---------|------|
| `Domain/` | 所有 `.cs` 文件 | 实体定义目录 |
| `Services/` | 以 `I` 开头、包含 `Service.`、以 `.cs` 结尾 | 服务接口目录 |
| `Controllers/` | 以 `I` 开头、包含 `Controller.`、以 `.cs` 结尾 | 控制器接口目录 |
| `Enums/` | 所有 `.cs` 文件 | 枚举定义目录 |

### 实体类型识别

实体类通过以下方式被识别：

1. **普通实体**：继承 `BaseEntity` 或实现实体接口
2. **树形实体**：实现 `ITreeDomain` 接口
3. **位序实体**：实现 `IIndexDomain` 接口
4. **视图实体**：添加 `[View]` 特性

```csharp
// 普通实体
public class User { }

// 树形实体（带分组）
[TreeGroup]
public class Category : ITreeDomain { }

// 位序实体（带分组）
[IndexGroup]
public class Banner : IIndexDomain { }

// 视图实体
[View]
public class UserView { }

// 查询视图（关联到另一个实体）
[QueryView(typeof(User))]
public class UserDetail { }
```

### 领域模型属性

`DomainModel` 类包含以下关键属性：

| 属性 | 说明 |
|------|------|
| `Name` | 类名 |
| `BaseClass` | 基类名 |
| `Interfaces` | 实现的接口列表 |
| `Properties` | 属性列表 |
| `Methods` | 方法列表 |
| `IsTreeDomain` | 是否为树形实体 |
| `IsIndexDomain` | 是否为位序实体 |
| `IsView` | 是否为视图 |

### 领域模型扩展方法

```csharp
// 获取查询领域（用于 QueryView）
DomainModel targetDomain = domain.GetQueryDomain(domains);

// 获取树形分组属性
PropertyModel? treeProperty = domain.GetTreeGroupProperty();

// 获取位序分组属性
PropertyModel? indexProperty = domain.GetIndexGroupProperty();
```

## 属性模型

`PropertyModel` 类描述了领域模型的属性：

| 属性 | 说明 |
|------|------|
| `Name` | 属性名 |
| `PredefinedType` | 属性类型 |
| `CanNull` | 是否可空 |
| `NullPredefinedType` | 可空类型（自动添加 ?） |
| `NotNullPredefinedType` | 不可空类型（移除 ?） |
| `Initializer` | 默认值 |
| `Annotation` | 注释 |
| `Attributes` | 特性列表 |
| `VerificationAttributes` | 验证特性（Required、MinLength 等） |
| `QueryAttributes` | 查询特性（Equal、Contains、Between 等） |
| `HasQueryAttribute` | 是否有查询特性 |

### 查询特性白名单

代码生成器识别以下查询特性：

| 特性 | 查询方式 |
|------|---------|
| `[Equal]` | 等值匹配 |
| `[NotEqual]` | 不等值匹配 |
| `[Contains]` | 模糊查询 |
| `[GreaterThan]` | 大于 |
| `[GreaterThanOrEqual]` | 大于等于 |
| `[LessThan]` | 小于 |
| `[LessThanOrEqual]` | 小于等于 |
| `[StartContains]` | 开头匹配 |
| `[Between]` | 范围查询（生成 Min/Max 属性） |

## 插件接口

所有代码生成插件需实现 `IMergeBlockGeneratorCodePlug` 接口：

```csharp
public interface IMergeBlockGeneratorCodePlug
{
    /// <summary>
    /// 前置处理
    /// </summary>
    Task BeforeExcuteAsync(GeneratorCodeContext context);

    /// <summary>
    /// 执行代码生成
    /// </summary>
    Task ExcuteAsync(GeneratorCodeContext context);

    /// <summary>
    /// 后置处理
    /// </summary>
    Task AfterExcuteAsync(GeneratorCodeContext context);
}
```

### 插件执行时机

| 插件 | 执行时机 |
|------|---------|
| `EnumControllerGeneratorCodePlug` | 后置处理 |
| `ControllerAccessorsGeneratorCodePlug` | 后置处理 |
| 其他插件 | 执行阶段 |

## 上下文对象

`GeneratorCodeContext` 是代码生成的核心上下文对象：

### 路径属性

| 属性 | 说明 |
|------|------|
| `CoreAbstractionsPath` | 核心抽象层路径 |
| `CoreAbstractionsMGCPath` | 核心抽象层 MGC 路径 |
| `CoreRepositoryPath` | 核心仓储路径 |
| `CoreApplicationPath` | 核心应用层路径 |
| `ModuleAbstractionsPath` | 模块抽象层路径 |
| `ModuleAbstractionsMGCPath` | 模块抽象层 MGC 路径 |
| `ModuleApplicationPath` | 模块应用层路径 |
| `ModuleApplicationMGCPath` | 模块应用层 MGC 路径 |
| `ModuleRepositoryPath` | 模块仓储路径 |
| `ModuleRepositoryMGCPath` | 模块仓储 MGC 路径 |
| `ModuleWebAPIPath` | 模块 WebAPI 路径 |

### 数据属性

| 属性 | 说明 |
|------|------|
| `ProjectName` | 项目名称 |
| `ModuleName` | 模块名称 |
| `Domains` | 领域模型列表 |
| `Services` | 服务模型列表 |
| `Controllers` | 控制器模型列表 |
| `Enums` | 枚举模型列表 |
| `GeneratorCodePlugs` | 代码生成插件列表 |

### 核心方法

```csharp
// 保存代码文件
context.SaveAs(stringBuilder, directoryPath, "DTO", "User", "UserDTO.cs");

// 删除所有 MGC 目录
context.DeleteAllMGCDirectorys();

// 刷新扫描
context.Refresh();
```

## 执行时机

建议在以下时机执行代码生成：

1. 创建新模块项目后
2. 添加新的实体类或枚举后
3. 修改服务接口后
4. 运行项目前

## 注意事项

1. **只能在模块根目录下运行**：代码生成工具只能在模块根目录下运行
2. **生成代码的位置**：所有生成的代码都会放在 `MGC` 目录下
3. **不要手动修改 MGC 目录**：该目录的代码是自动生成的，手动修改会在下次生成时被覆盖
4. **执行顺序**：按照开发流程，在设计完实体、服务、控制器后运行代码生成器
