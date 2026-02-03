# 图片工具使用指南

## 概述

`Materal.Utils.Image` 是一个基于 SkiaSharp 的图片处理工具库，支持图片压缩、缩略图生成、验证码生成等功能。

## 安装

```bash
dotnet add package Materal.Utils.Image
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

## ImageHelper - 图片操作

### 图片压缩

```csharp
using Materal.Utils.Image;
using SkiaSharp;

// 按比例压缩（默认50%）
SKBitmap compressed = ImageHelper.Compress("original.jpg");

// 按指定比例压缩
SKBitmap compressed = ImageHelper.Compress("original.jpg", 70); // 70%

// 按指定尺寸压缩
SKBitmap compressed = ImageHelper.Compress("original.jpg", 800, 600);

// 指定输出格式和质量
SKBitmap compressed = ImageHelper.Compress("original.jpg", 800, 600, SKEncodedImageFormat.Jpeg, 80);
```

### 缩略图生成

```csharp
using Materal.Utils.Image;
using SkiaSharp;

// 按比例生成缩略图（默认50%）
SKImage thumbnail = ImageHelper.GetThumbnailImage("original.jpg");

// 按指定比例生成
SKImage thumbnail = ImageHelper.GetThumbnailImage("original.jpg", 30); // 30%

// 按指定尺寸生成
SKImage thumbnail = ImageHelper.GetThumbnailImage("original.jpg", 200, 200);
```

### 保存图片

```csharp
using Materal.Utils.Image;
using SkiaSharp;

// 从文件保存
ImageHelper.SaveAs("original.jpg", "output.jpg", 80);

// 指定格式保存
ImageHelper.SaveAs("original.jpg", "output.png", SKEncodedImageFormat.Png, 100);

// SKBitmap 保存到文件
using SKBitmap bitmap = SKBitmap.Decode("original.jpg");
bitmap.SaveAs("output.jpg"); // 默认保持原格式
bitmap.SaveAs("output.png", SKEncodedImageFormat.Png);

// SKBitmap 保存到流
MemoryStream stream = new();
bitmap.SaveAs(stream, SKEncodedImageFormat.Png);
```

### 获取图片格式

```csharp
using Materal.Utils.Image;

// 根据文件扩展名获取图片格式
SKEncodedImageFormat format = ImageHelper.GetImageFormatFromFile("image.jpg"); // Jpeg
SKEncodedImageFormat format = ImageHelper.GetImageFormatFromFile("image.png"); // Png

// 支持的格式：bmp, gif, ico, jpg/jpeg, png, wbmp, webp, pkm, ktx, astc, dng, heif, avif
```

### 获取 Base64 图片

```csharp
using Materal.Utils.Image;
using SkiaSharp;

using SKImage image = SKImage.FromEncodedFile("image.png");

// 获取 Base64 字符串（默认 PNG 格式）
string base64 = image.GetBase64Image();

// 指定格式
string base64 = image.GetBase64Image(SKEncodedImageFormat.Jpeg);
```

## CaptchaHelper - 验证码生成

> **注意**：验证码功能仅在 .NET 8.0 及以上版本可用（使用 `#if NET` 条件编译）

### 生成验证码图片

```csharp
using Materal.Utils.Image;
using SkiaSharp;

// 生成验证码图片（默认随机预设样式）
SKImage captchaImage = CaptchaHelper.Draw("ABC123", 200, 80);

// 保存到文件
CaptchaHelper.Draw("ABC123", "captcha.png", 200, 80);

// 获取流
Stream stream = CaptchaHelper.DrawToStream("ABC123", 200, 80);

// 获取 Base64
string base64 = CaptchaHelper.DrawToBase64("ABC123", 200, 80);
```

### 使用预设样式

```csharp
using Materal.Utils.Image;
using SkiaSharp;

// 获取所有预设样式
IReadOnlyList<CaptchaPresetOptions> presets = CaptchaHelper.Presets;

// 随机获取一个预设样式
CaptchaPresetOptions randomPreset = CaptchaHelper.GetRandomPreset();

// 使用预设样式生成验证码
CaptchaOptions options = randomPreset.ToOptions();
SKImage image = CaptchaHelper.Draw("ABC123", 200, 80, options);
```

### 自定义验证码样式

