using System.Net;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Simulation;
using Terraria.Relationships;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace NSSLC.NetworkVerification;

internal static class WorldTimePacketVerification
{
  private static readonly TimeSpan PacketTimeout = TimeSpan.FromSeconds(5);

  public static async Task RunAsync()
  {
    await VerifyOwnerProjectionAsync();
    await VerifyTcpPublicationAsync();
  }

  private static async Task VerifyOwnerProjectionAsync()
  {
    await using var owner = new NetworkWorldOwner(() => new LoadedWorldSession());
    await owner.Ready.WaitAsync(PacketTimeout);
    EntityRuntimeId runtimeId = await owner.InvokeAsync(session => session.EntityRuntime.RuntimeId);
    var gateway = new PacketGateway(
        ServerProtocolProfile.Create(
            TerrariaProtocolProfile.Create(GeneratedProfileVerification.CreateFacts())),
        new RecordingAuthority(), worldRuntimeIdProvider: () => runtimeId);
    await using (gateway.ConfigureAwait(false))
    {
      var producer = new WorldTimePacketProducer(gateway, owner, runtimeId);
      await owner.InvokeAsync(session => {
        WorldTimeWeatherState state = session.World.TimeWeather;
        state.DayTime = true;
        state.Time = 54_000;
        state.SunModY = short.MinValue;
        state.MoonModY = short.MaxValue;
        return 0;
      });
      SetTimePacket day = await producer.CaptureAsync();
      Verify.That(day.DayTime == 1 && day.Time == 54_000
          && day.SunModY == short.MinValue && day.MoonModY == short.MaxValue,
          "Packet 18 must project the authoritative daytime boundary and signed offsets.");
      WorldClockSnapshotValue daySnapshot = await owner.InvokeAsync(session =>
          WorldClockSnapshotValue.Capture(session.World.TimeWeather));
      Verify.That(daySnapshot.DayTime && daySnapshot.Time == 54_000
          && daySnapshot.SunModY == short.MinValue && daySnapshot.MoonModY == short.MaxValue,
          "The clock snapshot must preserve both authoritative celestial offsets.");

      await owner.InvokeAsync(session => {
        WorldTimeWeatherState state = session.World.TimeWeather;
        state.DayTime = false;
        state.Time = 32_400;
        state.SunModY = -17;
        state.MoonModY = 23;
        return 0;
      });
      WorldClockSnapshotValue nightSnapshot = await owner.InvokeAsync(session =>
          WorldClockSnapshotValue.Capture(session.World.TimeWeather));
      SetTimePacket night = await producer.CaptureAsync();
      SetTimePacket repeat = await producer.CaptureAsync();
      WorldClockSnapshotValue repeatedSnapshot = await owner.InvokeAsync(session =>
          WorldClockSnapshotValue.Capture(session.World.TimeWeather));
      Verify.That(night.DayTime == 0 && night.Time == 32_400
          && night.SunModY == -17 && night.MoonModY == 23
          && repeat.DayTime == night.DayTime && repeat.Time == night.Time
          && repeat.SunModY == night.SunModY && repeat.MoonModY == night.MoonModY
          && repeatedSnapshot == nightSnapshot,
          "Repeated packet 18 snapshots must be deterministic without mutating clock state.");

      var staleProducer = new WorldTimePacketProducer(gateway, owner,
          new EntityRuntimeId(Guid.NewGuid()));
      Verify.That(!await staleProducer.PublishAsync(),
          "A producer captured for an old world runtime must not publish packet 18.");

      await owner.InvokeAsync(session => session.World.TimeWeather.Time = -1);
      await Verify.ThrowsAsync<InvalidOperationException>(() => producer.CaptureAsync().AsTask());
      await owner.InvokeAsync(session => session.World.TimeWeather.Time = double.NaN);
      await Verify.ThrowsAsync<InvalidOperationException>(() => producer.CaptureAsync().AsTask());
      await owner.InvokeAsync(session => {
        session.World.TimeWeather.DayTime = true;
        session.World.TimeWeather.Time = WorldSimulationClockSystem.DayLength + 1;
        return 0;
      });
      await Verify.ThrowsAsync<InvalidOperationException>(() => producer.CaptureAsync().AsTask());
      await owner.InvokeAsync(session => {
        session.World.TimeWeather.DayTime = false;
        session.World.TimeWeather.Time = WorldSimulationClockSystem.NightLength + 1;
        return 0;
      });
      await Verify.ThrowsAsync<InvalidOperationException>(() => producer.CaptureAsync().AsTask());
    }
  }

