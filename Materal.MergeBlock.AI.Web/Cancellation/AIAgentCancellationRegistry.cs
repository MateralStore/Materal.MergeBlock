namespace Materal.MergeBlock.AI.Web.Cancellation;

/// <summary>
/// AI Agent取消注册表
/// </summary>
public class AIAgentCancellationRegistry
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _sources = new(StringComparer.Ordinal);
    /// <summary>
    /// 注册运行
    /// </summary>
    public CancellationToken Register(string runId)
    {
        CancellationTokenSource source = new();
        _sources[runId] = source;
        return source.Token;
    }
    /// <summary>
    /// 取消运行
    /// </summary>
    public bool Cancel(string runId)
    {
        if (!_sources.TryRemove(runId, out CancellationTokenSource? source)) return false;
        source.Cancel();
        source.Dispose();
        return true;
    }
    /// <summary>
    /// 完成运行
    /// </summary>
    public void Complete(string runId)
    {
        if (!_sources.TryRemove(runId, out CancellationTokenSource? source)) return;
        source.Dispose();
    }
}
