using System.Net;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Relationships;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldStorage;

namespace NSSLC.NetworkVerification;

internal static class WorldFrameRepairTcpVerification {
  private static readonly TimeSpan PacketTimeout = TimeSpan.FromSeconds(5);

  public static async Task RunAsync() {
    ProtocolProfile profile = ServerProtocolProfile.Create(
        TerrariaProtocolProfile.Create(GeneratedProfileVerification.CreateFacts()));
    Verify.Throws<PacketProtocolException>(
        () => profile.Find(PacketDirection.ClientToServer, (byte)11));

    EntityRuntimeId worldRuntimeId = new(Guid.NewGuid());
    var world = new WorldDataPacket {
      MaxTilesX = 1000,
      MaxTilesY = 750,
      SpawnTileX = 10,
      SpawnTileY = 10,
      WorldId = 17,
      WorldName = "frame-repair-tcp",
      WorldGuid = Guid.NewGuid().ToByteArray(),
      WorldGeneratorVersion = 1
    };
    var observation = new WorldSynchronizationObservation();
    var authority = new RecordingAuthority();
    await using var host = new NetworkGatewayHost(IPAddress.Loopback, 0, profile,
        authority, new PacketGatewayOptions {
          MaximumSessions = 4,
          StageTimeout = TimeSpan.FromSeconds(10),
          HandshakeTimeout = TimeSpan.FromSeconds(20),
          ActiveIdleTimeout = TimeSpan.FromSeconds(30)
        }, worldRuntimeIdProvider: () => worldRuntimeId);
    var sections = new WorldSynchronizationPacketHandlers(world,
        new TileMapSnapshot(world.MaxTilesX, world.MaxTilesY,
            new TileCellState[world.MaxTilesX * world.MaxTilesY]),
        new bool[1], new PlayerControlsObservationStore(),
        new TileBreakObservationStore(), observation,
        host.Gateway.IsCurrentSender, () => worldRuntimeId);
    RegisterWorldHandlers(host, world, sections, observation);
    host.Start();

    await using IPacketConnection first = await host.Connections.ConnectAsync(
        "127.0.0.1", ((IPEndPoint)host.EndPoint).Port, PacketTimeout);
    await using IPacketConnection second = await host.Connections.ConnectAsync(
        "127.0.0.1", ((IPEndPoint)host.EndPoint).Port, PacketTimeout);
    await JoinAsync(first, host, world);
    await JoinAsync(second, host, world);

    (int X, int Y)[] firstInterest = [(2, 2), (2, 3), (3, 2), (3, 3)];
    foreach ((int sectionX, int sectionY) in firstInterest) {
      await RequestSectionAsync(first, sectionX, sectionY);
    }
    await RequestSectionAsync(second, 3, 3);
    await Verify.EventuallyAsync(() => host.Gateway.Sessions.Count(session =>
        session.Stage == NetworkSessionStage.Active) == 2,
        "Both TCP clients must retain their active session and section interest.");

    var actualAffectedRegion = new WorldGenerationTileFramingAndDebugSystem.TileFrameRegion(
        599, 449, 600, 450);
    WorldGenerationTileFramingAndDebugSystem.Result framing =
        WorldGenerationTileFramingAndDebugSystem.Execute(
            WorldGenerationTileFramingAndDebugActionsCommand.SetFrames(
                new TilePosition(actualAffectedRegion.StartX, actualAffectedRegion.StartY),
                frameNeighbors: true),
            new FixedFramingPort(actualAffectedRegion));
    Verify.That(framing.Accepted && framing.FramesApplied
        && framing.AffectedRegion == actualAffectedRegion,
        "The explicit framing action must carry the actual region returned by its effect owner.");

    var publisher = new WorldGenerationTileFrameRepairAdapter(
        host.Gateway, sections, worldRuntimeId);
    TileFrameRepairPublicationResult publication = await publisher.PublishAsync(framing);
    Verify.That(publication.Accepted && publication.PublishedDispatchCount == 4,
        "The framing adapter must publish one packet 11 for each affected section through Gateway.");

    (int X, int Y)[] expectedFirst = [(2, 2), (2, 3), (3, 2), (3, 3)];
    foreach ((int expectedX, int expectedY) in expectedFirst) {
      PacketMessage message = await ReadRequiredAsync(first, 11,
          "first interested TCP client packet 11");
      VerifyFramePacket(message.Get<TileFrameSectionPacket>(), expectedX, expectedY);
    }
    PacketMessage secondMessage = await ReadRequiredAsync(second, 11,
        "second interested TCP client packet 11");
    VerifyFramePacket(secondMessage.Get<TileFrameSectionPacket>(), 3, 3);
    await VerifyNoPacketAsync(first,
        "A peer subscribed to four repaired sections must receive exactly four packet-11 frames.");
    await VerifyNoPacketAsync(second,
        "A peer subscribed to one repaired section must receive exactly one packet-11 frame.");
  }

