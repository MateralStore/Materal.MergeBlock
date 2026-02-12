using System.Diagnostics;

namespace MateralMergeBlockVSIX.Helper;

internal static class CommandHelper
{
    /// <summary>
    /// 执行dotnet命令
    /// </summary>
    /// <param name="arguments">命令参数</param>
    public static void ExecuteDotnetCommand(string arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("无法启动dotnet进程");
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"dotnet命令执行失败: {error}");
        }
    }
    /// <summary>
    /// 执行dotnet命令
    /// </summary>
    /// <param name="arguments">命令参数</param>
    public static void ExecuteMMBCommand(string arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "MMB",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("无法启动MMB进程");
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"MMB命令执行失败: {error}");
        }
    }
}
