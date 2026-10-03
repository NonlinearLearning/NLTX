using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class CachedDispatchVerification {
  public static async Task RunAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var budget = new PacketByteBudget(2 * 1024 * 1024);
    using var cache = new PacketSnapshotCache(profile, budget);
    Guid world = Guid.NewGuid();
    var section = new SectionCoordinate(0, 0);
    Packet10Packet snapshot = SnapshotCacheVerification.CreateSection();
    long revision = 1;
    await using var gateway = new PacketGateway(profile, new RecordingAuthority(), snapshotCache: cache);
    GatewayVerification.RegisterProgression(gateway);
    gateway.Register(new PacketPolicy(30, NetworkSessionStage.Active),
        new RecordingHandler<Packet30Packet>((_, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, new[] {
              new OutboundDispatch(snapshot, PacketDispatchKind.SectionSubscribers,
                  worldKey: world, worldGeneration: 1, section: section, snapshotRevision: revision)
            }))));
    await using var sender = new GatewayPeer(gateway, profile);
    await using var subscribed = new GatewayPeer(gateway, profile);
    await using var excluded = new GatewayPeer(gateway, profile);
    await sender.JoinAsync();
    await subscribed.JoinAsync();
    await excluded.JoinAsync();
    subscribed.Session.Apply(30, new PacketHandlingResult(true, interest:
        new SectionInterestProjection(world, 1, 1, new[] { section })));
    excluded.Session.Apply(30, new PacketHandlingResult(true, interest:
        new SectionInterestProjection(world, 2, 1, new[] { section })));

    sender.Receive(new Packet30Packet());
    await Verify.EventuallyAsync(() => subscribed.Transport.FrameCount == 2,
        "The section cache dispatch did not reach its current subscribed connection.");
    Verify.That(cache.Count == 1 && budget.Used == cache.UsedBytes
        && excluded.Transport.FrameCount == 1,
        "Cache-backed routing must retain cache budget and preserve target eligibility.");
    byte[] first = subscribed.Transport.GetFrame(1);
    snapshot.Body.StartX = 7;
    sender.Receive(new Packet30Packet());
    await Verify.EventuallyAsync(() => subscribed.Transport.FrameCount == 3,
        "A repeated section revision did not use its cached dispatch path.");
    Verify.That(subscribed.Transport.GetFrame(2).SequenceEqual(first),
        "The same immutable snapshot identity must share exactly the originally encoded bytes.");

    revision = 2;
    sender.Receive(new Packet30Packet());
    await Verify.EventuallyAsync(() => subscribed.Transport.FrameCount == 4,
        "A new authoritative snapshot revision did not replace the cached output.");
    Packet10Packet newer = (Packet10Packet)profile.Find(PacketDirection.ServerToClient, (byte)10)
        .Decode(subscribed.Transport.GetFrame(3).AsMemory(3));
    Verify.That(newer.Body.StartX == 7 && cache.Count == 1 && excluded.Transport.FrameCount == 1,
        "New revisions must reencode and route only to eligible current-world subscribers.");
    cache.Dispose();
    Verify.That(budget.Used == 0, "Cache dispatch disposal must release retained shared frames.");
  }
}
