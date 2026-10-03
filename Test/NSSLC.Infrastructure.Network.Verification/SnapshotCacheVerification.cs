using System.Runtime.InteropServices;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class SnapshotCacheVerification {
  public static async Task OwnershipAndVersionsAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var budget = new PacketByteBudget(2 * 1024 * 1024);
    var time = new ManualTimeProvider();
    using var cache = new PacketSnapshotCache(profile, budget, maximumBytes: 4096,
        maximumItems: 4, timeProvider: time);
    Guid world = Guid.NewGuid();
    var first = new PacketSnapshotCacheKey(profile.Key, world, 1, new(0, 0), 1, "public");
    Packet10Packet packet = CreateSection();
    Verify.That(await cache.TryStoreAsync(first, packet), "The shared section snapshot was not cached.");
    packet.Body.StartX = 999;
    Verify.That(cache.TryGet(first, out ReadOnlyMemory<byte> encoded) && encoded.Span[2] == 10,
        "The cached section must own its encoded bytes independently of later packet mutation.");
    Packet10Packet decoded = (Packet10Packet)profile.Find(PacketDirection.ServerToClient, (byte)10)
        .Decode(encoded[3..]);
    Verify.That(decoded.Body.StartX == 0 && decoded.Body.Width == 1,
        "Cache bytes must preserve the snapshot captured at store time.");
    byte[] original = encoded.ToArray();
    Verify.That(MemoryMarshal.TryGetArray(encoded, out ArraySegment<byte> writable),
        "The ownership fixture requires an array-backed returned copy.");
    writable.Array![writable.Offset + 2] = 255;
    Verify.That(cache.TryGet(first, out ReadOnlyMemory<byte> again) && again.Span.SequenceEqual(original),
        "A caller mutating a returned copy must not corrupt shared cached bytes.");

    var secondRevision = first with { SnapshotRevision = 2 };
    Verify.That(await cache.TryStoreAsync(secondRevision, CreateSection())
        && !cache.TryGet(first, out _) && cache.TryGet(secondRevision, out _),
        "A newer revision must invalidate the previous snapshot of the same section and variant.");
    Verify.That(!await cache.TryStoreAsync(first, CreateSection())
        && cache.TryGet(secondRevision, out _),
        "A late older revision must not replace a newer cached snapshot.");
    var nextWorld = secondRevision with { WorldGeneration = 2, SnapshotRevision = 0 };
    Verify.That(await cache.TryStoreAsync(nextWorld, CreateSection())
        && !cache.TryGet(secondRevision, out _),
        "A new world generation must invalidate its predecessor independently of revision numbering.");
    Verify.Throws<ArgumentException>(() => cache.TryGet(first with { ProfileKey = "another" }, out _));
    Verify.Throws<ArgumentException>(() => cache.TryGet(first with { WorldKey = Guid.Empty }, out _));
    await Verify.ThrowsAsync<PacketEncodingException>(
        async () => await cache.TryStoreAsync(first, new Packet38Packet { Password = "secret" }));
    await Verify.ThrowsAsync<PacketEncodingException>(
        async () => await cache.TryStoreAsync(first, new Packet161Packet { Payload = new("secret") }));
    time.Advance(TimeSpan.FromSeconds(2));
    cache.SweepExpired();
    Verify.That(cache.Count == 0 && cache.UsedBytes == 0 && budget.Used == 0,
        "TTL eviction must return both cache accounting and process byte reservations.");
    await cache.TryStoreAsync(first, CreateSection());
    cache.InvalidateWorld(world);
    Verify.That(cache.Count == 0 && budget.Used == 0,
        "Explicit world invalidation must release every matching shared section.");
  }

  public static async Task LimitsAndEvictionAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var budget = new PacketByteBudget(2 * 1024 * 1024);
    Guid world = Guid.NewGuid();
    var first = new PacketSnapshotCacheKey(profile.Key, world, 1, new(0, 0), 1, "public");
    var second = first with { Section = new(1, 0) };
    var third = first with { Section = new(2, 0) };
    using (var cache = new PacketSnapshotCache(profile, budget, maximumBytes: 4096, maximumItems: 2)) {
      await cache.TryStoreAsync(first, CreateSection());
      await cache.TryStoreAsync(second, CreateSection());
      Verify.That(cache.TryGet(first, out _), "First LRU entry was not present before access.");
      await cache.TryStoreAsync(third, CreateSection());
      Verify.That(cache.Count == 2 && cache.TryGet(first, out _) && cache.TryGet(third, out _)
          && !cache.TryGet(second, out _) && cache.UsedBytes == budget.Used,
          "Entry limits must evict the least recently used snapshot and balance process bytes.");
    }
    Verify.That(budget.Used == 0, "Disposing the cache must release all entries exactly once.");
    using (var small = new PacketSnapshotCache(profile, budget, maximumBytes: 3)) {
      Verify.That(!await small.TryStoreAsync(first, CreateSection())
          && small.Count == 0 && budget.Used == 0,
          "An oversized entry must fail caching without retaining its encoding reservation.");
    }
    var unavailableBudget = new PacketByteBudget(ushort.MaxValue - 1);
    using (var unavailable = new PacketSnapshotCache(profile, unavailableBudget)) {
      Verify.That(!await unavailable.TryStoreAsync(first, CreateSection()) && unavailableBudget.Used == 0,
          "Unavailable process capacity must disable caching without blocking or leaking.");
    }
  }

  public static Packet10Packet CreateSection() {
    return new Packet10Packet {
      Body = new Packet10Body { Width = 1, Height = 1, Tiles = new[] { new Packet10Tile() } }
    };
  }
}
