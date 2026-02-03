# 核心工具使用指南

## 概述

`Materal.Utils` 是一个基础工具库，提供丰富的模型定义、扩展方法和辅助类，涵盖分页查询、结果封装、类型转换、JSON 处理、属性过滤等常用功能。

## 安装

```bash
dotnet add package Materal.Utils
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

## 核心模型

### 分页模型

#### PageRequestModel - 分页请求模型

分页请求的基类，支持自定义起始页码。

```csharp
using Materal.Utils.Models;

// 设置起始页码（默认为1）
PageRequestModel.PageStartNumber = 1;

// 创建分页请求
var request = new PageRequestModel(1, 10); // 页码1，每页10条
request.SortPropertyName = "CreateTime";
request.IsAsc = true;

// 属性说明
// PageIndex      - 当前页码（从 PageStartNumber 开始）
// PageSize       - 每页数量
// Skip           - 跳过数量（可覆盖 PageSkip）
// Take           - 获取数量
// PageSkip       - 根据页码和每页数量计算的跳过数量
```

#### PageModel - 分页结果模型

继承自 `PageRequestModel`，包含分页查询结果。

```csharp
using Materal.Utils.Models;

// 直接构造
var pageModel = new PageModel(1, 10, 100); // 页码1，每页10条，总数100条

// 从请求模型构造
var request = new PageRequestModel(1, 10);
var pageModel = new PageModel(request, 100); // 自动计算分页信息

// 属性说明
// PageCount      - 总页数
// StartIndex     - 开始序号（从1开始）
// DataCount      - 数据总数
```

#### RangeRequestModel - 范围请求模型

用于范围查询的请求模型，支持跳过和获取数量。

```csharp
using Materal.Utils.Models;

var request = new RangeRequestModel(0, 10); // 跳过0条，获取10条
request.SortPropertyName = "Name";
request.IsAsc = true;
```

#### RangeModel - 范围结果模型

范围查询的结果模型。

```csharp
using Materal.Utils.Models;

var rangeModel = new RangeModel(0, 10, 100); // 跳过0，获取10，总数100
```

### 返回模型

#### ResultModel - 基础返回模型

用于统一 API 返回格式。

```csharp
using Materal.Utils.Models;
using Materal.Utils.Enums;

// 成功返回
var successResult = ResultModel.Success("操作成功");

// 失败返回
var failResult = ResultModel.Fail("参数错误");

// 警告返回
var warningResult = ResultModel.Warning("请确认是否继续");

// 从异常构造
try
{
    // ...
}
catch (Exception ex)
{
    var errorResult = new ResultModel(ex); // 自动提取错误信息
}
```

#### ResultModel\<T\> - 泛型返回模型

带数据的返回模型。

```csharp
using Materal.Utils.Models;

// 成功返回数据
var result = ResultModel<User>.Success(user, "获取成功");

// 失败返回（无数据）
var failResult = ResultModel<User>.Fail("用户不存在");

// 静态方法
// Success(T data, string? message = null)
// Success(string? message = null)
// Fail(T data, string? message = null)
// Fail(string? message = null)
// Warning(T data, string? message = null)
// Warning(string? message = null)
```

#### CollectionResultModel\<T\> - 集合返回模型

用于返回列表数据的模型，支持分页信息。

```csharp
using Materal.Utils.Models;

var users = new List<User> { user1, user2, user3 };
var pageModel = new PageModel(1, 10, 100);

// 成功返回列表（带分页）
var result = CollectionResultModel<User>.Success(users, pageModel, "查询成功");

// 成功返回列表（带范围模型）
var rangeModel = new RangeModel(0, 10, 100);
var result = CollectionResultModel<User>.Success(users, rangeModel);
```

### FilterModel - 过滤器模型

用于动态构建查询表达式。

```csharp
using Materal.Utils.Models;
using System.Linq.Expressions;

// 获得查询表达式
Expression<Func<User, bool>> expression = filterModel.GetSearchExpression<User>();

// 获得排序表达式
Expression<Func<User, object>>? sortExpression = filterModel.GetSortExpression<User>();

// 获得查询委托（内存数据筛选用）
Func<User, bool> searchDelegate = filterModel.GetSearchDelegate<User>();

// 设置排序（IQueryable）
IQueryable<User> sortedQuery = filterModel.SetSortExpression(users);

// 设置排序（ICollection）
List<User> sortedList = filterModel.Sort(users);
```

### KeyValueModel - 键值对模型

用于枚举转键值对列表。

```csharp
using Materal.Utils.Models;

// 基础键值对
var kv1 = new KeyValueModel("Key1", "Value1");

