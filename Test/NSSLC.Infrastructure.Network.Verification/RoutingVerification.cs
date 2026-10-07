using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class RoutingVerification {
  public static async Task ServerWorldEffectsAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var originalRuntime = new Terraria.Relationships.EntityRuntimeId(Guid.NewGuid());
    Terraria.Relationships.EntityRuntimeId currentRuntime = originalRuntime;
    await using var gateway = new PacketGateway(profile, new RecordingAuthority(),
        worldRuntimeIdProvider: () => currentRuntime);
    GatewayVerification.RegisterProgression(gateway);
    await using var subscribed = new GatewayPeer(gateway, profile);
    await using var otherGeneration = new GatewayPeer(gateway, profile);
    await using var noInterest = new GatewayPeer(gateway, profile);
    await subscribed.JoinAsync();
    await otherGeneration.JoinAsync();
    await noInterest.JoinAsync();
    var section = new SectionCoordinate(0, 0);
    subscribed.Session.Apply(30, new PacketHandlingResult(true, interest:
        new SectionInterestProjection(originalRuntime.Value, 0, 1, [section])));
    otherGeneration.Session.Apply(30, new PacketHandlingResult(true, interest:
        new SectionInterestProjection(originalRuntime.Value, 1, 1, [section])));
    int subscribedBefore = subscribed.Transport.FrameCount;
    int otherBefore = otherGeneration.Transport.FrameCount;
    int noInterestBefore = noInterest.Transport.FrameCount;
    var repair = new OutboundDispatch(new TileFrameSectionPacket(),
        PacketDispatchKind.SectionSubscribers, worldKey: originalRuntime.Value, section: section);
    Verify.That(await gateway.PublishWorldAsync(originalRuntime, repair),
        "A current server-owned frame repair must complete without an incoming player packet.");
    Verify.That(subscribed.Transport.FrameCount == subscribedBefore + 1
        && otherGeneration.Transport.FrameCount == otherBefore
        && noInterest.Transport.FrameCount == noInterestBefore,
        "Server-owned repair must reach only active subscribers of the exact world generation.");
    byte[] frame = subscribed.Transport.GetFrame(subscribedBefore);
    var decoded = (TileFrameSectionPacket)profile.Find(PacketDirection.ServerToClient, (byte)11)
        .Decode(frame.AsMemory(3));
    Verify.That(decoded.X == 0 && decoded.Y == 0 && decoded.Width == 0 && decoded.Height == 0,
        "Server publication must preserve the valid single-section packet 11 wire coordinates.");
    var broadcast = new OutboundDispatch(new TileFrameSectionPacket(),
        PacketDispatchKind.AllActive, worldKey: originalRuntime.Value);
    Verify.That(await gateway.PublishWorldAsync(originalRuntime, broadcast)
        && subscribed.Transport.FrameCount == subscribedBefore + 2
        && otherGeneration.Transport.FrameCount == otherBefore + 1
        && noInterest.Transport.FrameCount == noInterestBefore + 1,
        "A server-owned all-active effect has no initiating player to exclude.");
    currentRuntime = new(Guid.NewGuid());
    Verify.That(!await gateway.PublishWorldAsync(originalRuntime, repair)
        && subscribed.Transport.FrameCount == subscribedBefore + 2,
        "A committed event from a replaced world must be rejected without another send.");
    var invalid = new OutboundDispatch(new TileFrameSectionPacket(),
        PacketDispatchKind.AllActiveExceptSender, worldKey: currentRuntime.Value);
    await Verify.ThrowsAsync<ArgumentException>(async () =>
        await gateway.PublishWorldAsync(currentRuntime, invalid));
    using var canceled = new CancellationTokenSource();
    canceled.Cancel();
    await Verify.ThrowsAsync<OperationCanceledException>(async () =>
        await gateway.PublishWorldAsync(originalRuntime, repair, canceled.Token));
  }

  public static async Task TargetsAndSectionsAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    await using var gateway = new PacketGateway(profile, new RecordingAuthority());
    GatewayVerification.RegisterProgression(gateway);
    OutboundDispatch? next = null;
    int handled = 0;
    gateway.Register(new PacketPolicy(30, NetworkSessionStage.Active),
        new RecordingHandler<TogglePVPPacket>((_, _, _) => {
          OutboundDispatch intent = next!;
          Interlocked.Increment(ref handled);
          return ValueTask.FromResult(new PacketHandlingResult(true, new[] { intent }));
        }));
    await using var sender = new GatewayPeer(gateway, profile);
    await using var second = new GatewayPeer(gateway, profile);
    await using var third = new GatewayPeer(gateway, profile);
    await sender.JoinAsync();
    await second.JoinAsync();
    await third.JoinAsync();
    await Verify.EventuallyAsync(() => sender.Transport.FrameCount == 1
        && second.Transport.FrameCount == 1 && third.Transport.FrameCount == 1,
        "The three joining peers did not complete their slot messages.");

    next = new OutboundDispatch(new PlayerLifeManaPacket { Player = 0, Life = 40, MaximumLife = 100 },
        PacketDispatchKind.AllActiveExceptSender);
    sender.Receive(new TogglePVPPacket());
    await Verify.EventuallyAsync(() => second.Transport.FrameCount == 2
        && third.Transport.FrameCount == 2, "Broadcast did not reach both other active peers.");
    Verify.That(sender.Transport.FrameCount == 1,
        "AllActiveExceptSender must exclude exactly the initiating current connection.");

    next = new OutboundDispatch(new PlayerLifeManaPacket { Player = 0, Life = 41, MaximumLife = 100 },
        PacketDispatchKind.Single, new[] { sender.Connection.Identity });
    sender.Receive(new TogglePVPPacket());
    await Verify.EventuallyAsync(() => sender.Transport.FrameCount == 2,
        "Explicit self echo was unexpectedly excluded.");
    Verify.That(second.Transport.FrameCount == 2 && third.Transport.FrameCount == 2,
        "A self-targeted authoritative reply must preserve its exact target scope.");

    next = new OutboundDispatch(new PlayerLifeManaPacket { Player = 0, Life = 42, MaximumLife = 100 },
        PacketDispatchKind.ExplicitTargets, new[] {
          second.Connection.Identity with { Epoch = second.Connection.Identity.Epoch + 1 },
          new ConnectionIdentity(Guid.NewGuid(), 1)
        });
    sender.Receive(new TogglePVPPacket());
    await Verify.EventuallyAsync(() => Volatile.Read(ref handled) == 3,
        "The stale explicit target intent was not handled.");
    next = new OutboundDispatch(new PlayerLifeManaPacket { Player = 0, Life = 43, MaximumLife = 100 },
        PacketDispatchKind.Single, new[] { third.Connection.Identity });
    sender.Receive(new TogglePVPPacket());
    await Verify.EventuallyAsync(() => third.Transport.FrameCount == 3,
        "The current explicitly targeted peer did not receive its reply.");
    Verify.That(second.Transport.FrameCount == 2,
        "An explicit stale epoch and nonexistent identity must not route to a current peer.");

    Guid world = Guid.NewGuid();
    var section = new SectionCoordinate(2, 3);
    second.Session.Apply(30, new PacketHandlingResult(true, interest:
        new SectionInterestProjection(world, 4, 1, new[] { section })));
    third.Session.Apply(30, new PacketHandlingResult(true, interest:
        new SectionInterestProjection(world, 5, 1, new[] { section })));
    next = new OutboundDispatch(new PlayerLifeManaPacket { Player = 0, Life = 44, MaximumLife = 100 },
        PacketDispatchKind.SectionSubscribers, worldKey: world, worldGeneration: 4, section: section);
    sender.Receive(new TogglePVPPacket());
    await Verify.EventuallyAsync(() => second.Transport.FrameCount == 3,
        "The matching world generation and section subscriber did not receive its snapshot.");
    Verify.That(third.Transport.FrameCount == 3 && sender.Transport.FrameCount == 2,
        "Section routing must exclude non-subscribers and other world generations.");
    Verify.Throws<PacketProtocolException>(() => second.Session.Apply(30,
        new PacketHandlingResult(true, interest:
            new SectionInterestProjection(world, 3, 2, new[] { section }))));
    Verify.Throws<PacketProtocolException>(() => second.Session.Apply(30,
        new PacketHandlingResult(true, interest:
            new SectionInterestProjection(world, 4, 0, new[] { section }))));
  }

  public static async Task SlowTargetIsolationAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    await using var gateway = new PacketGateway(profile, new RecordingAuthority());
    GatewayVerification.RegisterProgression(gateway);
    gateway.Register(new PacketPolicy(30, NetworkSessionStage.Active),
        new RecordingHandler<TogglePVPPacket>((_, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, new[] {
              new OutboundDispatch(new PlayerLifeManaPacket { Player = 0, Life = 40, MaximumLife = 100 },
                  PacketDispatchKind.AllActiveExceptSender)
            }))));
    await using var sender = new GatewayPeer(gateway, profile);
    await using var slow = new GatewayPeer(gateway, profile,
        ConnectionVerification.Options() with { SendTimeout = TimeSpan.FromMilliseconds(150) });
    await using var normal = new GatewayPeer(gateway, profile);
    await sender.JoinAsync();
    await slow.JoinAsync();
    await normal.JoinAsync();
    slow.Transport.OnSubmit = _ => true;
    sender.Receive(new TogglePVPPacket());
    await Verify.EventuallyAsync(() => normal.Transport.FrameCount == 2,
        "A slow target must not prevent normal recipients from receiving the same broadcast.");
    Verify.That(normal.Session.Stage == NetworkSessionStage.Active,
        "A normal target must remain admitted while the slow target's send is pending.");
    await slow.Run.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(slow.Transport.CloseCalls > 0 && slow.Budget.Used == 0
        && normal.Session.Stage == NetworkSessionStage.Active,
        "Only the slow target must close after its no-progress send deadline.");
  }
}
