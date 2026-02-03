# 条码工具使用指南

## 概述

`Materal.Utils.BarCode` 是一个基于 SkiaSharp 和 ZXing 的条码/二维码处理工具库，支持二维码的生成、读取、样式修改和 Logo 添加等功能。

## 安装

```bash
dotnet add package Materal.Utils.BarCode
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

## QRCodeHelper - 二维码操作

### 创建二维码

```csharp
using Materal.Utils.BarCode;
using SkiaSharp;

// 方式1：创建正方形二维码（默认300x300）
SKBitmap qrCode = QRCodeHelper.CreateQRCode("https://example.com");

// 方式2：创建指定尺寸的二维码
SKBitmap qrCode = QRCodeHelper.CreateQRCode("https://example.com", 500);

// 方式3：创建指定宽高的二维码（参数顺序：高度, 宽度）
SKBitmap qrCode = QRCodeHelper.CreateQRCode("https://example.com", 300, 400);

// 方式4：使用自定义编码选项
using ZXing.Common.EncodingOptions options = new EncodingOptions
{
    Width = 300,
    Height = 300,
    Margin = 2 // 边框空白
};
SKBitmap qrCode = QRCodeHelper.CreateQRCode("https://example.com", options);
```

### 读取二维码

```csharp
using Materal.Utils.BarCode;

// 从文件读取
using SKBitmap bitmap = SKBitmap.Decode("qrcode.png");
string content = QRCodeHelper.ReadQRCode(bitmap);
// content = "https://example.com"
```

### 添加 Logo

```csharp
using Materal.Utils.BarCode;
using SkiaSharp;

SKBitmap qrCode = QRCodeHelper.CreateQRCode("https://example.com");
using SKBitmap logo = SKBitmap.Decode("logo.png");

// 添加Logo（二维码中心区域）
SKBitmap result = QRCodeHelper.AddLogo(qrCode, logo, 80);
// Logo大小为80x80像素
```

### 自定义二维码样式

```csharp
using Materal.Utils.BarCode;
using SkiaSharp;

SKBitmap qrCode = QRCodeHelper.CreateQRCode("https://example.com");

// 自定义二维码点样式
SKBitmap styledQRCode = QRCodeHelper.ChangeQRCodeImage(
    qrCode,
    // 普通点的绘制方式：绘制圆形
    (canvas, paint, center, size) =>
    {
        canvas.DrawCircle(center.X, center.Y, size.Width / 2f, paint);
    },
    // 定位角的绘制方式：保持方形
    (paint, center) =>
    {
        paint.Color = SKColors.Black;
    },
    // 背景色
    background: SKColors.White
);
```

### 保存二维码图片

```csharp
using Materal.Utils.BarCode;
using SkiaSharp;

SKBitmap qrCode = QRCodeHelper.CreateQRCode("https://example.com");

// 使用扩展方法保存
qrCode.SaveAs("qrcode.png");                    // 保存为PNG
qrCode.SaveAs("qrcode.jpg", SKEncodedImageFormat.Jpeg); // 保存为JPG

// 或使用FileInfo
FileInfo fileInfo = new FileInfo("qrcode.png");
qrCode.SaveAs(fileInfo);
```

## BarCodeHelper - 通用条码操作

### 读取条码

```csharp
using Materal.Utils.BarCode;
using SkiaSharp;
using ZXing;

using SKBitmap bitmap = SKBitmap.Decode("barcode.png");

// 读取条码，返回内容及条码格式
string content = BarCodeHelper.ReadBarCode(bitmap, out BarcodeFormat format);

// format 可能的值：
// - BarcodeFormat.QR_CODE
// - BarcodeFormat.CODE_128
// - BarcodeFormat.EAN_13
// - BarcodeFormat.DATA_MATRIX
// - 等等...
```

## 完整示例

```csharp
using Materal.Utils.BarCode;
using SkiaSharp;

namespace Example;

public class QRCodeService
{
    /// <summary>
    /// 生成带Logo的二维码并保存
    /// </summary>
    /// <param name="content">二维码内容</param>
    /// <param name="logoPath">Logo图片路径</param>
    /// <param name="outputPath">输出图片路径</param>
    public async Task GenerateQRCodeAsync(string content, string logoPath, string outputPath)
    {
        // 生成二维码
        SKBitmap qrCode = QRCodeHelper.CreateQRCode(content, 300);

        // 加载Logo
        using SKBitmap logo = SKBitmap.Decode(logoPath);

        // 添加Logo
        SKBitmap result = QRCodeHelper.AddLogo(qrCode, logo, 60);

        // 保存结果
        result.SaveAs(outputPath);
    }

    /// <summary>
    /// 从图片中解析二维码内容
    /// </summary>
    /// <param name="imagePath">图片路径</param>
    /// <returns>二维码内容</returns>
    public string DecodeQRCode(string imagePath)
    {
        using SKBitmap bitmap = SKBitmap.Decode(imagePath);
        return QRCodeHelper.ReadQRCode(bitmap);
    }
}
```

## 注意事项

1. **依赖项**：库依赖 `SkiaSharp` 和 `ZXing.Net.Bindings.SkiaSharp`，确保项目已正确引用。

2. **图片格式**：使用 `SKEncodedImageFormat` 枚举，支持 Png、Jpeg、Gif、Bmp、WebP 等格式。

3. **异常处理**：
   - `UtilException`：读取失败时抛出（如图片不是二维码或无法识别）
   - `ArgumentException`：输入参数为空或无效时抛出

4. **性能考虑**：批量生成二维码时，建议复用 SKBitmap 对象以减少内存开销。
