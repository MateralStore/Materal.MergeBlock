# 腾讯云存储工具使用指南

## 概述

`Materal.Utils.CloudStorage.Tencent` 是一个腾讯云 COS（对象存储）操作工具库，提供文件上传、下载、删除、临时密钥获取等功能。

## 安装

```bash
dotnet add package Materal.Utils.CloudStorage.Tencent
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

### 依赖包

- `Materal.Utils`（基础工具包）
- `COSXML`（腾讯云 COS SDK）

## 配置

### TencentCloudStorageConfig

在 `appsettings.json` 中添加配置：

```json
{
  "TencentCloudStorage": {
    "AppID": "your-app-id",
    "SecretID": "your-secret-id",
    "SecretKey": "your-secret-key",
    "DefaultBucketName": "your-bucket-name",
    "DefaultRegion": "ap-guangzhou"
  }
}
```

**配置属性说明**：

| 属性 | 说明 | 默认值 |
|------|------|--------|
| `AppID` | 应用程序标识 | - |
| `SecretID` | 密钥ID | - |
| `SecretKey` | 密钥Key | - |
| `DefaultBucketName` | 默认存储桶名称 | "Default" |
| `DefaultRegion` | 默认区域 | "ap-guangzhou" |

**常用区域代码**：

| 区域 | 代码 |
|------|------|
| 广州 | ap-guangzhou |
| 上海 | ap-shanghai |
| 北京 | ap-beijing |
| 香港 | ap-hongkong |
| 新加坡 | ap-singapore |

**判断配置是否有效**：

```csharp
bool isValid = config.IsOK;
// 返回 true 当 AppID、SecretID、SecretKey 均不为空
```

**获取存储桶名称**：

```csharp
string bucketName = config.GetBucket("my-bucket");
// 返回 "my-bucket-{AppID}"
```

## 服务注册

```csharp
using Materal.Utils.CloudStorage.Tencent;

builder.Services.AddTencentCloudStorage();
```

## 基本操作

### 注入服务

```csharp
using Materal.Utils.CloudStorage.Tencent;

public class FileService(TencentCloudStorageService storageService)
{
    private readonly TencentCloudStorageService _storageService = storageService;
}
```

### 检查对象是否存在

```csharp
using Materal.Utils.CloudStorage.Tencent;

bool exist = _storageService.ObjectExist("file.txt");
// 使用默认存储桶

bool exist = _storageService.ObjectExist("images/avatar.png", "my-bucket");
// 指定存储桶
```

### 获取对象访问URL

```csharp
using Materal.Utils.CloudStorage.Tencent;

string url = _storageService.GetObjectUrl("file.txt");
// 返回对象的访问地址
```

## 文件上传

### 简单上传

```csharp
using Materal.Utils.CloudStorage.Tencent;

string key = await _storageService.UploadObjectAsync("C:\\uploads\\file.txt");
// 使用文件名作为 key，上传到默认存储桶

string key = await _storageService.UploadObjectAsync("C:\\uploads\\image.png", "images");
// 指定存储桶名称（不含 AppID）
```

### 指定Key上传

```csharp
using Materal.Utils.CloudStorage.Tencent;

await _storageService.UploadObjectByKeyAsync(
    "C:\\uploads\\file.txt",
    "documents/manual.pdf",
    "my-bucket"
);
```

## 文件下载

### 下载到指定路径

```csharp
using Materal.Utils.CloudStorage.Tencent;

await _storageService.DownloadObjectAsync(
    "C:\\downloads\\file.txt",
    "documents/manual.pdf"
);
```

### 下载到目录

```csharp
using Materal.Utils.CloudStorage.Tencent;

await _storageService.DownloadObjectAsync(
    "C:\\downloads",     // 目录路径
    "file.txt",          // 保存的文件名
    "documents/manual.pdf",
    "my-bucket"
);
```

## 文件删除

```csharp
using Materal.Utils.CloudStorage.Tencent;

_storageService.DeleteObject("file.txt", "my-bucket");
// 从指定存储桶删除对象
```

## 临时密钥获取

用于前端直传 COS 时获取临时访问凭证。

```csharp
using Materal.Utils.CloudStorage.Tencent;
using Materal.Utils.CloudStorage.Tencent.Models;

TemporaryKey tempKey = _storageService.GetTemporaryKey(
    new[] { "name/cos:PutObject", "name/cos:GetObject" }, // 允许的操作
    "images/*",                                         // 允许的路径前缀
    600,                                                // 有效期（秒）
    "my-bucket",                                        // 存储桶名称
    "ap-guangzhou"                                      // 区域
);
```

**allowActions 常用值**：

| 操作 | 说明 |
|------|------|
| `name/cos:PutObject` | 上传文件 |
| `name/cos:PostObject` | 表单上传 |
| `name/cos:GetObject` | 下载文件 |
| `name/cos:DeleteObject` | 删除文件 |
| `name/cos:HeadObject` | 查询对象元数据 |
| `name/cos:ListParts` | 查询分片上传已上传部分 |
| `name/cos:ListMultipartUploads` | 查询正在进行中的分片上传 |

**TemporaryKey 模型**：

```csharp
namespace Materal.Utils.CloudStorage.Tencent.Models
{
    public class TemporaryKey
    {
        public string SecretID { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public long ExpirationTime { get; set; }
        public long StartTime { get; set; }
    }
}
```

## 事件管理器

通过实现 `ITencentCloudStorageEventManager` 接口，可以监听上传下载进度和结果。

```csharp
using Materal.Utils.CloudStorage.Tencent;
using COSXML.Model;
using COSXML.Transfer;

public class StorageEventManager : ITencentCloudStorageEventManager
{
    public void SetTransferConfig(TransferConfig transferConfig)
    {
        // 配置传输参数
    }

