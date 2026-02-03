# DTO 设计规范

本文档描述了在 Materal.MergeBlock 项目中设计 DTO（Data Transfer Object）时需要遵循的规范。

## DTO 类型

### 1. 代码生成器自动生成的 DTO

| DTO 类型 | 命名 | 继承 | 生成条件 |
|----------|------|------|----------|
| **ListDTO** | `{EntityName}ListDTO` | 实现 `IListDTO` | 实体没有 `[NotListDTO]` 特性 |
| **DTO** | `{EntityName}DTO` | 继承 ListDTO，实现 `IDTO` | 实体没有 `[NotDTO]` 特性 |
| **TreeListDTO** | `{EntityName}TreeListDTO` | 继承 ListDTO，实现 `ITreeDTO<T>` | 实体实现 `ITreeDomain`，没有 `[EmptyTree]` 或 `[NotListDTO]` |

### 2. 自定义 DTO

手动创建的自定义 DTO，用于满足特殊业务需求：

```csharp
namespace {ProjectName}.{ModuleName}.Abstractions.DTO.Class;

/// <summary>
/// 班级数据传输模型
/// </summary>
public partial class ClassDTO
{
    /// <summary>
    /// 班级类型备注
    /// </summary>
    public string? ClassCategoryRemark { get; set; }
}
```

## 基本规范

### 命名规范

| 类型 | 命名格式 | 示例 |
|------|----------|------|
| 列表 DTO | `{EntityName}ListDTO` | `UserListDTO` |
| 详情 DTO | `{EntityName}DTO` | `UserDTO` |
| 树形列表 DTO | `{EntityName}TreeListDTO` | `DepartmentTreeListDTO` |
| 统计 DTO | `{Feature}StatisticDTO` | `UserStatisticDTO` |
| 结果 DTO | `{Operation}ResultDTO` | `AddUserResultDTO` |
| 自定义 DTO | `{Purpose}{EntityName}DTO` | `UserProfileDTO` |

### 文件位置

```
{ModuleName}.Abstractions/
└── DTO/
    └── {EntityName}/
        ├── {EntityName}DTO.cs
        └── {EntityName}ListDTO.cs
```

### 命名空间

```
{ProjectName}.{ModuleName}.Abstractions.DTO.{EntityName}
```

**实际示例**（参考 {ProjectName}.{ModuleName} 项目）：

```csharp
// 位置: {ProjectName}.{ModuleName}.Abstractions/DTO/AssignmentType/AssignmentTypeLeafListDTO.cs
namespace {ProjectName}.{ModuleName}.Abstractions.DTO.AssignmentType;
public partial class AssignmentTypeLeafListDTO { }
```

## 代码生成器行为

### 影响 DTO 生成的特性

| 特性 | 作用目标 | 说明 |
|------|----------|------|
| `[NotDTO]` | 类/属性 | 类：不生成 DTO；属性：不出现在任何 DTO |
| `[NotListDTO]` | 类/属性 | 类：不生成 ListDTO；属性：不出现在 ListDTO，但出现在 DTO |
| `[DTOText]` | 属性 | 为枚举属性自动生成 `{PropertyName}Text` 只读属性 |
| `[QueryView(ViewName)]` | 类 | 指定使用哪个领域模型生成 DTO 属性 |
| `[EmptyTree]` | 类 | 树形实体不生成 TreeListDTO |

### ListDTO 固定属性

```csharp
/// <summary>
/// 唯一标识
/// </summary>
[Required(ErrorMessage = "唯一标识为空")]
public Guid ID { get; set; }

/// <summary>
/// 创建时间
/// </summary>
[Required(ErrorMessage = "创建时间为空")]
public DateTime CreateTime { get; set; }
```

## 自定义 DTO 场景

### 场景 1：添加关联实体的 DTO 属性

```csharp
namespace {ProjectName}.{ModuleName}.Abstractions.DTO.Student;

/// <summary>
/// 学生班级数据传输模型
/// </summary>
public partial class StudentDTO
{
    /// <summary>
    /// 班级信息
    /// </summary>
    public ClassListDTO ClassInfo { get; set; } = new();
}
```

### 场景 2：添加计算属性

```csharp
/// <summary>
/// 班级统计数据传输模型
/// </summary>
public partial class ClassScheduleFeekbackStatisticDTO
{
    /// <summary>
    /// 完成百分比
    /// </summary>
    public decimal PercentageComplete => Count <= 0 ? 1 : (decimal)AlreadyFeekback / Count;
}
```

### 场景 3：添加枚举文本属性

如需自定义枚举文本属性，可在自定义 DTO 中添加：

```csharp
/// <summary>
/// 班级状态
/// </summary>
public ClassStatus Status { get; set; }

/// <summary>
/// 状态文本
/// </summary>
public string StatusText => Status.GetDescription();
```

## 属性类型

### 基础类型

```csharp
// 字符串
public string Name { get; set; } = string.Empty;
public string? Description { get; set; }

// 数值
public int Count { get; set; }
public decimal Amount { get; set; }

// 日期
public DateTime Date { get; set; }
public DateTime? EndDate { get; set; }

// 枚举
public UserStatus Status { get; set; }
```

### 集合类型

```csharp
// 列表
public List<StudentListDTO> Students { get; set; } = new();

// 数组
public Guid[] AdjunctIDs { get; set; } = [];
```

## 文档注释

所有 DTO 类和属性必须包含 XML 文档注释：

```csharp
namespace {ProjectName}.{ModuleName}.Abstractions.DTO.User;

/// <summary>
/// 用户数据传输模型
/// </summary>
public partial class UserDTO
{
    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; } = string.Empty;
}
```

## 返回类型规范（Service 层）

Service 层返回 DTO 时，控制器会自动将返回值包装为 `ResultModel<T>`：

| Service 层返回 | Controller 返回 |
|----------------|-----------------|
| `Task<{Entity}DTO>` | `Task<ResultModel<{Entity}DTO>>` |
| `Task<List<{Entity}DTO>>` | `Task<ResultModel<List<{Entity}DTO>>>` |
| `Task<(List<T> data, RangeModel rangeInfo)>` | `Task<CollectionResultModel<T>>` |

## 设计建议

1. **控制属性暴露**：使用 `[NotDTO]`、`[NotListDTO]` 特性控制敏感字段不暴露给前端
2. **添加关联信息**：在 DTO 中嵌套关联实体的 DTO，避免 N+1 查询问题
3. **使用计算属性**：将复杂计算逻辑放在 DTO 中，保持服务层简洁
4. **枚举文本显示**：使用 `[DTOText]` 特性或手动添加 `{PropertyName}Text` 属性
5. **保持职责单一**：自定义 DTO 专注于特定场景，不要过度设计
6. **命名空间组织**：按实体模块组织 DTO 文件，保持目录结构清晰
