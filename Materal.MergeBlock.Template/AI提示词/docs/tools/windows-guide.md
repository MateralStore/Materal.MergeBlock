# Windows 系统工具使用指南

## 概述

`Materal.Utils.Windows` 是一个 Windows 系统工具库，提供进程管理、CMD 命令执行、资源管理器操作、注册表查询等 Windows 特有功能。

## 安装

```bash
dotnet add package Materal.Utils.Windows
```

### 依赖框架

- .NET Standard 2.0 / 2.1
- .NET 8.0 / 9.0 / 10.0

### 依赖包

- `Materal.Utils`（基础工具包）

## 进程管理

### ProcessHelper - 进程管理

提供进程启动和管理功能，支持普通启动和以当前用户身份启动。

**获取进程启动信息**：

```csharp
using Materal.Utils.Windows;

ProcessStartInfo startInfo = ProcessHelper.GetProcessStartInfo("notepad.exe", "test.txt");
```

**启动进程**：

```csharp
using Materal.Utils.Windows;

var _processHelper = new ProcessHelper();
_processHelper.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
_processHelper.ErrorDataReceived += (sender, args) => Console.WriteLine($"Error: {args.Data}");
_processHelper.ProcessStart("notepad.exe", string.Empty);
```

**以当前用户身份启动进程**（适用于服务场景）：

```csharp
using Materal.Utils.Windows;

ProcessHelper _processHelper = new();
_processHelper.ProcessStartAsCurrentUser("notepad.exe", string.Empty);
```

> **注意**：`ProcessStartAsCurrentUser` 使用 Windows API 在当前活动用户会话中启动进程，适用于 Windows 服务需要启动交互式应用的场景。

## CMD 命令执行

### CmdHelper - CMD 命令执行

在进程中执行 CMD 命令，支持多条命令顺序执行。

```csharp
using Materal.Utils.Windows;

var cmdHelper = new CmdHelper();
cmdHelper.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
cmdHelper.ErrorDataReceived += (sender, args) => Console.WriteLine($"Error: {args.Data}");

await cmdHelper.RunCmdCommandsAsync(
    "echo Hello World",
    "dir",
    "ipconfig"
);
```

## 资源管理器操作

### ExplorerHelper - 资源管理器操作

打开资源管理器并选中指定文件或文件夹。

```csharp
using Materal.Utils.Windows;

// 打开资源管理器并选中文件
Process process = ExplorerHelper.OpenExplorer(@"C:\Users\example\document.txt");

// 打开资源管理器并选中文件夹
Process folderProcess = ExplorerHelper.OpenExplorer(@"C:\Users\example\Documents");
```

## 注册表操作

### RegistryHelper - 注册表操作

在指定的注册表范围内查询键是否存在。

**检查注册表项是否存在（搜索所有根键）**：

```csharp
using Materal.Utils.Windows;
using Microsoft.Win32;

bool exists = RegistryHelper.AnyAll(@"Software\Microsoft\Windows");
```

**检查注册表项是否存在（指定搜索范围）**：

```csharp
using Materal.Utils.Windows;
using Microsoft.Win32;

bool exists = RegistryHelper.AnyAll(
    @"Software\MyApp",
    Registry.CurrentUser,
    Registry.LocalMachine
);
```

## 完整示例

### 进程启动并捕获输出

```csharp
using Materal.Utils.Windows;

namespace Example
{
    public class ProcessService
    {
        /// <summary>
        /// 执行命令并返回输出
        /// </summary>
        public async Task<(string output, string error)> ExecuteCommandAsync(string command)
        {
            List<string> outputs = [];
            List<string> errors = [];
            var cmdHelper = new CmdHelper();

            cmdHelper.OutputDataReceived += (sender, args) =>
            {
                if (args.Data is not null)
                {
                    outputs.Add(args.Data);
                }
            };
            cmdHelper.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data is not null)
                {
                    errors.Add(args.Data);
                }
            };

            await cmdHelper.RunCmdCommandsAsync(command);

            return (string.Join(Environment.NewLine, outputs), string.Join(Environment.NewLine, errors));
        }
    }
}
```

### 服务中启动用户进程

```csharp
using Materal.Utils.Windows;

namespace Example
{
    public class UserProcessService
    {
        /// <summary>
        /// 在当前用户会话中启动进程
        /// </summary>
        /// <param name="appPath">应用程序路径</param>
        /// <param name="arguments">启动参数</param>
        public void LaunchUserApplication(string appPath, string arguments = "")
        {
            var processHelper = new ProcessHelper();
            processHelper.ProcessStartAsCurrentUser(appPath, arguments);
        }
    }
}
```

### 检查应用是否已安装

```csharp
using Materal.Utils.Windows;
using Microsoft.Win32;

namespace Example
{
    public class SoftwareChecker
    {
        /// <summary>
        /// 检查指定程序是否已安装
        /// </summary>
        /// <param name="softwareName">软件名称</param>
        /// <returns>是否已安装</returns>
        public bool IsSoftwareInstalled(string softwareName)
        {
            // 在 HKEY_CURRENT_USER 和 HKEY_LOCAL_MACHINE 中查找
            return RegistryHelper.AnyAll($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{softwareName}",
                Registry.CurrentUser,
                Registry.LocalMachine);
        }
    }
}
```

## GlobalUsing 命名空间

使用 `Materal.Utils.Windows` 后，以下命名空间会自动导入：

- `Microsoft.Win32`
- `System.Diagnostics`

## 平台限制

本库仅支持 Windows 平台，在非 Windows 平台上使用会导致异常。