// 泛型键值对
var kv2 = new KeyValueModel<int, string>(1, "One");

// 枚举键值对
var kvList = KeyValueModel<UserStatus>.GetAllCode();
// 生成：[{ Key: UserStatus.Active, Value: "激活" }, ...]

// 扩展方法
Dictionary<string, string> dict = kvList.ToDictionary();
List<KeyValueModel> kvModels = dict.ToKeyValueModel();
```

## 属性过滤特性

用于 `FilterModel` 的属性标记，自动生成查询表达式。

### EqualAttribute - 等于过滤

```csharp
using Materal.Utils.Models.Attributes;

public class UserFilterModel : FilterModel
{
    [Equal]
    public Guid? ID { get; set; }

    [Equal]
    public UserStatus? Status { get; set; }

    // 指定目标属性名
    [Equal(targetPropertyName: "IsEnabled")]
    public bool? Enabled { get; set; }
}

// 生成：m => m.ID == filter.ID && m.Status == filter.Status
```

### ContainsAttribute - 包含过滤（模糊查询）

```csharp
using Materal.Utils.Models.Attributes;

public class UserFilterModel : FilterModel
{
    [Contains]
    public string? Name { get; set; }

    [Contains]
    public string? Email { get; set; }
}

// 生成：m => m.Name.Contains(filter.Name) && m.Email.Contains(filter.Email)
```

### StartContainsAttribute - 开头包含

```csharp
using Materal.Utils.Models.Attributes;

public class ProductFilterModel : FilterModel
{
    [StartContains]
    public string? Code { get; set; }
}

// 生成：m => m.Code.StartsWith(filter.Code)
```

### EndContainsAttribute - 结尾包含

```csharp
using Materal.Utils.Models.Attributes;

public class FileFilterModel : FilterModel
{
    [EndContains]
    public string? Extension { get; set; }
}

// 生成：m => m.Extension.EndsWith(filter.Extension)
```

### NotEqualAttribute - 不等于过滤

```csharp
using Materal.Utils.Models.Attributes;

public class OrderFilterModel : FilterModel
{
    [NotEqual]
    public OrderStatus? ExcludeStatus { get; set; }
}

// 生成：m => m.ExcludeStatus != filter.ExcludeStatus
```

### 比较过滤特性

| 特性 | 说明 | 示例 |
|------|------|------|
| `GreaterThan` | 大于 (>) | `m.Price > 100` |
| `GreaterThanOrEqual` | 大于等于 (>=) | `m.Quantity >= 10` |
| `LessThan` | 小于 (<) | `m.Age < 18` |
| `LessThanOrEqual` | 小于等于 (<=) | `m.Score <= 100` |

```csharp
using Materal.Utils.Models.Attributes;

public class ProductFilterModel : FilterModel
{
    [GreaterThan]
    public decimal? MinPrice { get; set; }

    [LessThan]
    public decimal? MaxPrice { get; set; }

    [GreaterThanOrEqual(targetPropertyName: "CreateTime")]
    public DateTime? StartDate { get; set; }
}
```

## 扩展方法

### 字符串扩展

#### 转换扩展

```csharp
using Materal.Utils.Extensions;

// 转换为枚举
string statusStr = "Active";
UserStatus status = statusStr.ConvertToEnum<UserStatus>();

// 首字母转换
string lower = "userName".ToLowerFirstLetter();  // "username"
string upper = "userName".ToUpperFirstLetter();  // "UserName"
```

#### 验证扩展

```csharp
using Materal.Utils.Extensions;

// 格式验证
"user@example.com".IsEMail();          // 是否为邮箱
"12345678901".IsPhoneNumber();         // 是否为手机号
"192.168.1.1".IsIPv4();                // 是否为IPv4
"https://example.com".IsUrl();         // 是否为URL
"123456".IsInteger();                  // 是否为整数
"123.45".IsNumber();                   // 是否为数字
"中文".IsChinese();                    // 是否为中文
"ABC123".IsLetterOrNumber();           // 是否为字母或数字

// 获取匹配内容
MatchCollection? emails = "test@example.com test2@test.com".GetEMail();

// 身份证验证
"110101199001011234".IsIDCardForChina();      // 18位身份证
"110101900101123".IsIDCard15ForChina();       // 15位身份证

// 日期时间
"2024-01-15".IsDate();             // 是否为日期
"14:30:00".IsTime();               // 是否为时间
"2024-01-15 14:30:00".IsDateTime(); // 是否为日期时间

