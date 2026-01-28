using Materal.MergeBlock.GeneratorCode;
using System.CommandLine;

namespace Materal.MergeBlock.Tools;

/// <summary>
/// 主程序
/// </summary>
public class Program
{
    static async Task<int> Main(string[] args)
    {
        Option<string> pathOption = new("--ProjectPath", "指定项目根路径");
        pathOption.AddAlias("-p");
        pathOption.IsRequired = false;

        Command generatorCodeCommand = new("GeneratorCode", "生成代码");
        generatorCodeCommand.AddOption(pathOption);
        generatorCodeCommand.SetHandler(GeneratorCodeAsync, pathOption);

        RootCommand rootCommand = new("Materal.MergeBlock.Tools");
        rootCommand.AddCommand(generatorCodeCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task GeneratorCodeAsync(string? projectPath)
    {
        projectPath = string.IsNullOrWhiteSpace(projectPath) ? Environment.CurrentDirectory : projectPath;
        await PlugHost.RunAsync(projectPath);
    }
}
