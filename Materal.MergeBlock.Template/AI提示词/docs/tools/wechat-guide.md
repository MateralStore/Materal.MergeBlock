# 微信工具使用指南

## 概述

`Materal.Utils.Wechat` 是一个微信工具库，提供微信公众号、小程序的消息处理、AccessToken 获取、模板消息发送等功能。

## 安装

```bash
dotnet add package Materal.Utils.Wechat
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

### 依赖包

- `LitJson` - JSON 序列化
- `Materal.Utils.Network` - 网络请求
- `Materal.Utils.Validation` - 数据验证

## 配置模型

### WechatConfigModel - 微信配置

```csharp
using Materal.Utils.Wechat.Model;

var config = new WechatConfigModel
{
    WechatAPIUrl = "https://api.weixin.qq.com/",
    APPID = "your_appid",
    APPSECRET = "your_appsecret"
};
```

| 属性 | 类型 | 说明 |
|------|------|------|
| WechatAPIUrl | string | 微信 API 域名，默认 `https://api.weixin.qq.com/` |
| APPID | string | 绑定 APPID（必须配置） |
| APPSECRET | string | 公众帐号密钥（必须配置） |

## 基础帮助类

### WechatHelper - 微信帮助类基类

提供 AccessToken 获取和 HTTP 结果处理功能：

```csharp
using Materal.Utils.Wechat;

var config = new WechatConfigModel
{
    APPID = "your_appid",
    APPSECRET = "your_appsecret"
};
WechatOfficialAccountHelper helper = new(config);

// 获取 AccessToken
AccessTokenResultModel token = await helper.GetAccessTokenAsync();
Console.WriteLine(token.AccessToken);
Console.WriteLine(token.ExpiresIn);
```

## 微信公众号

### WechatOfficialAccountHelper - 公众号帮助类

```csharp
using Materal.Utils.Wechat;

var config = new WechatConfigModel
{
    APPID = "your_appid",
    APPSECRET = "your_appsecret"
};
WechatOfficialAccountHelper helper = new(config);
```

**获取网页 AccessToken：**

```csharp
WebAccessTokenResultModel result = await helper.GetWebAccessTokenByCodeAsync("authorization_code");
```

**发送模板消息：**

```csharp
await helper.SendTemplateMessageAsync(new SendTemplateMessageRequestModel
{
    AccessToken = "access_token",
    UserOpenID = "openid",
    TemplateID = "template_id",
    Url = "https://example.com",
    TemplateDatas =
    [
        new KeyValueModel { Key = "first", Value = "您好，您有新的消息" },
        new KeyValueModel { Key = "keyword1", Value = "订单编号" },
        new KeyValueModel { Key = "keyword2", Value = "2024-01-01" }
    ]
});
```

### WechatOfficialAccountServerHelper - 公众号服务端帮助类

用于处理微信服务器推送的消息和事件：

```csharp
using Materal.Utils.Wechat;

var serverHelper = new WechatOfficialAccountServerHelper("your_token", serviceProvider);

// 验证签名
bool isValid = serverHelper.IsWechatRequest(timestamp, nonce, signature);

// 获取签名
string signature = serverHelper.GetSignature(timestamp, nonce);

// 处理微信事件
ReplyMessageModel? result = await serverHelper.HandlerWechatEventAsync(xmlDocument);
```

## 微信小程序

### WechatMiniProgramHelper - 小程序帮助类

```csharp
using Materal.Utils.Wechat;

var config = new WechatConfigModel
{
    APPID = "your_appid",
    APPSECRET = "your_appsecret"
};
WechatMiniProgramHelper helper = new(config);
```

**根据 Code 获取 OpenID：**

```csharp
string openID = await helper.GetOpenIDByCodeAsync("js_code");
```

**发送订阅消息：**

```csharp
await helper.SubscribeMessageSendAsync(new SubscribeMessageSendRequestModel
{
    AccessToken = "access_token",
    OpenID = "openid",
    TemplateID = "template_id",
    GoToPage = "pages/index/index",
    MiniprogramState = "formal",
    Language = "zh_CN",
    TemplateData = new Dictionary<string, string>
    {
        ["thing1"] = "您好，有新消息",
        ["time2"] = "2024-01-01 12:00"
    }
});
```

## 消息处理

### IEventHandler<TEvent> - 事件处理器接口

所有消息和事件处理器都继承自此接口：

```csharp
using Materal.Utils.Wechat.ServerEventHandler;

public interface IEventHandler<TEvent>
{
    Task<ReplyMessageModel?> HandlerAsync(TEvent @event);
}
```

### 消息事件处理器

