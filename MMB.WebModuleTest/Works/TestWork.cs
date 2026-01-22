using Materal.MergeBlock.Oscillator.Abstractions;

namespace MMB.WebModuleTest.Works;

/// <summary>
/// 测试作业
/// </summary>
public class TestWork() : MergeBlockWork<TestWorkData>
{
    /// <inheritdoc/>
    protected override async Task WorkInitAsync(IOscillatorContext workContext)
    {
        Console.WriteLine($"{DateTime.Now}_计划初始化");
        await base.WorkInitAsync(workContext);
    }

    /// <inheritdoc/>
    protected override async Task WorkExecuteAsync(IOscillatorContext workContext)
    {
        Console.WriteLine($"{DateTime.Now}_计划执行");
    }
}
