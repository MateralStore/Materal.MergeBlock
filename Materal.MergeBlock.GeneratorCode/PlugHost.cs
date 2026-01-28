namespace Materal.MergeBlock.GeneratorCode;

/// <summary>
/// 插件主机
/// </summary>
public static class PlugHost
{
    /// <summary>
    /// 执行
    /// </summary>
    /// <returns></returns>
    public static async Task RunAsync(string projectPath)
    {
        GeneratorCodeContext context = new(projectPath);
        context.DeleteAllMGCDirectorys();
        context.Refresh();
        foreach (IMergeBlockGeneratorCodePlug plug in context.GeneratorCodePlugs)
        {
            await plug.BeforeExcuteAsync(context);
            context.Refresh();
        }
        foreach (IMergeBlockGeneratorCodePlug plug in context.GeneratorCodePlugs)
        {
            await plug.ExcuteAsync(context);
            context.Refresh();
        }
        foreach (IMergeBlockGeneratorCodePlug plug in context.GeneratorCodePlugs)
        {
            await plug.AfterExcuteAsync(context);
            context.Refresh();
        }
    }
}