| 接口 | 说明 |
|------|------|
| `ITextMessageEventHandler` | 文本消息处理器 |
| `IImageMessageEventHandler` | 图片消息处理器 |
| `IVoiceMessageEventHandler` | 语音消息处理器 |
| `IVideoMessageEventHandler` | 视频消息处理器 |
| `IShortVideoMessageEventHandler` | 小视频消息处理器 |
| `ILocationMessageEventHandler` | 地理位置消息处理器 |
| `ILinkMessageEventHandler` | 链接消息处理器 |

### 事件处理器

| 接口 | 说明 |
|------|------|
| `ISubscribeEventHandler` | 用户关注事件处理器 |
| `IUnsubscribeEventHandler` | 用户取消关注事件处理器 |
| `ITemplateSendJobFinishEventHandler` | 模板消息发送完成事件处理器 |

## 回复消息模型

### ReplyMessageModel - 回复消息基类

```csharp
using Materal.Utils.Wechat.Model;

// 回复文本消息
ReplyTextMessageModel reply = new(toUserName, fromUserName, "收到消息");
string xml = reply.GetXmlString();
```

| 属性 | 说明 |
|------|------|
| ToUserName | 开发者微信号 |
| FromUserName | 订阅用户的 OpenID |
| CreateTime | 消息创建时间 |
| MessageType | 消息类型 |

## 使用示例

### 完整的微信公众号消息处理示例

```csharp
using Materal.Utils.Wechat;
using Materal.Utils.Wechat.ServerEventHandler;
using Materal.Utils.Wechat.Model;

namespace Example
{
    /// <summary>
    /// 文本消息处理器
    /// </summary>
    public class TextMessageHandler : ITextMessageEventHandler
    {
        public Task<ReplyMessageModel?> HandlerAsync(TextMessageEvent @event)
        {
            string response = $"收到消息：{@event.Content}";
            var reply = new ReplyTextMessageModel(
                @event.ToUserName,
                @event.FromUserName,
                response
            );
            return Task.FromResult<ReplyMessageModel?>(reply);
        }
    }

    /// <summary>
    /// 关注事件处理器
    /// </summary>
    public class SubscribeHandler : ISubscribeEventHandler
    {
        public Task<ReplyMessageModel?> HandlerAsync(SubscribeEvent @event)
        {
            var reply = new ReplyTextMessageModel(
                @event.ToUserName,
                @event.FromUserName,
                "感谢关注！"
            );
            return Task.FromResult<ReplyMessageModel?>(reply);
        }
    }
}
```

### 在 ASP.NET Core 中使用

```csharp
using Materal.Utils.Wechat;
using Microsoft.AspNetCore.Mvc;

namespace Example.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WechatController : ControllerBase
    {
        private const string Token = "your_token";
        private static readonly WechatOfficialAccountServerHelper _serverHelper = new(Token, serviceProvider);

        /// <summary>
        /// 微信服务器验证接口（GET）
        /// </summary>
        /// <param name="signature">签名</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="echostr">随机字符串</param>
        /// <returns>验证结果</returns>
        [HttpGet]
        public IActionResult Get(string signature, string timestamp, string nonce, string echostr)
        {
            if (_serverHelper.IsWechatRequest(timestamp, nonce, signature))
            {
                return Content(echostr);
            }
            return BadRequest();
        }

        /// <summary>
        /// 微信消息接收接口（POST）
        /// </summary>
        /// <param name="signature">签名</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <returns>处理结果</returns>
        [HttpPost]
        public async Task<IActionResult> Post(string signature, string timestamp, string nonce)
        {
            if (!ServerHelper.IsWechatRequest(timestamp, nonce, signature))
            {
                return BadRequest();
            }

            using var reader = new StreamReader(Request.Body);
            string xmlContent = await reader.ReadToEndAsync();

            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(xmlContent);

            ReplyMessageModel? reply = await ServerHelper.HandlerWechatEventAsync(xmlDocument);
            if (reply == null) return Ok();

            return Content(reply.GetXmlString(), "application/xml");
        }
    }
}
```

## 异常处理

### WechatException - 微信异常

```csharp
using Materal.Utils.Wechat.Model;

try
{
    // 微信操作
}
catch (WechatException ex)
{
    // 记录错误日志
    Console.WriteLine($"错误代码：{ex.ErrorCode}");
    Console.WriteLine($"错误信息：{ex.Message}");
}
```

| 属性 | 说明 |
|------|------|
| ErrorCode | 错误代码 |
| Message | 错误消息 |

## 注意事项

1. **AccessToken 缓存**：AccessToken 有效期为 7200 秒（2 小时），建议缓存使用，避免频繁请求导致接口受限。
2. **签名验证**：公众号服务器配置时需要验证签名，确保 Token 保密。
3. **消息格式**：微信服务器推送的是 XML 格式数据，已自动解析为事件对象。
4. **CDATA 处理**：回复消息内容会自动添加 CDATA 包裹，无需手动处理。
