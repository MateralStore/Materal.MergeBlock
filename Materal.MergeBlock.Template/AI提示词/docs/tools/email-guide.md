# 邮件工具使用指南

## 概述

`Materal.Utils.Email` 是一个邮件发送工具库，基于 .NET 的 `System.Net.Mail` 实现，支持单发邮件、多收件人、HTML 格式和附件发送。

## 安装

```bash
dotnet add package Materal.Utils.Email
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

### 依赖包

- `Materal.Utils`（基础工具包）

## 邮件配置

### EmailConfig - 邮件发送配置

```csharp
namespace Materal.Utils.Email;

public sealed class EmailConfig
{
    /// <summary>
    /// SMTP服务器地址
    /// </summary>
    public string SmtpHost { get; set; } = "smtp.qq.com";

    /// <summary>
    /// SMTP端口
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// 发送方邮箱
    /// </summary>
    public string FromAddress { get; set; } = "";

    /// <summary>
    /// 授权码（不是密码）
    /// </summary>
    public string AuthorizationCode { get; set; } = "";

    /// <summary>
    /// 是否启用SSL
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// 发送者显示名称
    /// </summary>
    public string DisplayName { get; set; } = "测试邮件系统";

    /// <summary>
    /// 验证配置是否完整
    /// </summary>
    public bool IsValid() => !string.IsNullOrEmpty(FromAddress)
        && !string.IsNullOrEmpty(AuthorizationCode)
        && !string.IsNullOrEmpty(SmtpHost);
}
```

### 配置示例

```csharp
using Materal.Utils.Email;

EmailConfig emailConfig = new()
{
    SmtpHost = "smtp.qq.com",
    SmtpPort = 587,
    FromAddress = "your-email@qq.com",
    AuthorizationCode = "your-authorization-code",
    EnableSsl = true,
    DisplayName = "我的应用"
};
```

> **注意**：授权码不是邮箱密码，需要在邮箱设置中单独获取。以 QQ 邮箱为例，请在「设置」→「账户」中开启 SMTP 服务并获取授权码。

## 依赖注入

### 注册服务

```csharp
using Microsoft.Extensions.DependencyInjection;
using Materal.Utils.Email;

services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));
services.AddMateralEmailUtils();
```

> **注意**：`EmailConfig` 需要通过 `services.Configure<EmailConfig>()` 从配置中加载，或者使用 `services.AddSingleton()` 手动注册。

### 使用构造注入

```csharp
namespace Example;

public class MyService
{
    private readonly IEmailService _emailService;

    public MyService(IEmailService emailService)
    {
        _emailService = emailService;
    }
}
```

## 发送邮件

### IEmailService 接口

```csharp
using System.Net.Mail;

namespace Materal.Utils.Email;

public interface IEmailService
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to">收件人邮箱</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">邮件内容</param>
    /// <param name="isHtml">是否HTML格式</param>
    /// <param name="attachments">附件列表</param>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false, IEnumerable<Attachment>? attachments = null);

    /// <summary>
    /// 发送邮件给多个收件人
    /// </summary>
    /// <param name="tos">收件人邮箱列表</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">邮件内容</param>
    /// <param name="isHtml">是否HTML格式</param>
    /// <param name="attachments">附件列表</param>
    Task SendEmailAsync(IEnumerable<string> tos, string subject, string body, bool isHtml = false, IEnumerable<Attachment>? attachments = null);
}
```

### 发送简单文本邮件

```csharp
using Materal.Utils.Email;

await emailService.SendEmailAsync(
    to: "recipient@example.com",
    subject: "测试邮件",
    body: "这是一封测试邮件"
);
```

### 发送 HTML 格式邮件

```csharp
using Materal.Utils.Email;