```csharp
using Materal.Utils.Image;
using SkiaSharp;

CaptchaOptions options = new()
{
    // 背景设置
    BackgroundColor = new SKColor(240, 248, 255), // 浅蓝背景
    GradientBackgroundStartColor = new SKColor(240, 248, 255),
    GradientBackgroundEndColor = new SKColor(230, 220, 255),
    GradientDirection = 1, // 0=水平, 1=垂直

    // 文字设置
    TextColor = SKColors.DarkBlue,
    FontSize = 40,
    FontFamily = "Microsoft YaHei",
    MaxCharRotation = 25, // 字符最大旋转角度
    MaxCharOffset = 5,    // 字符最大偏移量

    // 字符颜色变化
    EnableCharColorVariation = true,
    CharColorVariationRange = 50,

    // 文字阴影
    EnableTextShadow = true,
    ShadowColor = new SKColor(128, 128, 128, 128),
    ShadowOffsetX = 2,
    ShadowOffsetY = 2,

    // 干扰线
    EnableInterferenceLines = true,
    InterferenceLineColor = SKColors.LightSteelBlue,
    InterferenceLineWidth = 1,
    InterferenceLineCount = 3,

    // 曲线干扰线
    EnableCurvedLines = true,
    CurvedLineCount = 2,
    CurveAmplitude = 10,
    CurveFrequency = 2,

    // 渐变干扰线
    EnableGradientLines = true,
    GradientLineCount = 2,

    // 网格线
    EnableGridLines = false,
    GridLineColor = new SKColor(200, 200, 200, 50),
    GridSpacing = 20,

    // 噪点
    EnableNoisePoints = true,
    NoisePointColor = SKColors.LightSteelBlue,
    NoisePointCount = 100,

    // 特殊噪点（矩形、线条、弧线、十字）
    EnableSpecialNoise = true,
    SpecialNoiseCount = 20,

    // 波浪扭曲
    EnableWaveDistortion = true,
    WaveAmplitudeX = 3,
    WaveAmplitudeY = 5,
    WaveFrequencyX = 10,
    WaveFrequencyY = 5
};

SKImage captchaImage = CaptchaHelper.Draw("ABC123", 200, 80, options);
```

### 预设样式列表

| 名称 | 背景色 | 文字颜色 |
|------|--------|----------|
| 经典白底黑字 | 白色 | 黑色 |
| 浅蓝背景 | RGB(240,248,255) | 深蓝色 |
| 浅黄背景 | RGB(255,250,240) | 深红色 |
| 浅绿背景 | RGB(240,255,240) | 深绿色 |
| 浅粉背景 | RGB(255,240,245) | 深洋红色 |
| 渐变蓝紫 | RGB(240,248,255) → RGB(230,220,255) | 深石板蓝 |
| 渐变橙红 | RGB(255,250,240) → RGB(255,230,230) | 深红色 |
| 渐变青绿 | RGB(240,255,240) → RGB(220,255,250) | 青色 |

## 完整示例

```csharp
using Materal.Utils.Image;
using SkiaSharp;

namespace Example;

public class CaptchaService
{
    /// <summary>
    /// 生成验证码图片并返回 Base64
    /// </summary>
    public (string ImageBase64, string Code) GenerateCaptcha(int width = 200, int height = 80)
    {
        string code = GenerateRandomCode(6);
        string base64 = CaptchaHelper.DrawToBase64(code, width, height);
        return (base64, code);
    }

    /// <summary>
    /// 压缩图片并保存
    /// </summary>
    public void CompressAndSave(string inputPath, string outputPath, int maxWidth = 800)
    {
        using SKBitmap original = SKBitmap.Decode(inputPath);
        int height = (int)(original.Height * (double)maxWidth / original.Width);
        using SKBitmap compressed = original.Compress(maxWidth, height, SKEncodedImageFormat.Jpeg, 80);
        compressed.SaveAs(outputPath);
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Random random = new();
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
```

## 注意事项

1. **依赖项**：库依赖 `SkiaSharp`，确保项目已正确引用。

2. **图片格式**：`SKEncodedImageFormat` 枚举支持以下格式：
   - Bmp、Gif、Ico、Jpeg、Png、Wbmp、WebP
   - Pkm、Ktx、Astc、Dng、Heif、Avif

3. **资源释放**：
   - `SKBitmap`、`SKImage`、`SKData` 等对象使用后建议 using 释放
   - 或使用 `Dispose()` 方法手动释放

4. **验证码字体**：默认优先使用中文字体（微软雅黑、宋体、黑体等），回退到 Arial 字体。

5. **NET 条件编译**：验证码功能仅在 .NET 8.0 及以上版本可用。
