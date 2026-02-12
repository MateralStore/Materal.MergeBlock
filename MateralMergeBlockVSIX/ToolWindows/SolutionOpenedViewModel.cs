#nullable enable
using Materal.Abstractions;
using MateralMergeBlockVSIX.Helper;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace MateralMergeBlockVSIX.ToolWindows
{
    public partial class SolutionOpenedViewModel : ObservableObject
    {
        /// <summary>
        /// 解决方案名称
        /// </summary>
        public string SolutionName { get; set { field = value; NotifyPropertyChanged(); } } = "不是MergeBlock项目";

        /// <summary>
        /// 显示状态
        /// </summary>
        public Visibility Visibility { get; set { field = value; NotifyPropertyChanged(); } } = Visibility.Collapsed;
        /// <summary>
        /// 模块
        /// </summary>
        public ObservableCollection<ModuleViewModel> Modules { get; set; } = [];
        private string? _projectName;
        public string? ProjectName => _projectName;
        private string? _projectPath;
        public string? ProjectPath => _projectPath;
        private string? _moduleName;
        private SolutionItem? _coreAbstractions;
        private SolutionItem? _coreApplication;
        private SolutionItem? _coreRepository;
        private SolutionItem? _moduleAbstractions;
        private SolutionItem? _moduleApplication;
        private SolutionItem? _moduleRepository;
        private SolutionItem? _moduleWebAPI;
        private Solution? _solution;
        /// <summary>
        /// 生成代码
        /// </summary>
        public async Task GeneratorCodeAsync()
        {
            try
            {
                await VS.StatusBar.ShowMessageAsync("开始生成代码");
                CommandHelper.ExecuteMMBCommand($"GeneratorCode --ModulePath {ProjectPath}\\{ProjectName}.{_moduleName}");
                await VS.StatusBar.ShowMessageAsync("代码生成成功");
            }
            catch (Exception ex)
            {
                await VS.MessageBox.ShowAsync("错误", ex.GetErrorMessage(), OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="solution"></param>
        public void Init(Solution solution)
        {
            try
            {
                _solution = solution;
                var solutionNames = _solution.Name.Split('.');
                if (solutionNames.Length != 3) throw new Exception("解决方案名称不符合规范");
                _projectName = solutionNames[0];
                _moduleName = solutionNames[1];
                if (_moduleName == "Core")
                {
                    _moduleName = null;
                }
                BindingSolutionItems(solution.Children);
                if (_coreAbstractions is null || _coreApplication is null || _coreRepository is null
                    || _moduleAbstractions is null || _moduleApplication is null || _moduleRepository is null || _moduleWebAPI is null)
                {
                    InitFail();
                    return;
                }
                SolutionName = $"{_projectName}.{_moduleName}";
                Visibility = Visibility.Visible;
                FindOtherModules();
            }
            catch
            {
                InitFail();
            }
        }
        /// <summary>
        /// 初始化失败
        /// </summary>
        private void InitFail()
        {
            SolutionName = "不是MergeBlock项目";
            Visibility = Visibility.Collapsed;
            _projectName = null;
            _moduleName = null;
            _solution = null;
            _coreAbstractions = null;
            _coreApplication = null;
            _coreRepository = null;
            _moduleAbstractions = null;
            _moduleApplication = null;
            _moduleRepository = null;
            _moduleWebAPI = null;
        }
        /// <summary>
        /// 绑定解决方案项
        /// </summary>
        /// <param name="solutionItems"></param>
        private void BindingSolutionItems(IEnumerable<SolutionItem?> solutionItems)
        {
            foreach (SolutionItem? solutionItem in solutionItems)
            {
                if (solutionItem is null) continue;
                BindingSolutionItem(solutionItem);
            }
        }
        /// <summary>
        /// 绑定解决方案项
        /// </summary>
        /// <param name="solutionItem"></param>
        /// <exception cref="Exception"></exception>
        private void BindingSolutionItem(SolutionItem solutionItem)
        {
            if (solutionItem.Type == SolutionItemType.SolutionFolder)
            {
                BindingSolutionItems(solutionItem.Children);
            }
            else if (solutionItem.Type == SolutionItemType.Project)
            {
                string[] projectNames = solutionItem.Name.Split('.');
                if (projectNames.Length != 3) return;
                if (projectNames[1] != "Core")
                {
                    _moduleName ??= projectNames[1];
                    if (_projectName != projectNames[0]) return;
                    if (_moduleName != projectNames[1]) return;
                    switch (projectNames[2])
                    {
                        case "WebAPI":
                            _moduleWebAPI = solutionItem;
                            break;
                        case "Abstractions":
                            _moduleAbstractions = solutionItem;
                            break;
                        case "Application":
                            _moduleApplication = solutionItem;
                            break;
                        case "Repository":
                            _moduleRepository = solutionItem;
                            break;
                    }
                }
                else
                {
                    switch (projectNames[2])
                    {
                        case "Abstractions":
                            _coreAbstractions = solutionItem;
                            break;
                        case "Application":
                            _coreApplication = solutionItem;
                            break;
                        case "Repository":
                            _coreRepository = solutionItem;
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// 寻找其他模块
        /// </summary>
        private void FindOtherModules()
        {
            if (_solution is null || string.IsNullOrWhiteSpace(_solution.FullPath)) return;
            string? solutionPath = _solution.FullPath;
            FileInfo thisSolutionFileInfo = new(solutionPath);
            if (!thisSolutionFileInfo.Exists) return;
            DirectoryInfo projectDirectoryInfo = thisSolutionFileInfo.Directory.Parent;
            if (!projectDirectoryInfo.Exists) return;
            _projectPath = projectDirectoryInfo.FullName;
            foreach (DirectoryInfo moduleDirectoryInfo in projectDirectoryInfo.GetDirectories())
            {
                if (!moduleDirectoryInfo.Name.StartsWith($"{_projectName}.")) continue;
                string moduleFullName = moduleDirectoryInfo.Name;
                FileInfo[] slnFileInfos = moduleDirectoryInfo.GetFiles("*.sln");
                AddModules(moduleFullName, slnFileInfos);
                FileInfo[] slnxFileInfos = moduleDirectoryInfo.GetFiles("*.slnx");
                AddModules(moduleFullName, slnxFileInfos);
            }
        }

        /// <summary>
        /// 添加模块
        /// </summary>
        /// <param name="moduleFullName"></param>
        /// <param name="slnFileInfos"></param>
        private void AddModules(string moduleFullName, FileInfo[] slnFileInfos)
        {
            foreach (FileInfo solutionFileInfo in slnFileInfos)
            {
                ModuleViewModel moduleViewModel = new()
                {
                    SolutionPath = solutionFileInfo.FullName,
                    CanOpen = SolutionName != moduleFullName
                };
                Modules.Add(moduleViewModel);
            }
        }
    }
}
