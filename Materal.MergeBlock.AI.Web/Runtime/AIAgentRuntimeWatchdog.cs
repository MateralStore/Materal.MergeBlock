using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Materal.MergeBlock.AI.Web.Runtime;

/// <summary>
/// AI Agent运行时看门狗
/// </summary>
public class AIAgentRuntimeWatchdog(AIAgentWatchdogOptions options)
{
    /// <summary>
    /// 监听运行时输出
    /// </summary>
    public async IAsyncEnumerable<AIAgentRunOutput> WatchAsync(
        IAsyncEnumerable<AIAgentRunOutput> source,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Channel<AIAgentRunOutput> channel = Channel.CreateUnbounded<AIAgentRunOutput>();
        using CancellationTokenSource producerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task producer = Task.Run(async () =>
        {
            try
            {
                await foreach (AIAgentRunOutput output in source.WithCancellation(producerCancellationTokenSource.Token))
                {
                    await channel.Writer.WriteAsync(output, producerCancellationTokenSource.Token);
                }
                channel.Writer.TryComplete();
            }
            catch (OperationCanceledException)
            {
                channel.Writer.TryComplete();
            }
            catch (Exception exception)
            {
                channel.Writer.TryComplete(exception);
            }
        }, CancellationToken.None);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset lastOutputAt = now;
        DateTimeOffset lastNonThinkingAt = now;
        bool thinkingOnly = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            Task<bool> waitForOutputTask = channel.Reader.WaitToReadAsync(cancellationToken).AsTask();
            Task heartbeatTask = Task.Delay(options.HeartbeatInterval, cancellationToken);
            Task completedTask = await Task.WhenAny(waitForOutputTask, heartbeatTask);
            if (completedTask == heartbeatTask)
            {
                now = DateTimeOffset.UtcNow;
                yield return AIAgentRunOutput.Heartbeat();
                if (now - lastOutputAt >= options.IdleTimeout)
                {
                    producerCancellationTokenSource.Cancel();
                    yield return AIAgentRunOutput.RecoveryStarted();
                    yield return AIAgentRunOutput.RecoveryFailed("runtime idle timeout");
                    yield return AIAgentRunOutput.Error("Runtime idle timeout.", "runtime_idle_timeout");
                    yield break;
                }
                if (thinkingOnly && now - lastNonThinkingAt >= options.ThinkingOnlyTimeout)
                {
                    producerCancellationTokenSource.Cancel();
                    yield return AIAgentRunOutput.RecoveryStarted();
                    yield return AIAgentRunOutput.RecoveryFailed("thinking-only timeout");
                    yield return AIAgentRunOutput.Error("Runtime thinking stream timed out.", "runtime_thinking_timeout");
                    yield break;
                }
                continue;
            }

            bool hasOutput = await waitForOutputTask;
            if (!hasOutput) break;
            while (channel.Reader.TryRead(out AIAgentRunOutput? output))
            {
                now = DateTimeOffset.UtcNow;
                lastOutputAt = now;
                if (output.Type is AIAgentRunOutputType.ThinkingDelta)
                {
                    thinkingOnly = true;
                }
                else
                {
                    lastNonThinkingAt = now;
                    thinkingOnly = false;
                }
                yield return output;
            }
        }

        await producer;
    }
}