// JSON/XML
"{'name':'test'}".IsJson();        // 是否为JSON
"{'name':'test'}".IsObjectJson();  // 是否为JSON对象
"[1,2,3]".IsArrayJson();           // 是否为JSON数组
"<root></root>".IsXml();           // 是否为XML

// GUID
"550e8400-e29b-41d4-a716-446655440000".IsGuid();
```

### 日期时间扩展

```csharp
using Materal.Utils.Extensions;
using System.Globalization;

// 时间戳转换
DateTime now = DateTime.Now;
long timestamp = now.GetTimeStamp();           // 秒级时间戳

// 转换为DateTimeOffset
DateTimeOffset offset = now.ToDateTimeOffset();

// 日期/时间组件提取
DateTime date = new(2024, 3, 15);
int quarter = date.GetQuarterOfYear();         // 年份季度（1-4）
int monthOfQuarter = date.GetMonthOfQuarter(); // 季度中月份（1-3）
int weekOfQuarter = date.GetWeekOfQuarter();   // 季度中周数
int dayOfQuarter = date.GetDayOfQuarter();     // 季度中天数
int weekOfYear = date.GetWeekOfYear();         // 年份周数
int weekOfMonth = date.GetWeekOfMonth();       // 月份周数

// 日期边界
DateTime dateTime = new(2024, 3, 15, 14, 30, 0);
dateTime.GetDayFirstSecond();    // 当天第一秒 00:00:00
dateTime.GetDayLastSecond();     // 当天最后一秒 23:59:59
dateTime.GetMonthFirstSecond();  // 当月第一秒
dateTime.GetMonthLastSecond();   // 当月最后一秒
dateTime.GetYearFirstSecond();   // 当年第一秒
dateTime.GetYearLastSecond();    // 当年最后一秒

// DateOnly/TimeOnly转换
DateOnly dateOnly = dateTime.ToDateOnly();
TimeOnly timeOnly = dateTime.ToTimeOnly();
DateTime combined = new DateOnly(2024, 1, 1).ToDateTime();
DateTime combined2 = new TimeOnly(14, 30).ToDateTime(new DateOnly(2024, 1, 1));
```

### 数字扩展

```csharp
using Materal.Utils.Extensions;

// 转换为中文
123.ConvertToSimplifiedChinese();  // "一百二十三"
456.ConvertToCapitalChinese();     // "肆佰伍拾陆"

// 二进制字符串
10.GetBinaryString();  // "1010"
```

### 集合扩展

```csharp
using Materal.Utils.Extensions;
using System.Collections.ObjectModel;

// 转换为ObservableCollection
IEnumerable<User> users = GetUsers();
ObservableCollection<User> observable = users.ToObservableCollection();

// 集合对比（新增/删除）
var newRoles = new List<string> { "Admin", "User" };
var oldRoles = new List<string> { "User", "Guest" };
var (addList, removeList) = oldRoles.GetAddArrayAndRemoveArray(newRoles);
// addList: ["Admin"]
// removeList: ["Guest"]

// 自定义比较器去重
List<User> distinctUsers = users.Distinct((a, b) => a.Name == b.Name);

// HashSet去重（高性能）
IEnumerable<User> deduped = users.DistinctByHashSet(u => u.Name);
IEnumerable<User> deduped2 = users.DistinctByHashSet(u => u.Name, StringComparer.OrdinalIgnoreCase);
```

### JSON扩展

```csharp
using Materal.Utils.Extensions;

// 序列化/反序列化
User user = new() { Name = "张三", Age = 25 };
string json = user.ToJson();  // {"Name":"张三","Age":25}

User? deserializedUser = json.JsonToObject<User>();

// 保留类型信息（用于接口/基类）
string typeJson = user.ToJsonWithInferredTypes();
IUser? restored = json.JsonToObjectWithInferredTypes<IUser>("TypeName");

// 接口类型反序列化
ITestInterface result = jsonStr.JsonToInterface<ITestInterface>("ImplementationTypeName");
```

### 对象扩展

```csharp
using Materal.Utils.Extensions;

// 空值判断
object? obj = null;
obj.IsNullOrEmptyString();      // 是否为空或空字符串
obj.IsNullOrWhiteSpaceString(); // 是否为空或空白字符串

// 对象比较（指定属性映射）
bool equals = objA.Equals(objB, new Dictionary<string, Func<object?, bool>>
{
    ["Name"] = v => v?.ToString() == "test"
});

// 获取对象属性值（支持嵌套和路径）
var user = new User { Name = "张三", Address = new Address { City = "北京" } };

// 简单属性
string? name = user.GetObjectValue<string>("Name");  // "张三"

// 嵌套属性
string? city = user.GetObjectValue<string>("Address.City");  // "北京"

