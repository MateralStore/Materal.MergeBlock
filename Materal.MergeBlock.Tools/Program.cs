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
        //await GeneratorCodeAsync(@"E:\Project\Materal\Materal\Materal.MergeBlock\MMB\MMB.Demo");
        await GeneratorCodeAsync(@"E:\Project\GDB\XMJ\Server\XMJ\XMJ.Authority");
#endif
        Option<string?> pathOption = new("--ModulePath") { Description = "指定模块路径" };
        pathOption.Aliases.Add("-m");

        Command generatorCodeCommand = new("GeneratorCode") { Description = "生成代码" };
        generatorCodeCommand.Options.Add(pathOption);
        generatorCodeCommand.SetAction(parseResult => GeneratorCodeAsync(parseResult.GetValue(pathOption)));

        RootCommand rootCommand = new("Materal.MergeBlock.Tools");
        rootCommand.Subcommands.Add(generatorCodeCommand);

        return await rootCommand.Parse(args).InvokeAsync();
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