    public void UploadObjectProgress(long completed, long total)
    {
        double progress = (double)completed / total * 100;
        Console.WriteLine($"上传进度: {progress:F2}%");
    }

    public void UploadObjectSuccess(CosResult result)
    {
        Console.WriteLine("上传成功");
    }

    public void UploadObjectFail(CosClientException clientException, CosServerException serverException)
    {
        Console.WriteLine($"上传失败: {serverException?.Message}");
    }

    public void DownloadObjectProgress(long completed, long total)
    {
        double progress = (double)completed / total * 100;
        Console.WriteLine($"下载进度: {progress:F2}%");
    }

    public void DownloadObjectSuccess(CosResult result)
    {
        Console.WriteLine("下载成功");
    }

    public void DownloadObjectFail(CosClientException clientException, CosServerException serverException)
    {
        Console.WriteLine($"下载失败: {serverException?.Message}");
    }

    public string GetFileKey(string filePath)
    {
        // 自定义文件 key 生成逻辑
        string fileName = Path.GetFileName(filePath);
        return $"uploads/{DateTime.Now:yyyyMMdd}/{fileName}";
    }
}
```

## 异常处理

所有操作可能抛出 `TencentCloudStorageException` 异常。

```csharp
using Materal.Utils.CloudStorage.Tencent;

try
{
    await _storageService.UploadObjectAsync("file.txt");
}
catch (TencentCloudStorageException ex)
{
    Console.WriteLine($"存储操作失败: {ex.Message}");
}
```

## 完整示例

### 文件上传服务

```csharp
using Materal.Utils.CloudStorage.Tencent;

namespace Example
{
    public class FileUploadService(
        TencentCloudStorageService storageService,
        ILogger<FileUploadService> logger)
    {
        private readonly TencentCloudStorageService _storageService = storageService;
        private readonly ILogger<FileUploadService> _logger = logger;

        /// <summary>
        /// 上传用户头像
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="userID">用户ID</param>
        /// <returns>头像访问URL</returns>
        public async Task<string> UploadAvatarAsync(string filePath, Guid userID)
        {
            if (string.IsNullOrEmpty(filePath)) throw new {ProjectName}Exception("文件路径不能为空");

            string extension = Path.GetExtension(filePath);
            string key = $"avatars/{userID}{extension}";

            await _storageService.UploadObjectByKeyAsync(filePath, key, "avatars");
            _logger.LogInformation("用户头像上传成功: {Key}", key);

            return _storageService.GetObjectUrl(key, "avatars");
        }

        /// <summary>
        /// 上传文档
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文档key和访问URL</returns>
        public async Task<(string key, string url)> UploadDocumentAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new {ProjectName}Exception("文件路径不能为空");

            string fileName = Path.GetFileName(filePath);
            string dateFolder = DateTime.Now.ToString("yyyyMMdd");
            string key = $"documents/{dateFolder}/{fileName}";

            await _storageService.UploadObjectByKeyAsync(filePath, key, "documents");
            _logger.LogInformation("文档上传成功: {Key}", key);

            string url = _storageService.GetObjectUrl(key, "documents");
            return (key, url);
        }
    }
}
```

### 前端直传服务

```csharp
using Materal.Utils.CloudStorage.Tencent;
using Materal.Utils.CloudStorage.Tencent.Models;

namespace Example
{
    public class DirectUploadService(TencentCloudStorageService storageService)
    {
        private readonly TencentCloudStorageService _storageService = storageService;

        /// <summary>
        /// 获取前端直传凭证
        /// </summary>
        /// <param name="prefix">路径前缀</param>
        /// <returns>临时密钥</returns>
        public TemporaryKey GetUploadCredential(string prefix)
        {
            return _storageService.GetTemporaryKey(
                new[] { "name/cos:PutObject" },
                $"{prefix}/*",
                3600, // 1小时有效
                "uploads"
            );
        }
    }
}
```

### 批量删除服务

```csharp
using Materal.Utils.CloudStorage.Tencent;

namespace Example
{
    public class FileCleanupService(TencentCloudStorageService storageService, ILogger<FileCleanupService> logger)
    {
        private readonly TencentCloudStorageService _storageService = storageService;
        private readonly ILogger<FileCleanupService> _logger = logger;

        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="keys">文件key列表</param>
        /// <param name="bucketName">存储桶名称</param>
        /// <returns>成功删除的数量</returns>
        public async Task<int> BatchDeleteAsync(IEnumerable<string> keys, string? bucketName = null)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            int successCount = 0;
            foreach (string key in keys)
            {
                try
                {
                    _storageService.DeleteObject(key, bucketName);
                    successCount++;
                }
                catch (TencentCloudStorageException ex)
                {
                    // 记录失败日志，继续处理其他文件
                    _logger.LogWarning(ex, "删除文件失败: {Key}", key);
                }
            }
            return successCount;
        }
    }
}
```

## 最佳实践

1. **存储桶命名**：使用有意义的名称，如 `images`、`documents`、`avatars`
2. **文件路径组织**：按日期或业务类型组织文件结构
3. **临时密钥**：前端直传时使用临时密钥，避免暴露主密钥
4. **异常处理**：对存储操作添加适当的异常处理
5. **进度监控**：大文件上传建议使用事件管理器监控进度