// 数组/集合元素
var list = new List<string> { "A", "B", "C" };
string? first = list.GetObjectValue<string>("[0]");  // "A"

// 字典值
var dict = new Dictionary<string, object> { ["Key"] = "Value" };
object? value = dict.GetObjectValue("Key");
```

## 辅助类

### DateTimeHelper - 日期时间辅助类

```csharp
using Materal.Utils.Helpers;
using Materal.Utils.Enums;

// 获得时间戳
long timestamp = DateTimeHelper.GetTimeStamp();           // UTC时间戳
long localTimestamp = DateTimeHelper.GetTimeStamp(DateTimeKind.Local);

// 时间戳转DateTime
DateTime dateTime = DateTimeHelper.TimeStampToDateTime(1704067200);

// 时间戳转DateTimeOffset
DateTimeOffset offset = DateTimeHelper.TimeStampToDateTimeOffset(1704067200);

// 时间单位转换
double ms = DateTimeHelper.ToMilliseconds(5, DateTimeUnit.Minute);  // 300000
double seconds = DateTimeHelper.ToSeconds(1, DateTimeUnit.Hour);    // 3600
double minutes = DateTimeHelper.ToMinutes(2, DateTimeUnit.Day);     // 2880
double hours = DateTimeHelper.ToHours(48, DateTimeUnit.Day);        // 48
double days = DateTimeHelper.ToDay(30, DateTimeUnit.Month);        // ~0.985
double months = DateTimeHelper.ToMonth(1, DateTimeUnit.Year);      // 12
double years = DateTimeHelper.ToYear(12, DateTimeUnit.Month);      // 1
```

### ConvertHelper - 类型转换辅助类

```csharp
using Materal.Utils.Helpers;

// 转换为指定类型
int num = ConvertHelper.ConvertTo<int>("123");
long longNum = ConvertHelper.ConvertTo<long>("123456789");
bool boolVal = ConvertHelper.ConvertTo<bool>("true");
DateTime dateTime = ConvertHelper.ConvertTo<DateTime>("2024-01-15");
Guid guid = ConvertHelper.ConvertTo<Guid>("550e8400-e29b-41d4-a716-446655440000");

// 判断是否支持转换
bool canConvert = ConvertHelper.CanConvertTo<DateTime>();  // true

// 添加自定义转换
ConvertHelper.AddConvertDictionary<MyEnum>(value => (MyEnum)int.Parse(value.ToString()));
```

### CloneHelper - 对象克隆辅助类

```csharp
using Materal.Utils.Helpers;

// JSON方式克隆（推荐，性能最佳）
User clonedUser = CloneHelper.CloneByJson(user);

// 反射克隆（支持深度克隆）
User? clonedUser2 = CloneHelper.CloneByReflex(user);

// XML序列化克隆
User? clonedUser3 = CloneHelper.CloneByXml(user);

// 属性复制
User target = new();
CloneHelper.CopyProperties(source, target);           // 复制所有属性
CloneHelper.CopyProperties(source, target, "ID", "CreateTime");  // 排除指定属性

// 属性复制并返回新对象
User newUser = CloneHelper.CopyProperties<User>(source, "Password");  // 排除Password
```

### StringHelper - 字符串辅助类

```csharp
using Materal.Utils.Helpers;

// 随机字符串（GUID模式）
string guidStr = StringHelper.GetRandomStringByGuid(32);  // 32位随机字符串
string randomStr = StringHelper.GetRandomStringByGuid(8, 16);  // 8-16位随机

