#nullable enable
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;
using System.IO;

namespace MateralMergeBlockVSIX.ToolWindows.ViewModels
{
    public partial class SolutionNotOpenedViewModel : ObservableObject
    {
        public string ProjectName
        {
            get;
            set
            {
                field = value;
                NotifyPropertyChanged();
            }
        } = "MMB";

        public string ModuleName
        {
            get;
            set
            {
                field = value;
                NotifyPropertyChanged();
            }
        } = "NewModule";

        public string ProjectPath
        {
            get;
            set
            {
                field = value;
                NotifyPropertyChanged();
                if (string.IsNullOrEmpty(ProjectPath)) return;
                ProjectName = Path.GetFileName(ProjectPath);
            }
        } = @"C:\Project\MMB";
        public void CreateModule()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            CreateCoreSolution();
            string slnFilePath = CreateModuleSolution();
            OpenSln(slnFilePath);
        }
        /// <summary>
        /// 创建核心解决方案
        /// </summary>
        private void CreateCoreSolution()
        {
            string coreDirectoryName = $"{ProjectName}.Core";
            string coreDirectoryPath = Path.Combine(ProjectPath, coreDirectoryName);
            if (Directory.Exists(coreDirectoryPath)) return;
            ExecuteDotnetCommand($"new materal-core -o \"{ProjectPath}\"");
        }
        /// <summary>
        /// 创建模块解决方案
        /// </summary>
        private string CreateModuleSolution()
        {
            string moduleDirectoryName = $"{ProjectName}.{ModuleName}";
            string moduleDirectoryPath = Path.Combine(ProjectPath, moduleDirectoryName);
            if (!Directory.Exists(moduleDirectoryPath))
            {
                ExecuteDotnetCommand($"new materal-module --ModuleName {ModuleName} -o \"{ProjectPath}\"");
            }
            return Path.Combine(moduleDirectoryPath, $"{moduleDirectoryName}.slnx");
        }
        /// <summary>
        /// 打开Sln文件
        /// </summary>
        /// <param name="slnPath"></param>
        private void OpenSln(string slnPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!File.Exists(slnPath)) return;
            if (Package.GetGlobalService(typeof(SVsSolution)) is not IVsSolution solution) return;
            solution.OpenSolutionFile((uint)__VSSLNOPENOPTIONS.SLNOPENOPT_Silent, slnPath);
        }
        /// <summary>
        /// 执行dotnet命令
        /// </summary>
        /// <param name="arguments">命令参数</param>
        private void ExecuteDotnetCommand(string arguments)
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
    }
}