  private static void RegisterWorldHandlers(NetworkGatewayHost host, WorldDataPacket world,
      WorldSynchronizationPacketHandlers sections,
      WorldSynchronizationObservation observation) {
    host.Gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData,
        MaximumPerWindow: 1, MaximumBytesPerWindow: 1),
        new RecordingHandler<RequestWorldDataPacket>((context, _, _) => {
          var dispatch = new OutboundDispatch(world, PacketDispatchKind.Single,
              [context.Connection], allowedStages: NetworkSessionStage.AwaitSectionRequest);
          return ValueTask.FromResult(new PacketHandlingResult(true, [dispatch],
              nextStage: NetworkSessionStage.AwaitSectionRequest));
        }));
    host.Gateway.Register<SpawnTileDataPacket>(
        new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest,
            MaximumPerWindow: 1, MaximumBytesPerWindow: 9), sections);
    host.Gateway.Register<RequestSectionPacket>(
        new PacketPolicy(159, NetworkSessionStage.Active), sections);
    host.Gateway.Register(new PacketPolicy(12, NetworkSessionStage.Synchronizing,
        MaximumPerWindow: 1, MaximumBytesPerWindow: 15),
        new PlayerSpawnPacketHandler(observation));
  }

  private static async Task JoinAsync(IPacketConnection client, NetworkGatewayHost host,
      WorldDataPacket expectedWorld) {
    await client.WritePacketAsync(new HelloPacket { Version = host.Profile.HelloVersion });
    PacketMessage admission = await ReadRequiredAsync(client, 3, "packet 3 admission");
    Verify.That(admission.Get<PlayerInfoPacket>().Accepted == false,
        "The TCP test client must be bound by normal gateway admission.");

    await client.WritePacketAsync(new RequestWorldDataPacket());
    PacketMessage initialWorld = await ReadRequiredAsync(client, 7, "packet 7 after packet 6");
    Verify.That(initialWorld.Get<WorldDataPacket>().WorldId == expectedWorld.WorldId,
        "Packet 6 must return the configured world snapshot before section transfer.");

    await client.WritePacketAsync(new SpawnTileDataPacket { X = -1, Y = -1 });
    PacketMessage repeatedWorld = await ReadRequiredAsync(client, 7,
        "packet 7 at the start of packet 8");
    Verify.That(repeatedWorld.Get<WorldDataPacket>().WorldId == expectedWorld.WorldId,
        "Packet 8 must begin with the world snapshot.");
    PacketMessage progressMessage = await ReadRequiredAsync(client, 9,
        "packet 9 initial section progress");
    StatusTextSizePacket progress = progressMessage.Get<StatusTextSizePacket>();
    Verify.That(progress.Value == 6,
        "The fixture's default spawn transfer must publish six initial sections.");
    for (int index = 0; index < progress.Value; index++) {
      _ = await ReadRequiredAsync(client, 10, "packet 10 initial section");
    }
    _ = await ReadRequiredAsync(client, 49, "packet 49 initial spawn");

    await client.WritePacketAsync(new PlayerSpawnPacket());
    _ = await ReadRequiredAsync(client, 129, "packet 129 finished connecting");
    await Verify.EventuallyAsync(() => host.Gateway.Sessions.Count(session =>
        session.Stage == NetworkSessionStage.Active) >= 1,
        "The TCP client must become Active before section interest requests.");
  }

  private static async Task RequestSectionAsync(IPacketConnection client, int sectionX,
      int sectionY) {
    await client.WritePacketAsync(new RequestSectionPacket {
      SectionX = checked((ushort)sectionX),
      SectionY = checked((ushort)sectionY)
    });
    _ = await ReadRequiredAsync(client, 9, "packet 9 section progress");
    TileSectionPacket section = (await ReadRequiredAsync(client, 10,
        "packet 10 requested section")).Get<TileSectionPacket>();
    Verify.That(section.StartX == sectionX * 200 && section.StartY == sectionY * 150,
        "The requested packet 10 must correspond to the section added to session interest.");
  }

  private static async Task<PacketMessage> ReadRequiredAsync(IPacketConnection client,
      byte expectedId, string phase) {
    PacketMessage? message = await client.ReadPacketAsync().AsTask()
        .WaitAsync(PacketTimeout);
    if (message is null) {
      throw new InvalidOperationException($"TCP peer closed before {phase}.");
    }
    Verify.That(message.MessageId == expectedId,
        $"Expected packet {expectedId} during {phase}, received {message.MessageId}.");
    return message;
  }

  private static void VerifyFramePacket(TileFrameSectionPacket packet, int sectionX,
      int sectionY) {
    Verify.That(packet.X == sectionX && packet.Y == sectionY
        && packet.Width == sectionX && packet.Height == sectionY,
        "Packet 11 must encode its one-section start and inclusive end coordinates.");
  }

  private static async Task VerifyNoPacketAsync(IPacketConnection client, string message) {
    using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
    try {
      PacketMessage? extra = await client.ReadPacketAsync(timeout.Token).AsTask();
      throw new InvalidOperationException(extra is null
          ? message + " The TCP connection closed unexpectedly."
          : message + $" Unexpected packet {extra.MessageId} arrived.");
    } catch (OperationCanceledException) when (timeout.IsCancellationRequested) {
    }
  }

  private sealed class FixedFramingPort(
      WorldGenerationTileFramingAndDebugSystem.TileFrameRegion region)
      : WorldGenerationTileFramingAndDebugSystem.IFramingPort {
    public WorldGenerationTileFramingAndDebugSystem.TileFrameRegion Frame(
        TilePosition target, bool frameNeighbors) => region;
  }
}
