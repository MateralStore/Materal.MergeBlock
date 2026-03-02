using Materal.Abstractions;
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
#if DEBUG
        await GeneratorCodeAsync(@"E:\Project\Materal\Materal\Materal.MergeBlock\MMB\MMB.Demo");
        //await GeneratorCodeAsync(@"E:\Project\GDB\YueHeShe\Server\YueHeShe.Main");
#endif
        Option<string> pathOption = new("--ModulePath", "指定模块路径");
        pathOption.AddAlias("-m");
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
        try
        {
            await PlugHost.RunAsync(projectPath);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("代码生成成功！");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"代码生成失败：{ex.GetErrorMessage()}");
            Console.ResetColor();
            throw;
        }
    }
}
