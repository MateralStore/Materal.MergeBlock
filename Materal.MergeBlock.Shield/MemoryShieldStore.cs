using Materal.MergeBlock.Shield.Models;

namespace Materal.MergeBlock.Shield;

/// <summary>
/// 内存Shield状态存储
/// </summary>
public class MemoryShieldStore : IShieldStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, SourceState> _states = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public ValueTask<ShieldBlockState> GetBlockStateAsync(string source, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            if (!_states.TryGetValue(source, out SourceState? state)) return ValueTask.FromResult(new ShieldBlockState(false, null));
            if (state.BlockedUntil is null) return ValueTask.FromResult(new ShieldBlockState(false, null));
            if (state.BlockedUntil > now) return ValueTask.FromResult(new ShieldBlockState(true, state.BlockedUntil));
            state.BlockedUntil = null;
            return ValueTask.FromResult(new ShieldBlockState(false, null));
        }
    }

    /// <inheritdoc/>
    public ValueTask<ShieldHitResult> RecordNotFoundAsync(string source, ShieldHitContext hitContext, ShieldOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            SourceState state = GetOrCreateState(source);
            DateTimeOffset now = hitContext.Timestamp;
            if (state.BlockedUntil is not null && state.BlockedUntil > now)
            {
                return ValueTask.FromResult(CreateResult(state, false));
            }
            if (state.BlockedUntil is not null && state.BlockedUntil <= now)
            {
                state.BlockedUntil = null;
            }

            int windowSeconds = Math.Max(1, options.WindowSeconds);
            DateTimeOffset minHitTime = now.AddSeconds(-windowSeconds);
            state.Hits.RemoveAll(m => m < minHitTime);
            state.Hits.Add(now);

            bool newlyBlocked = false;
            int notFoundLimit = Math.Max(1, options.NotFoundLimit);
            if (state.Hits.Count >= notFoundLimit)
            {
                int blockedSeconds = Math.Max(1, options.BlockedSeconds);
                state.BlockedUntil = now.AddSeconds(blockedSeconds);
                newlyBlocked = true;
            }
            return ValueTask.FromResult(CreateResult(state, newlyBlocked));
        }
    }

    private SourceState GetOrCreateState(string source)
    {
        if (_states.TryGetValue(source, out SourceState? state)) return state;
        state = new SourceState();
        _states[source] = state;
        return state;
    }

    private static ShieldHitResult CreateResult(SourceState state, bool newlyBlocked)
    {
        DateTimeOffset? firstHit = state.Hits.Count > 0 ? state.Hits[0] : null;
        DateTimeOffset? lastHit = state.Hits.Count > 0 ? state.Hits[^1] : null;
        return new ShieldHitResult(state.Hits.Count, state.BlockedUntil is not null, newlyBlocked, firstHit, lastHit, state.BlockedUntil);
    }

    private sealed class SourceState
    {
        public List<DateTimeOffset> Hits { get; } = [];
        public DateTimeOffset? BlockedUntil { get; set; }
    }
}
