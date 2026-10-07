using System.Net;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace NSSLC.NetworkVerification;

internal static class WorldTileMetricsPacketVerification
{
  private static readonly TimeSpan PacketTimeout = TimeSpan.FromSeconds(5);

  public static async Task RunAsync()
  {
    await VerifyPublishedSnapshotProjectionAsync();
    await VerifyTcpSynchronizationAndPublicationAsync();
  }

  private static async Task VerifyPublishedSnapshotProjectionAsync()
  {
    await using var owner = new NetworkWorldOwner(() => new LoadedWorldSession());
    await owner.Ready.WaitAsync(PacketTimeout);
    EntityRuntimeId runtimeId = await owner.InvokeAsync(session => session.EntityRuntime.RuntimeId);
    ProtocolProfile profile = ServerProtocolProfile.Create(
        TerrariaProtocolProfile.Create(GeneratedProfileVerification.CreateFacts()));
    Verify.Throws<PacketProtocolException>(
        () => profile.Find(PacketDirection.ClientToServer, (byte)57));
    var gateway = new PacketGateway(profile, new RecordingAuthority(),
        worldRuntimeIdProvider: () => runtimeId);
    await using (gateway.ConfigureAwait(false))
    {
      var producer = new WorldTileMetricsPacketProducer(gateway, owner, runtimeId);
      Verify.That(await producer.CaptureAsync() is null,
          "An unpublished default metrics value must not be sent as a zero snapshot.");
      Verify.That(!await producer.PublishAsync(),
          "An unpublished metrics owner must not dispatch a fabricated packet 57.");

      WorldTileMetricsPublicationResult empty = await owner.InvokeAsync(session =>
          PublishMetrics(session, good: 0, evil: 0, blood: 0, solid: 0));
      Verify.That(empty.HasSnapshot && empty.ShouldSendMessage57,
          "The formal metrics owner must publish even an empty completed scan.");
      Unknown57Packet emptyPacket = await producer.CaptureAsync()
          ?? throw new InvalidOperationException("A published empty metrics snapshot was omitted.");
      Verify.That(emptyPacket.Good == 0 && emptyPacket.Evil == 0 && emptyPacket.Blood == 0,
          "A completed empty metrics scan must project three zero percentages.");

      await owner.InvokeAsync(session => {
        AccumulateMetrics(session, good: 1, evil: 0, blood: 0, solid: 8);
        return 0;
      });
      Unknown57Packet pendingPacket = await producer.CaptureAsync()
          ?? throw new InvalidOperationException("The prior published metrics snapshot disappeared.");
      Verify.That(pendingPacket.Good == 0 && pendingPacket.Evil == 0 && pendingPacket.Blood == 0,
          "Pending metrics must not replace the prior published packet snapshot.");
      await owner.InvokeAsync(session => PublishPendingMetrics(session));
      Unknown57Packet eighthPacket = await producer.CaptureAsync()
          ?? throw new InvalidOperationException("The published one-eighth metrics snapshot was omitted.");
      Verify.That(eighthPacket.Good == 12,
          "A one-eighth alignment must retain Version4 midpoint-to-even rounding.");

      WorldTileMetricsPublicationResult evilPublication = await owner.InvokeAsync(session =>
          PublishMetrics(session, good: 0, evil: 3, blood: 0, solid: 8));
      Unknown57Packet evilPacket = await producer.CaptureAsync()
          ?? throw new InvalidOperationException("The published evil metrics snapshot was omitted.");
      Verify.That(evilPublication.Snapshot.EvilPercent == 38 && evilPacket.Evil == 38,
          "A three-eighths evil alignment must project the rounded published value 38.");

      WorldTileMetricsPublicationResult bloodPublication = await owner.InvokeAsync(session =>
          PublishMetrics(session, good: 0, evil: 0, blood: 1, solid: 400));
      Unknown57Packet bloodPacket = await producer.CaptureAsync()
          ?? throw new InvalidOperationException("The published blood metrics snapshot was omitted.");
      WorldTileMetricsSnapshot repeatedSnapshot = await owner.InvokeAsync(session =>
          session.World.TileMetrics.CreateSnapshot());
      Unknown57Packet repeatedPacket = await producer.CaptureAsync()
          ?? throw new InvalidOperationException("A repeated published snapshot was omitted.");
      Verify.That(bloodPublication.Snapshot.BloodPercent == 1 && bloodPacket.Blood == 1
          && repeatedPacket.Blood == bloodPacket.Blood && repeatedSnapshot == bloodPublication.Snapshot,
          "A nonzero one-in-400 alignment must publish as 1 without mutating the owner snapshot.");

      var staleProducer = new WorldTileMetricsPacketProducer(gateway, owner,
          new EntityRuntimeId(Guid.NewGuid()));
      Verify.That(!await staleProducer.PublishAsync(),
          "A metrics producer bound to a stale world runtime must not publish packet 57.");
    }
  }

