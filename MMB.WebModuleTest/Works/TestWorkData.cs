using Materal.MergeBlock.Oscillator.Abstractions;
using Materal.Oscillator.Abstractions.PlanTriggers;

namespace MMB.WebModuleTest.Works;

/// <summary>
/// 测试作业数据
/// </summary>
public partial class TestWorkData() : MergeBlockWorkData("测试作业数据")
{
    /// <inheritdoc/>
    public override ICollection<IPlanTriggerData> GetPlanTriggers()
    {
        IPlanTriggerData planTrigger = new OneTimePlanTriggerData() { StartTime = DateTime.Now.AddSeconds(10) };
        Console.WriteLine(planTrigger.GetDescriptionText());
        return [planTrigger];
    }
}