string htmlBody = @"
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .title { color: #333; font-size: 24px; }
        .content { color: #666; font-size: 16px; }
    </style>
</head>
<body>
    <h1 class=""title"">欢迎使用邮件服务</h1>
    <p class=""content"">这是一封 HTML 格式的邮件。</p>
</body>
</html>";

await emailService.SendEmailAsync(
    to: "recipient@example.com",
    subject: "HTML 邮件测试",
    body: htmlBody,
    isHtml: true
);
```

### 发送多收件人邮件

```csharp
using Materal.Utils.Email;

List<string> recipients = new()
{
    "user1@example.com",
    "user2@example.com",
    "user3@example.com"
};

await emailService.SendEmailAsync(
    tos: recipients,
    subject: "群发邮件",
    body: "这是一封群发邮件"
);
```

### 发送带附件的邮件

```csharp
using System.Net.Mail;
using Materal.Utils.Email;

// 从文件创建附件
using Attachment attachment = new Attachment("path/to/file.pdf");

// 从字节数组创建附件
byte[] fileData = File.ReadAllBytes("path/to/file.xlsx");
using MemoryStream memoryStream = new MemoryStream(fileData);
using Attachment attachmentFromBytes = new Attachment(memoryStream, "data.xlsx");

// 从流创建附件
using MemoryStream stream = new MemoryStream(fileData);
using Attachment attachmentFromStream = new Attachment(stream, "report.pdf");

List<Attachment> attachments = new()
{
    attachmentFromBytes,
    attachmentFromStream
};

await emailService.SendEmailAsync(
    to: "recipient@example.com",
    subject: "带附件的邮件",
    body: "请查收附件",
    attachments: attachments
);
```

### 完整 HTML 邮件示例

```csharp
using System.Net.Mail;
using Materal.Utils.Email;

namespace Example;

public class EmailServiceWrapper(IEmailService emailService)
{
    private readonly IEmailService _emailService = emailService ?? throw new {ProjectName}Exception("邮件服务不能为空");

    /// <summary>
    /// 发送验证码邮件
    /// </summary>
    public async Task SendVerificationCodeAsync(string to, string code)
    {
        string htmlBody = $@"
<html>
<body style=""margin: 0; padding: 20px; font-family: Arial, sans-serif;"">
    <div style=""max-width: 600px; margin: 0 auto; border: 1px solid #ddd; border-radius: 8px;"">
        <div style=""background: #007bff; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;"">
            <h1 style=""margin: 0;"">验证码</h1>
        </div>
        <div style=""padding: 30px; text-align: center;"">
            <p style=""font-size: 16px; color: #333;"">您的验证码是：</p>
            <div style=""font-size: 32px; font-weight: bold; color: #007bff; letter-spacing: 8px; margin: 20px 0;"">
                {code}
            </div>
            <p style=""font-size: 14px; color: #999;"">验证码有效期为 10 分钟，请勿泄露给他人。</p>
        </div>
    </div>
</body>
</html>";

        await _emailService.SendEmailAsync(
            to: to,
            subject: "您的验证码",
            body: htmlBody,
            isHtml: true
        );
    }

    /// <summary>
    /// 发送带报告附件的邮件
    /// </summary>
    /// <param name="to">收件人邮箱</param>
    /// <param name="reportTitle">报告标题</param>
    /// <param name="reportData">报告数据</param>
    public async Task SendReportEmailAsync(string to, string reportTitle, byte[] reportData)
    {
        if (string.IsNullOrEmpty(to)) throw new ArgumentException("收件人邮箱不能为空", nameof(to));
        if (reportData == null || reportData.Length == 0) throw new ArgumentException("报告数据不能为空", nameof(reportData));

        using MemoryStream reportStream = new MemoryStream(reportData);
        using Attachment reportAttachment = new Attachment(reportStream, $"{reportTitle}.xlsx");

        await _emailService.SendEmailAsync(
            to: to,
            subject: reportTitle,
            body: $"您好，{reportTitle} 已生成，请查收附件。",
            attachments: new[] { reportAttachment }
        );
    }

    /// <summary>
    /// 发送系统通知邮件给多个管理员
    /// </summary>
    public async Task SendSystemNotificationAsync(IEnumerable<string> admins, string message)
    {
        await _emailService.SendEmailAsync(
            tos: admins,
            subject: "系统通知",
            body: $"<p>{message}</p><p style=""color: #999; font-size: 12px;"">此邮件由系统自动发送，请勿回复。</p>",
            isHtml: true
        );
    }
}
```

## 常见问题

### SMTP 服务器配置

| 邮箱服务商 | SMTP 服务器 | 端口 | SSL |
|------------|-------------|------|-----|
| QQ 邮箱 | smtp.qq.com | 587 | 是 |
| 163 邮箱 | smtp.163.com | 465/994 | 是 |
| Gmail | smtp.gmail.com | 587 | 是 |
| Outlook | smtp.office365.com | 587 | 是 |

### 错误处理

```csharp
using Materal.Utils.Email;

try
{
    await emailService.SendEmailAsync(to, subject, body);
}
catch (InvalidOperationException ex)
{
    // 配置不完整或发送失败，记录日志
    Console.WriteLine($"发送失败: {ex.Message}");
}
```

### 附件清理

`Attachment` 对象在使用完毕后会自动释放，推荐使用 `using` 语句：

```csharp
using System.Net.Mail;

using Attachment attachment = new Attachment("file.pdf");
await emailService.SendEmailAsync(to, subject, body, attachments: new[] { attachment });
```

## 配置验证

在发送邮件前，可以先验证配置是否有效：

```csharp
using Materal.Utils.Email;

EmailConfig config = new()
{
    FromAddress = "test@example.com",
    AuthorizationCode = "code",
    SmtpHost = "smtp.example.com"
};

if (!config.IsValid())
{
    Console.WriteLine("邮件配置不完整");
    return;
}

IEmailService emailService = new EmailService(config);
```
