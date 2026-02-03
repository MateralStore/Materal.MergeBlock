# Excel 工具使用指南

## 概述

`Materal.Utils.Excel` 是一个 Excel 操作工具库，基于 NPOI 实现，支持 `.xlsx` 和 `.xls` 格式文件的读取与写入。

## 安装

```bash
dotnet add package Materal.Utils.Excel
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

### 依赖包

- `NPOI`（Excel 操作库）
- `Materal.Utils`（基础工具包）

## ExcelHelper - Excel 读取

`ExcelHelper` 提供读取 Excel 文件到工作簿的功能，是静态帮助类。

### 从文件路径读取

```csharp
using Materal.Utils.Excel;

IWorkbook workbook = ExcelHelper.ReadExcelToWorkbook("data.xlsx");
```

### 从文件流读取

```csharp
using Materal.Utils.Excel;

using FileStream fs = new("data.xlsx", FileMode.Open, FileAccess.Read);
IWorkbook workbook = ExcelHelper.ReadExcelToWorkbook(fs);
```

> **注意**：支持 `.xlsx` 和 `.xls` 格式，自动根据文件扩展名判断类型。

> **注意**：`ExcelHelper` 是静态类，无需注入，直接通过 `ExcelHelper.xxx()` 调用。

## IWorkbookExtension - 工作簿扩展

`IWorkbookExtension` 为 `IWorkbook` 提供扩展方法，支持将工作簿保存为文件。

### 保存工作簿

```csharp
using Materal.Utils.Excel;
using NPOI.SS.UserModel;

IWorkbook workbook = ExcelHelper.ReadExcelToWorkbook("data.xlsx");
// 对工作簿进行操作...
workbook.SaveAs("output.xlsx");
```

## 完整示例

### 读取并保存 Excel

```csharp
using Materal.Utils.Excel;

namespace Example
{
    public class ExcelService
    {
        /// <summary>
        /// 复制 Excel 文件
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="targetPath">目标文件路径</param>
        public void CopyExcel(string sourcePath, string targetPath)
        {
            if (string.IsNullOrEmpty(sourcePath)) throw new {ProjectName}Exception("源文件路径不能为空");
            if (string.IsNullOrEmpty(targetPath)) throw new {ProjectName}Exception("目标文件路径不能为空");

            using IWorkbook workbook = ExcelHelper.ReadExcelToWorkbook(sourcePath);
            workbook.SaveAs(targetPath);
        }
    }
}
```

### 读取并处理 Excel 数据

```csharp
using Materal.Utils.Excel;
using NPOI.SS.UserModel;

namespace Example
{
    public class ExcelDataService
    {
        /// <summary>
        /// 读取 Excel 第一个工作表数据
        /// </summary>
        /// <param name="filePath">Excel 文件路径</param>
        /// <returns>所有单元格数据列表</returns>
        public List<string> ReadFirstSheetData(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new {ProjectName}Exception("文件路径不能为空");

            using IWorkbook workbook = ExcelHelper.ReadExcelToWorkbook(filePath);
            ISheet? sheet = workbook.GetSheetAt(0);
            var result = new List<string>();

            if (sheet != null)
            {
                foreach (IRow row in sheet)
                {
                    foreach (ICell cell in row)
                    {
                        result.Add(cell.ToString() ?? string.Empty);
                    }
                }
            }

            return result;
        }
    }
}
```

## 注意事项

| 项目 | 说明 |
|------|------|
| 文件格式 | 支持 `.xlsx`（Excel 2007+）和 `.xls`（Excel 97-2003） |
| 资源释放 | 使用 `using` 语句确保 `IWorkbook` 正确释放，避免资源泄漏 |
| 空工作表 | `GetSheetAt(0)` 返回 `ISheet?`，需进行空值判断 |