// 随机字符串（字典模式）
string random = StringHelper.GetRandomStringByDictionary(16);
string random2 = StringHelper.GetRandomStringByDictionary(16, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

// 随机字符串（时间戳模式）
string tickStr = StringHelper.GetRandomStringByTick(10);
```

### ConfigHelper - 配置辅助类

用于动态修改 appsettings.json 配置文件。

```csharp
using Materal.Utils.Helpers;
using Microsoft.Extensions.Configuration;

// 注入服务
builder.Services.AddSingleton<ConfigHelper>();

// 使用
IConfigurationRoot configuration = builder.Build();
var configHelper = new ConfigHelper(configuration);

// 设置配置值
configHelper.SetValue("AppSettings:Name", "新名称");
configHelper.SetValue("AppSettings:Port", 8080);

// 保存更改到文件
configHelper.SaveChanges();
```

### FileHelper - 文件辅助类

```csharp
using Materal.Utils.Helpers;

// 判断是否为图片文件
bool isImage = FileHelper.IsImageFile("photo.jpg", out string? imageType);
bool isImageFile = FileHelper.IsImageFile("document.pdf");
```

## 完整示例

### 分页查询服务

```csharp
using Materal.Utils.Models;
using Materal.Utils.Models.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Example;

public class UserFilterModel : FilterModel
{
    [Contains]
    public string? Name { get; set; }

    [Contains]
    public string? Email { get; set; }

    [Equal]
    public UserStatus? Status { get; set; }

    [GreaterThanOrEqual(targetPropertyName: "CreateTime")]
    public DateTime? StartTime { get; set; }

    [LessThanOrEqual(targetPropertyName: "CreateTime")]
    public DateTime? EndTime { get; set; }
}

public interface IUserService
{
    Task<CollectionResultModel<User>> GetUsersAsync(UserFilterModel filter);
}

public class UserService(MyDbContext context) : IUserService
{
    private readonly MyDbContext _context = context;

    public async Task<CollectionResultModel<User>> GetUsersAsync(UserFilterModel filter)
    {
        IQueryable<User> query = _context.Users.AsQueryable();

        // 应用过滤条件
        Expression<Func<User, bool>>? expression = filter.GetSearchExpression<User>();
        if (expression != null)
        {
            query = query.Where(expression);
        }

        // 获取总数
        int totalCount = await query.CountAsync();

        // 设置排序
        query = filter.SetSortExpression(query, u => u.CreateTime, false);

        // 分页
        query = query.Skip((int)filter.Skip).Take((int)filter.Take);
        List<User> users = await query.ToListAsync();

        // 构建分页模型
        PageModel pageModel = new(filter, totalCount);

        return CollectionResultModel<User>.Success(users, pageModel);
    }
}
```

### API控制器返回

```csharp
using Materal.Utils.Models;
using Microsoft.AspNetCore.Mvc;

namespace Example.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<CollectionResultModel<UserDto>> GetUsers([FromQuery] UserFilterModel filter)
    {
        return await _userService.GetUsersAsync(filter);
    }

    [HttpGet("{id}")]
    public async Task<ResultModel<UserDto>> GetUserAsync(Guid id)
    {
        User user = await _userService.GetByIDAsync(id);
        return ResultModel<UserDto>.Success(_mapper.Map<UserDto>(user));
    }

    [HttpPost]
    public async Task<ResultModel> CreateUser(CreateUserRequest request)
    {
        try
        {
            await _userService.CreateAsync(request);
            return ResultModel.Success("创建成功");
        }
        catch (Exception ex)
        {
            // 记录错误日志，返回友好错误信息
            return ResultModel.Fail("操作失败，请稍后重试");
        }
    }
}
```

### 对象操作示例

```csharp
using Materal.Utils.Extensions;
using Materal.Utils.Helpers;

// 对象克隆
var original = new User { Name = "张三", Age = 25 };
var cloned = CloneHelper.CloneByJson(original);

// 属性复制
var target = new UserDto();
CloneHelper.CopyProperties(original, target, "Password");

// JSON序列化
string json = original.ToJson();
User? restored = json.JsonToObject<User>();

// 日期时间处理
DateTime now = DateTime.Now;
long timestamp = now.GetTimeStamp();
DateTime fromStamp = DateTimeHelper.TimeStampToDateTime(timestamp);

// 字符串验证
bool isEmail = "test@example.com".IsEMail();
bool isPhone = "13800138000".IsPhoneNumber();
```

## 枚举定义

### ResultType - 返回结果类型

```csharp
public enum ResultType
{
    [Description("成功")]
    Success = 0,

    [Description("失败")]
    Fail = 1,

    [Description("警告")]
    Warning = 2
}
```

### DateTimeUnit - 日期时间单位

```csharp
public enum DateTimeUnit
{
    [Description("年")]
    YearUnit = 0,

    [Description("月")]
    MonthUnit = 1,

    [Description("日")]
    DayUnit = 2,

    [Description("时")]
    HourUnit = 3,

    [Description("分")]
    MinuteUnit = 4,

    [Description("秒")]
    SecondUnit = 5,

    [Description("毫秒")]
    MillisecondUnit = 6
}
```

## 注意事项

1. **分页起始页码**：默认从 1 开始，可通过 `PageRequestModel.PageStartNumber` 修改
2. **JSON序列化**：默认配置不转换 camelCase，保持原始属性名大小写
3. **过滤器特性**：属性值为 null 或空字符串时，对应条件会被忽略
4. **可空类型处理**：对于可空类型属性，特性会自动添加 HasValue 检查
5. **克隆性能**：推荐使用 `CloneByJson`，性能最佳且支持度广
