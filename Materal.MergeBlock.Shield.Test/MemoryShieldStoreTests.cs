using Materal.MergeBlock.Shield.Models;

namespace Materal.MergeBlock.Shield.Test;

[TestClass]
public sealed class MemoryShieldStoreTests
{
    [TestMethod]
    public async Task RecordNotFoundAsync_ShouldBlock_WhenLimitReached()
    {
        MemoryShieldStore store = new();
        ShieldOptions options = new()
        {
            WindowSeconds = 60,
            NotFoundLimit = 2,
            BlockedSeconds = 600,
        };
        DateTimeOffset now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        ShieldHitResult first = await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/missing1", "GET", null, now), options);
        ShieldHitResult second = await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/missing2", "GET", null, now.AddSeconds(1)), options);

        Assert.AreEqual(1, first.HitCount);
        Assert.IsFalse(first.IsBlocked);
        Assert.AreEqual(2, second.HitCount);
        Assert.IsTrue(second.IsBlocked);
        Assert.IsTrue(second.IsNewlyBlocked);
        Assert.AreEqual(now.AddSeconds(601), second.BlockedUntil);
    }

    [TestMethod]
    public async Task RecordNotFoundAsync_ShouldIgnoreExpiredHits()
    {
        MemoryShieldStore store = new();
        ShieldOptions options = new()
        {
            WindowSeconds = 10,
            NotFoundLimit = 2,
            BlockedSeconds = 600,
        };
        DateTimeOffset now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/old", "GET", null, now), options);
        ShieldHitResult result = await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/new", "GET", null, now.AddSeconds(11)), options);

        Assert.AreEqual(1, result.HitCount);
        Assert.IsFalse(result.IsBlocked);
    }

    [TestMethod]
    public async Task GetBlockStateAsync_ShouldClearExpiredBlock()
    {
        MemoryShieldStore store = new();
        ShieldOptions options = new()
        {
            WindowSeconds = 60,
            NotFoundLimit = 1,
            BlockedSeconds = 5,
        };
        DateTimeOffset now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        await store.RecordNotFoundAsync("127.0.0.1", new ShieldHitContext("/api/missing", "GET", null, now), options);
        ShieldBlockState blocked = await store.GetBlockStateAsync("127.0.0.1", now.AddSeconds(1));
        ShieldBlockState expired = await store.GetBlockStateAsync("127.0.0.1", now.AddSeconds(6));

        Assert.IsTrue(blocked.IsBlocked);
        Assert.IsFalse(expired.IsBlocked);
    }
}