  private static async Task VerifyTcpSynchronizationAndPublicationAsync()
  {
    ProtocolProfile profile = ServerProtocolProfile.Create(
        TerrariaProtocolProfile.Create(GeneratedProfileVerification.CreateFacts()));
    Verify.Throws<PacketProtocolException>(
        () => profile.Find(PacketDirection.ClientToServer, (byte)57));

    await using var worldOwner = new NetworkWorldOwner(() => new LoadedWorldSession());
    await worldOwner.Ready.WaitAsync(PacketTimeout);
    EntityRuntimeId runtimeId = await worldOwner.InvokeAsync(session =>
    {
      PublishMetrics(session, good: 1, evil: 0, blood: 0, solid: 8);
      return session.EntityRuntime.RuntimeId;
    });

    var world = new WorldDataPacket
    {
      MaxTilesX = 420,
      MaxTilesY = 240,
      SpawnTileX = 100,
      SpawnTileY = 80,
      WorldId = 57,
      WorldName = "packet-57",
      WorldGuid = Guid.NewGuid().ToByteArray(),
      WorldGeneratorVersion = 1
    };
    await using var host = new NetworkGatewayHost(IPAddress.Loopback, 0, profile,
        new RecordingAuthority(), new PacketGatewayOptions
        {
          MaximumSessions = 2,
          StageTimeout = TimeSpan.FromSeconds(10),
          HandshakeTimeout = TimeSpan.FromSeconds(20),
          ActiveIdleTimeout = TimeSpan.FromSeconds(30)
        }, worldRuntimeIdProvider: () => runtimeId);
    var playerOwner = new NetworkPlayerOwner(worldOwner, host.Gateway.IsCurrentSender);
    var lifecycle = new PlayerLifecyclePacketHandlers(playerOwner,
        world.MaxTilesX, world.MaxTilesY);
    var sections = new WorldSynchronizationPacketHandlers(world,
        new TileMapSnapshot(world.MaxTilesX, world.MaxTilesY,
            new TileCellState[world.MaxTilesX * world.MaxTilesY]),
        new bool[1], new PlayerControlsObservationStore(),
        new TileBreakObservationStore(), new WorldSynchronizationObservation(),
        host.Gateway.IsCurrentSender, () => runtimeId);
    var timeProducer = new WorldTimePacketProducer(host.Gateway, worldOwner, runtimeId);
    var metricsProducer = new WorldTileMetricsPacketProducer(host.Gateway, worldOwner, runtimeId);
    var initialSynchronization = new WorldSpawnSynchronizationPacketHandler(
        sections, timeProducer, metricsProducer);
    host.Gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
        new RecordingHandler<RequestWorldDataPacket>((context, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, [new OutboundDispatch(world, PacketDispatchKind.Single,
                [context.Connection], allowedStages: NetworkSessionStage.AwaitSectionRequest)],
                nextStage: NetworkSessionStage.AwaitSectionRequest))));
    host.Gateway.Register<SpawnTileDataPacket>(
        new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest), initialSynchronization);
    PlayerLifecyclePacketRegistration.Register(host.Gateway, lifecycle);
    host.Start();

    await using IPacketConnection client = await host.Connections.ConnectAsync(
        "127.0.0.1", ((IPEndPoint)host.EndPoint).Port, PacketTimeout);
    await client.WritePacketAsync(new HelloPacket { Version = host.Profile.HelloVersion });
    _ = await ReadRequiredAsync(client, 3, "packet 3 admission");
    await client.WritePacketAsync(new RequestWorldDataPacket());
    _ = await ReadRequiredAsync(client, 7, "packet 7 world data");
    await client.WritePacketAsync(new SpawnTileDataPacket { X = -1, Y = -1 });
    _ = await ReadRequiredAsync(client, 7, "packet 7 initial synchronization");
    StatusTextSizePacket progress = (await ReadRequiredAsync(client, 9,
        "packet 9 initial synchronization")).Get<StatusTextSizePacket>();
    for (int index = 0; index < progress.Value; index++)
    {
      _ = await ReadRequiredAsync(client, 10, "packet 10 initial synchronization");
    }

    _ = await ReadRequiredAsync(client, 49, "packet 49 initial synchronization");
    _ = await ReadRequiredAsync(client, 18, "packet 18 initial synchronization");
    Unknown57Packet initial = (await ReadRequiredAsync(client, 57,
        "packet 57 initial synchronization")).Get<Unknown57Packet>();
    Verify.That(initial.Good == 12 && initial.Evil == 0 && initial.Blood == 0,
        "Packet 8 must send the current published packet-57 snapshot to this connection.");

    await client.WritePacketAsync(new PlayerSpawnPacket());
    _ = await ReadRequiredAsync(client, 129, "packet 129 active transition");
    await Verify.EventuallyAsync(() => host.Gateway.Sessions.Single().Stage
        == NetworkSessionStage.Active, "The TCP client must become Active before packet 57.");

    await worldOwner.InvokeAsync(session => {
      AccumulateMetrics(session, good: 0, evil: 3, blood: 0, solid: 8);
      return 0;
    });
    Verify.That(await metricsProducer.PublishAsync(),
        "The latest completed metrics snapshot must publish through the active world gateway.");
    Unknown57Packet stillPublished = (await ReadRequiredAsync(client, 57,
        "packet 57 prior published snapshot")).Get<Unknown57Packet>();
    Verify.That(stillPublished.Good == 12 && stillPublished.Evil == 0,
        "A pending metrics window must leave packet 57 on the previous published values.");

    await worldOwner.InvokeAsync(session => PublishPendingMetrics(session));
    Verify.That(await metricsProducer.PublishAsync(),
        "A newly completed metrics window must publish through the active world gateway.");
    Unknown57Packet updated = (await ReadRequiredAsync(client, 57,
        "packet 57 updated publication")).Get<Unknown57Packet>();
    Verify.That(updated.Good == 0 && updated.Evil == 38 && updated.Blood == 0,
        "A real TCP client must receive updated percentages from the formal world metrics owner.");
  }

  private static WorldTileMetricsPublicationResult PublishMetrics(
      LoadedWorldSession session,
      int good,
      int evil,
      int blood,
      int solid)
  {
    WorldTileMetricsSystem.Reset(session.World.TileMetrics);
    AccumulateMetrics(session, good, evil, blood, solid);
    return PublishPendingMetrics(session);
  }

  private static void AccumulateMetrics(
      LoadedWorldSession session,
      int good,
      int evil,
      int blood,
      int solid)
  {
    if (good < 0 || evil < 0 || blood < 0 || solid < good + evil + blood)
    {
      throw new ArgumentOutOfRangeException(nameof(solid));
    }

    int[] counts = new int[640];
    counts[2] = solid - good - evil - blood;
    counts[100] = good;
    counts[200] = evil;
    counts[300] = blood;
    WorldTileMetricsSystem.AccumulateAlignmentCounts(session.World.TileMetrics,
        counts, [100], [200], [300], remixWorld: false, clearCounts: true);
  }

  private static WorldTileMetricsPublicationResult PublishPendingMetrics(
      LoadedWorldSession session)
  {
    WorldTileMetricsPublicationResult publication = WorldTileMetricsSystem.BeginColumn(
        session.World.TileMetrics, 0);
    WorldTileMetricsSystem.CompleteColumnStart(session.World.TileMetrics, 0);
    return publication;
  }

  private static async Task<PacketMessage> ReadRequiredAsync(IPacketConnection client,
      byte expectedId, string phase)
  {
    PacketMessage? message = await client.ReadPacketAsync().AsTask().WaitAsync(PacketTimeout);
    if (message is null)
    {
      throw new InvalidOperationException($"TCP peer closed before {phase}.");
    }

    Verify.That(message.MessageId == expectedId,
        $"Expected packet {expectedId} during {phase}, received {message.MessageId}.");
    return message;
  }
}