  private static async Task VerifyTcpPublicationAsync()
  {
    ProtocolProfile profile = ServerProtocolProfile.Create(
        TerrariaProtocolProfile.Create(GeneratedProfileVerification.CreateFacts()));
    Verify.Throws<PacketProtocolException>(
        () => profile.Find(PacketDirection.ClientToServer, (byte)18));

    EntityRuntimeId runtimeId;
    await using var worldOwner = new NetworkWorldOwner(() => new LoadedWorldSession());
    await worldOwner.Ready.WaitAsync(PacketTimeout);
    runtimeId = await worldOwner.InvokeAsync(session => {
      session.World.TimeWeather.DayTime = true;
      session.World.TimeWeather.Time = 1234;
      session.World.TimeWeather.SunModY = -4;
      session.World.TimeWeather.MoonModY = 8;
      return session.EntityRuntime.RuntimeId;
    });

    var world = new WorldDataPacket {
      MaxTilesX = 420,
      MaxTilesY = 240,
      SpawnTileX = 100,
      SpawnTileY = 80,
      WorldId = 18,
      WorldName = "packet-18",
      WorldGuid = Guid.NewGuid().ToByteArray(),
      WorldGeneratorVersion = 1
    };
    var authority = new RecordingAuthority();
    await using var host = new NetworkGatewayHost(IPAddress.Loopback, 0, profile, authority,
        new PacketGatewayOptions {
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
    var initialSynchronization = new WorldSpawnSynchronizationPacketHandler(sections,
        new WorldTimePacketProducer(host.Gateway, worldOwner, runtimeId));
    host.Gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
        new RecordingHandler<RequestWorldDataPacket>((context, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, [new OutboundDispatch(world, PacketDispatchKind.Single,
                [context.Connection], allowedStages: NetworkSessionStage.AwaitSectionRequest)],
                nextStage: NetworkSessionStage.AwaitSectionRequest))));
    host.Gateway.Register<SpawnTileDataPacket>(
        new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest), initialSynchronization);
    PlayerLifecyclePacketRegistration.Register(host.Gateway, lifecycle);
    var producer = new WorldTimePacketProducer(host.Gateway, worldOwner, runtimeId);
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
    SetTimePacket initial = (await ReadRequiredAsync(client, 18,
        "packet 18 initial synchronization")).Get<SetTimePacket>();
    Verify.That(initial.Time == 1234 && initial.SunModY == -4 && initial.MoonModY == 8,
        "Initial world synchronization must include the authoritative packet-18 snapshot.");
    await client.WritePacketAsync(new PlayerSpawnPacket());
    _ = await ReadRequiredAsync(client, 129, "packet 129 active transition");
    await Verify.EventuallyAsync(() => host.Gateway.Sessions.Single().Stage
        == NetworkSessionStage.Active, "The TCP client must become Active before packet 18.");

    Verify.That(await producer.PublishAsync(),
        "The current world runtime must publish packet 18 through the gateway.");
    SetTimePacket packet = (await ReadRequiredAsync(client, 18, "packet 18 publication"))
        .Get<SetTimePacket>();
    Verify.That(packet.DayTime == 1 && packet.Time == 1234
        && packet.SunModY == -4 && packet.MoonModY == 8,
        "A real TCP client must receive packet 18 from the authoritative world owner.");

    await client.DisposeAsync();
    await Verify.EventuallyAsync(() => host.Gateway.Sessions.Count == 0,
        "A closed TCP client must leave the active world session set.");
    Verify.That(await producer.PublishAsync() && host.Gateway.Sessions.Count == 0,
        "Publishing packet 18 after client shutdown must not retain or target the closed session.");
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
