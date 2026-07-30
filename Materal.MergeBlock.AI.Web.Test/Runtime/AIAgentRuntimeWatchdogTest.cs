using Materal.MergeBlock.AI.Abstractions.Runtime;
using Materal.MergeBlock.AI.Web.Runtime;

namespace Materal.MergeBlock.AI.Web.Test.Runtime;

[TestClass]
public class AIAgentRuntimeWatchdogTest
{
    [TestMethod]
    public async Task WatchAsync_ShouldEmitHeartbeatWhileWaiting()
    {
        AIAgentRuntimeWatchdog watchdog = new(new AIAgentWatchdogOptions
        {
            HeartbeatInterval = TimeSpan.FromMilliseconds(10),
            IdleTimeout = TimeSpan.FromMilliseconds(100),
            ThinkingOnlyTimeout = TimeSpan.FromMilliseconds(100)
        });

        async IAsyncEnumerable<AIAgentRunOutput> SlowOutputs()
        {
            await Task.Delay(30);
            yield return AIAgentRunOutput.MessageDelta("hello");
            yield return AIAgentRunOutput.RunCompleted();
        }

        List<AIAgentRunOutput> outputs = [];
        await foreach (AIAgentRunOutput output in watchdog.WatchAsync(SlowOutputs(), CancellationToken.None))
        {
            outputs.Add(output);
        }

        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.Heartbeat));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.MessageDelta));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.RunCompleted));
    }

    [TestMethod]
    public async Task WatchAsync_ShouldEmitRecoveryAndError_WhenRuntimeIsIdle()
    {
        AIAgentRuntimeWatchdog watchdog = new(new AIAgentWatchdogOptions
        {
            HeartbeatInterval = TimeSpan.FromMilliseconds(10),
            IdleTimeout = TimeSpan.FromMilliseconds(30),
            ThinkingOnlyTimeout = TimeSpan.FromMilliseconds(100)
        });

        async IAsyncEnumerable<AIAgentRunOutput> IdleOutputs()
        {
            await Task.Delay(200);
            yield return AIAgentRunOutput.MessageDelta("late");
        }

        List<AIAgentRunOutput> outputs = [];
        await foreach (AIAgentRunOutput output in watchdog.WatchAsync(IdleOutputs(), CancellationToken.None))
        {
            outputs.Add(output);
            if (output.Type == AIAgentRunOutputType.Error) break;
        }

        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.Heartbeat));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.RecoveryStarted));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.RecoveryFailed));
        Assert.IsTrue(outputs.Any(m => m.Type == AIAgentRunOutputType.Error));
    }
}
