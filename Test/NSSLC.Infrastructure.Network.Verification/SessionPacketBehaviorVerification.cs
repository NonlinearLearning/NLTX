using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace NSSLC.NetworkVerification;

internal static class SessionPacketBehaviorVerification {
  public static async Task RunAsync() {
    await RunDirectValidationAsync();
    await RunRealTcpValidationAsync();
  }

  private static async Task RunDirectValidationAsync() {
    RealSessionHost.VerifyWorldDataProjection();
    var registry = new SessionClientUuidRegistry();
    var handler = new SessionClientUuidPacketHandler(registry);
    ConnectionIdentity connection = new(Guid.NewGuid(), 1);
    var context = new NetworkSessionContext(connection, "test-profile",
        NetworkSessionStage.AwaitPlayerData, new SenderBinding(3, Guid.NewGuid()), false);

    PacketHandlingResult accepted = await handler.HandleAsync(context,
        new Unknown68Packet { ClientUuid = "client-uuid-68" }, CancellationToken.None);
    Verify.That(accepted.Accepted && accepted.Outbound.Count == 0
        && registry.TryGet(connection, out string? stored)
        && stored == "client-uuid-68",
        "Packet 68 must preserve a bounded client UUID without creating an outbound gameplay packet.");

    PacketHandlingResult duplicate = await handler.HandleAsync(context,
        new Unknown68Packet { ClientUuid = "client-uuid-68" }, CancellationToken.None);
    Verify.That(!duplicate.Accepted && duplicate.RejectionCode == "DuplicateClientUuid"
        && registry.Snapshot().Count == 1,
        "Packet 68 must not partially overwrite an already recorded connection identity.");

    PacketHandlingResult empty = await handler.HandleAsync(context,
        new Unknown68Packet { ClientUuid = "   " }, CancellationToken.None);
    Verify.That(!empty.Accepted && empty.RejectionCode == "ClientUuidRequired",
        "An empty packet-68 UUID must be rejected before recording state.");

    PacketHandlingResult nullByte = await handler.HandleAsync(context,
        new Unknown68Packet { ClientUuid = "valid\0uuid" }, CancellationToken.None);
    Verify.That(!nullByte.Accepted && nullByte.RejectionCode == "ClientUuidContainsNull",
        "A packet-68 UUID containing a NUL must be rejected before recording state.");

    PacketHandlingResult tooLong = await handler.HandleAsync(context,
        new Unknown68Packet { ClientUuid = new string('x', 129) }, CancellationToken.None);
    Verify.That(!tooLong.Accepted && tooLong.RejectionCode == "ClientUuidTooLong",
        "An overlong packet-68 UUID must be rejected before recording state.");

    Verify.That(registry.Remove(connection) && !registry.TryGet(connection, out _),
        "Disconnect cleanup must be able to remove the UUID associated with the old connection epoch.");
  }

  private static async Task RunRealTcpValidationAsync() {
    await VerifySteamWorldDataOverTcpAsync();
    await VerifyPasswordAdmissionOverRealTcpAsync();
    await VerifySuccessfulHostAuthorizationAsync();
    await VerifyFailedHostAuthorizationKeepsSessionAsync();
    await VerifyUnconfiguredHostAuthorizationKeepsSessionAsync();
    await VerifyRealTcpUuidCleanupAsync();
    await VerifyReverseHostResultIsRejectedAsync();
  }

  private static async Task VerifySteamWorldDataOverTcpAsync() {
    await using var authorityAndHost = await RealSessionHost.StartAsync(null, steamProfile: true);
    await using IPacketConnection client = await ConnectAndAdmitAsync(authorityAndHost);
    await AdvanceToActiveAsync(client, authorityAndHost);
    Verify.That(authorityAndHost.Host.Gateway.Sessions.Single().Stage
        == NetworkSessionStage.Active,
        "Steam 326 packet 7 with dungeon coordinates must complete real TCP synchronization.");
  }

  private static async Task VerifyPasswordAdmissionOverRealTcpAsync() {
    await using var authorityAndHost = await RealSessionHost.StartAsync(null,
        password: "correct-password");
    await using (IPacketConnection wrong = await authorityAndHost.Host.Connections.ConnectAsync(
        "127.0.0.1", authorityAndHost.Port, TimeSpan.FromSeconds(5))) {
      await wrong.WritePacketAsync(new HelloPacket { Version = "Terraria319" });
      PacketMessage request = await ReadRequiredAsync(wrong, authorityAndHost.Host.Gateway,
          "packet 37 password request");
      Verify.That(request.MessageId == 37 && request.Get<RequestPasswordPacket>() is not null,
          "A password protected formal TCP session must request packet 37 before admission.");

      await wrong.WritePacketAsync(new SendPasswordPacket { Password = "wrong-password" });
      PacketMessage rejected = await ReadRequiredAsync(wrong, authorityAndHost.Host.Gateway,
          "wrong password rejection");
      Verify.That(rejected.MessageId == 2,
          "A wrong formal TCP password must receive a Kick packet before EOF.");
      PacketMessage? eof = await wrong.ReadPacketAsync().AsTask()
          .WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(eof is null,
          "A wrong formal TCP password must close the connection after the Kick packet.");
    }

    await Verify.EventuallyAsync(
        () => authorityAndHost.Host.Gateway.Sessions.Count == 0
            && authorityAndHost.Authority.ActiveBindings == 0,
        "A wrong formal TCP password must not retain a player slot.");
    Verify.That(authorityAndHost.Authority.AdmissionCount == 0,
        "A wrong formal TCP password must not increment the authority admission count.");

    await using IPacketConnection correct = await ConnectAndAdmitAsync(authorityAndHost,
        password: "correct-password");
    Verify.That(authorityAndHost.Authority.AdmissionCount == 1
        && authorityAndHost.Authority.ActiveBindings == 1
        && authorityAndHost.Host.Gateway.Sessions.Single().Binding?.PlayerSlot == 0,
        "A correct formal TCP password must allocate exactly one authority-owned slot.");
  }

  private static async Task VerifySuccessfulHostAuthorizationAsync() {
    await using var authorityAndHost = await RealSessionHost.StartAsync("host-secret");
    await using IPacketConnection client = await ConnectAndAdmitAsync(authorityAndHost);
    await client.WritePacketAsync(new HostTokenPacket { HostToken = "host-secret" });

    PacketMessage hostResult = await ReadRequiredAsync(client);
    SetCountsAsHostForGameplayPacket hostPacket = hostResult.Get<SetCountsAsHostForGameplayPacket>();
    Verify.That(hostResult.MessageId == 139 && hostPacket.Player == 0
        && hostPacket.CountsAsHost,
        "A correct host token must produce exactly one server-authored packet 139 for the allocated slot.");

    await AdvanceToActiveAsync(client, authorityAndHost);
    Verify.That(authorityAndHost.Authority.ActiveBindings == 1
        && authorityAndHost.Host.Gateway.Sessions.Single().Stage == NetworkSessionStage.Active,
        "A correctly authorized TCP session must continue through 6, 8 and 12 to Active.");
    Verify.That(ReadDiagnostics(authorityAndHost.Host.Gateway)
        .All(diagnostic => diagnostic.Code != "HostAuthorizationDenied"),
        "A correct host token must not record a host authorization denial.");
  }

  private static async Task VerifyFailedHostAuthorizationKeepsSessionAsync() {
    await using var authorityAndHost = await RealSessionHost.StartAsync("host-secret");
    await using IPacketConnection client = await ConnectAndAdmitAsync(authorityAndHost);
    await client.WritePacketAsync(new HostTokenPacket { HostToken = "wrong-secret" });
    await AssertNoServerPacketAsync(client,
        "An incorrect host token must not produce 139 or a Kick packet.");

    Verify.That(authorityAndHost.Authority.ActiveBindings == 1
        && authorityAndHost.Host.Gateway.Sessions.Single().Stage == NetworkSessionStage.AwaitPlayerData,
        "An incorrect host token must keep the currently bound session alive in AwaitPlayerData.");
    await AdvanceToActiveAsync(client, authorityAndHost);
    Verify.That(ReadDiagnostics(authorityAndHost.Host.Gateway)
        .Any(diagnostic => diagnostic.Code == "HostAuthorizationDenied"
            && diagnostic.MessageId == 161),
        "An incorrect host token must record HostAuthorizationDenied for packet 161.");
    Verify.That(authorityAndHost.Authority.ActiveBindings == 1,
        "A denied host token must not release the valid player binding.");
  }

  private static async Task VerifyUnconfiguredHostAuthorizationKeepsSessionAsync() {
    await using var authorityAndHost = await RealSessionHost.StartAsync(null);
    await using IPacketConnection client = await ConnectAndAdmitAsync(authorityAndHost);
    await client.WritePacketAsync(new HostTokenPacket { HostToken = "arbitrary-client-token" });
    await AssertNoServerPacketAsync(client,
        "An unconfigured host token must not produce 139 or a Kick packet.");

    Verify.That(authorityAndHost.Authority.ActiveBindings == 1,
        "An unconfigured host token must leave the valid binding in place.");
    await AdvanceToActiveAsync(client, authorityAndHost);
    Verify.That(ReadDiagnostics(authorityAndHost.Host.Gateway)
        .Any(diagnostic => diagnostic.Code == "HostAuthorizationDenied"
            && diagnostic.MessageId == 161),
        "An unconfigured host token must record HostAuthorizationDenied for packet 161.");
  }

  private static async Task VerifyRealTcpUuidCleanupAsync() {
    await using var authorityAndHost = await RealSessionHost.StartAsync(null);
    await using (IPacketConnection client = await ConnectAndAdmitAsync(authorityAndHost)) {
      ConnectionIdentity serverIdentity = authorityAndHost.Host.Gateway.Sessions.Single().Identity;
      authorityAndHost.Remember(serverIdentity);
      await client.WritePacketAsync(new Unknown68Packet { ClientUuid = "client-uuid-68-tcp" });
      await Verify.EventuallyAsync(
          () => authorityAndHost.Authority.ClientUuids.TryGet(serverIdentity, out string? value)
              && value == "client-uuid-68-tcp",
          "A real TCP packet 68 was not retained by the formal authority registry.");
      Verify.That(authorityAndHost.Host.Gateway.Sessions.Single().Stage
          == NetworkSessionStage.AwaitPlayerData,
          "Packet 68 must not advance the session or create an outbound packet.");
    }

    await Verify.EventuallyAsync(
        () => authorityAndHost.Host.Gateway.Sessions.Count == 0
            && authorityAndHost.Authority.ActiveBindings == 0,
        "Disconnect did not release the formal authority binding after packet 68.");
    Verify.That(!authorityAndHost.Authority.ClientUuids.TryGet(
        authorityAndHost.LastIdentity, out _),
        "Formal authority cleanup must remove the UUID for the disconnected connection epoch.");
  }

  private static async Task VerifyReverseHostResultIsRejectedAsync() {
    await using var authorityAndHost = await RealSessionHost.StartAsync("host-secret");
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, authorityAndHost.Port);
    NetworkStream stream = client.GetStream();
    byte[] hello = authorityAndHost.Host.Profile
        .Find(PacketDirection.ClientToServer, typeof(HelloPacket))
        .Encode(new HelloPacket { Version = "Terraria319" });
    await stream.WriteAsync(hello);
    byte[] playerInfo = await ReadFrameAsync(stream);
    Verify.That(playerInfo[2] == 3,
        "The raw TCP direction probe must complete Hello before sending packet 139.");

    byte[] reverseHostResult = new byte[5];
    BinaryPrimitives.WriteUInt16LittleEndian(reverseHostResult, 5);
    reverseHostResult[2] = 139;
    reverseHostResult[3] = 0;
    reverseHostResult[4] = 1;
    await stream.WriteAsync(reverseHostResult);

    using var closeTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    int read = await stream.ReadAsync(new byte[1], closeTimeout.Token);
    Verify.That(read == 0,
        "A client-to-server packet 139 must be rejected and close the raw TCP session.");
    await Verify.EventuallyAsync(
        () => authorityAndHost.Host.Gateway.Sessions.Count == 0
            && authorityAndHost.Authority.ActiveBindings == 0,
        "Reverse packet 139 rejection did not release the formal authority binding.");
    Verify.That(ReadDiagnostics(authorityAndHost.Host.Gateway)
        .Any(diagnostic => diagnostic.Code == "AdmissionRejected"
            && diagnostic.MessageId == 139),
        "Reverse packet 139 must be rejected at pre-decode admission.");
  }

  private static async Task<IPacketConnection> ConnectAndAdmitAsync(RealSessionHost host,
      string? password = null) {
    IPacketConnection client = await host.Host.Connections.ConnectAsync(
        "127.0.0.1", host.Port, TimeSpan.FromSeconds(5));
    await client.WritePacketAsync(new HelloPacket { Version = host.Host.Profile.HelloVersion });
    if (host.Authority.RequiresPassword) {
      PacketMessage request = await ReadRequiredAsync(client, host.Host.Gateway,
          "packet 37 password request");
      Verify.That(request.MessageId == 37,
          "A password protected formal TCP session must request packet 37.");
      await client.WritePacketAsync(new SendPasswordPacket { Password = password ?? string.Empty });
    }
    PacketMessage admitted = await ReadRequiredAsync(client, host.Host.Gateway,
        "packet 3 player info");
    PlayerInfoPacket playerInfo = admitted.Get<PlayerInfoPacket>();
    Verify.That(admitted.MessageId == 3 && playerInfo.Player == 0,
        "A formal TCP session must receive PlayerInfo for the authority-allocated slot.");
    return client;
  }

  private static async Task AdvanceToActiveAsync(IPacketConnection client,
      RealSessionHost authorityAndHost) {
    NetworkGatewayHost host = authorityAndHost.Host;
    await client.WritePacketAsync(new RequestWorldDataPacket());
    PacketMessage worldMessage = await ReadRequiredAsync(client, host.Gateway,
        "packet 7 world data after packet 6");
    WorldDataPacket worldData = worldMessage.Get<WorldDataPacket>();
    if (host.Profile.HelloVersion == "Terraria326") {
      Verify.That(worldData.DungeonX == 3 && worldData.DungeonY == 4,
          "The formal Steam TCP world response must preserve the loaded dungeon location.");
    }
    Verify.That(worldMessage.MessageId == 7 && worldData.MaxTilesX == 20
        && worldData.MaxTilesY == 20 && worldData.SpawnTileX == 10
        && worldData.SpawnTileY == 10,
        "Formal packet 6 handling must return the configured world dimensions and spawn point.");

    await client.WritePacketAsync(new SpawnTileDataPacket { X = -1, Y = -1 });
    PacketMessage repeatedWorld = await ReadRequiredAsync(client, host.Gateway,
        "packet 7 world snapshot after packet 8");
    if (host.Profile.HelloVersion == "Terraria326") {
      WorldDataPacket repeatedData = repeatedWorld.Get<WorldDataPacket>();
      Verify.That(repeatedData.DungeonX == 3 && repeatedData.DungeonY == 4,
          "Steam's repeated packet 7 before sections must retain both dungeon coordinates.");
    }
    PacketMessage progress = await ReadRequiredAsync(client, host.Gateway,
        "packet 9 progress after packet 8");
    PacketMessage section = await ReadRequiredAsync(client, host.Gateway,
        "packet 10 tile section after packet 8");
    PacketMessage initialSpawn = await ReadRequiredAsync(client, host.Gateway,
        "packet 49 initial spawn after packet 8");
    Verify.That(repeatedWorld.MessageId == 7
        && repeatedWorld.Get<WorldDataPacket>().MaxTilesX == 20
        && progress.MessageId == 9 && section.MessageId == 10
        && initialSpawn.MessageId == 49 && section.Get<TileSectionPacket>().Width == 20
        && section.Get<TileSectionPacket>().Height == 20,
        "Formal packet 8 handling must send progress, the requested tile section and packet 49.");

    await client.WritePacketAsync(new PlayerSpawnPacket());
    PacketMessage finished = await ReadRequiredAsync(client, host.Gateway,
        "packet 129 after packet 12");
    Verify.That(finished.MessageId == 129,
        "Formal packet 12 handling must send the server's packet 129 completion marker.");
    await Verify.EventuallyAsync(
        () => host.Gateway.Sessions.Count == 1
            && host.Gateway.Sessions[0].Stage == NetworkSessionStage.Active,
        "A formal TCP session did not progress through 6, 8 and 12 to Active.");
    Verify.That(authorityAndHost.Observation.PlayerSpawns.Any(spawn => spawn.PlayerSlot == 0
        && spawn.SpawnContext == 0),
        "The formal packet 12 owner must record the authoritative player spawn observation.");
  }

  private static async Task<PacketMessage> ReadRequiredAsync(IPacketConnection client,
      PacketGateway? gateway = null, string? phase = null) {
    PacketMessage? message = await client.ReadPacketAsync().AsTask()
        .WaitAsync(TimeSpan.FromSeconds(5));
    if (message is not null) {
      return message;
    }
    string diagnostics = gateway is null ? "none" : FormatDiagnostics(gateway);
    string label = phase ?? "the required packet";
    throw new InvalidOperationException(
        $"The formal TCP peer closed before {label}. Gateway diagnostics: {diagnostics}.");
  }

  private static string FormatDiagnostics(PacketGateway gateway) {
    IReadOnlyList<PacketGatewayDiagnostic> diagnostics = ReadDiagnostics(gateway);
    if (diagnostics.Count == 0) {
      return "none";
    }
    return string.Join(", ", diagnostics.Select(diagnostic =>
        $"{diagnostic.Code}(id={diagnostic.MessageId?.ToString() ?? "-"},"
        + $"body={diagnostic.BodyLength?.ToString() ?? "-"},"
        + $"offset={diagnostic.BodyOffset?.ToString() ?? "-"},"
        + $"certainty={diagnostic.SendCertainty?.ToString() ?? "-"})"));
  }

  private static async Task AssertNoServerPacketAsync(IPacketConnection client, string message) {
    using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
    try {
      PacketMessage? packet = await client.ReadPacketAsync(timeout.Token).AsTask();
      throw new InvalidOperationException(packet is null
          ? message + " The connection closed unexpectedly."
          : message + $" Received packet {packet.MessageId}.");
    } catch (OperationCanceledException) when (timeout.IsCancellationRequested) {
    }
  }

  private static async Task<byte[]> ReadFrameAsync(NetworkStream stream) {
    byte[] header = new byte[2];
    await ReadExactlyAsync(stream, header);
    int length = BinaryPrimitives.ReadUInt16LittleEndian(header);
    Verify.That(length >= 3, "A TCP frame must include its length, message ID and body.");
    byte[] frame = new byte[length];
    header.CopyTo(frame, 0);
    await ReadExactlyAsync(stream, frame.AsMemory(2));
    return frame;
  }

  private static async Task ReadExactlyAsync(NetworkStream stream, Memory<byte> buffer) {
    int offset = 0;
    while (offset < buffer.Length) {
      int read = await stream.ReadAsync(buffer[offset..]);
      if (read == 0) {
        throw new EndOfStreamException("The TCP peer closed while reading a complete frame.");
      }
      offset += read;
    }
  }

  private static IReadOnlyList<PacketGatewayDiagnostic> ReadDiagnostics(PacketGateway gateway) {
    var diagnostics = new List<PacketGatewayDiagnostic>();
    while (gateway.TryReadDiagnostic(out PacketGatewayDiagnostic? diagnostic)) {
      diagnostics.Add(diagnostic!);
    }
    return diagnostics;
  }

  private sealed class RealSessionHost : IAsyncDisposable {
    public PlayerSlotSessionAuthority Authority { get; }
    public NetworkGatewayHost Host { get; }
    public WorldSynchronizationObservation Observation { get; }
    public int Port => ((IPEndPoint)Host.EndPoint).Port;
    public ConnectionIdentity LastIdentity { get; private set; }

    private RealSessionHost(PlayerSlotSessionAuthority authority, NetworkGatewayHost host,
        WorldSynchronizationObservation observation) {
      Authority = authority;
      Host = host;
      Observation = observation;
    }

    public void Remember(ConnectionIdentity identity) {
      LastIdentity = identity;
    }

    public static async Task<RealSessionHost> StartAsync(string? hostToken,
        string? password = null, bool steamProfile = false) {
      ProtocolProfile profile = ServerProtocolProfile.Create(
          steamProfile
              ? SteamProtocolProfile.Create(GeneratedProfileVerification.CreateFacts(steamModules: true))
              : TerrariaProtocolProfile.Create(GeneratedProfileVerification.CreateFacts()));
      EntityRuntimeId worldRuntimeId = new(Guid.NewGuid());
      var authority = new PlayerSlotSessionAuthority(4, password: password,
          hostToken: hostToken);
      var host = new NetworkGatewayHost(IPAddress.Loopback, 0, profile, authority,
          new PacketGatewayOptions {
            MaximumSessions = 4,
            EnableHostAuthorization = true,
            StageTimeout = TimeSpan.FromSeconds(5),
            HandshakeTimeout = TimeSpan.FromSeconds(10),
            CleanupTimeout = TimeSpan.FromSeconds(5)
          }, worldRuntimeIdProvider: () => worldRuntimeId);
      const int worldSize = 20;
      WorldPersistenceDocument world = CreateMinimalWorldDocument(worldSize);
      WorldDataPacket worldData = WorldDataPacketProjection.Create(world);
      var controls = new PlayerControlsObservationStore();
      var observation = new WorldSynchronizationObservation();
      var synchronization = new WorldSynchronizationPacketHandlers(worldData,
          new TileMapSnapshot(worldSize, worldSize, new TileCellState[worldSize * worldSize]),
          new bool[1], controls, new TileBreakObservationStore(), observation,
          host.Gateway.IsCurrentSender);
      host.Gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData,
          MaximumPerWindow: 1, MaximumBytesPerWindow: 1),
          new WorldDataPacketHandler(world, host.Gateway.IsCurrentSender));
      host.Gateway.Register<SpawnTileDataPacket>(
          new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest,
              MaximumPerWindow: 1, MaximumBytesPerWindow: 9), synchronization);
      host.Gateway.Register(new PacketPolicy(12, NetworkSessionStage.Synchronizing,
          MaximumPerWindow: 1, MaximumBytesPerWindow: 15),
          new PlayerSpawnPacketHandler(observation));
      host.Gateway.Register<Unknown68Packet>(
          new PacketPolicy(68, NetworkSessionStage.AwaitPlayerData,
              MaximumPerWindow: 1, MaximumBytesPerWindow: 132),
          new SessionClientUuidPacketHandler(authority.ClientUuids));
      host.Start();
      await Task.Yield();
      return new RealSessionHost(authority, host, observation);
    }

    public static void VerifyWorldDataProjection() {
      WorldPersistenceDocument original = CreateMinimalWorldDocument(20);
      byte[] expectedFlags = [0x02, 0x20, 0x40, 0x80];
      for (int index = 0; index < expectedFlags.Length; index++) {
        var boss = new WorldFileBossProgressionSection(
            fastForwardTimeToDawn: index == 0, downedFishron: index == 1,
            downedMartians: index == 2, downedAncientCultist: index == 3,
            downedMoonlord: false, downedHalloweenKing: false, downedHalloweenTree: false,
            downedChristmasIceQueen: false, downedChristmasSantank: false,
            downedChristmasTree: false);
        WorldPersistenceDocument updated = original.WithReplacements(
            WorldPersistenceSection.Create(WorldFileBossProgressionSection.SectionId, boss));
        WorldDataPacket projected = WorldDataPacketProjection.Create(updated);
        Verify.That(projected.WorldFlagGroups[2] == expectedFlags[index],
            "Packet 7 must preserve Steam's reserved bit 0 and exact boss flag positions.");
        Verify.That(WorldDataPacketProjection.Create(original).WorldFlagGroups[2] == 0,
            "Projecting a new document must not mutate the previous world snapshot.");
      }

      byte[] expectedSeedFlags = [0x02, 0x04, 0x08, 0x10];
      ProtocolFacts facts = GeneratedProfileVerification.CreateFacts(steamModules: true);
      ProtocolProfile steam = SteamProtocolProfile.Create(facts);
      PacketBinding steamBinding = steam.Find(PacketDirection.ServerToClient, (byte)7);
      PacketBinding legacyBinding = TerrariaProtocolProfile.Create(facts)
          .Find(PacketDirection.ServerToClient, (byte)7);
      for (int index = 0; index < expectedSeedFlags.Length; index++) {
        var policy = new WorldFileTimePolicySection(false, 0, index == 0, index == 1,
            false, false, 0, 0, false);
        var spawn = new WorldFileSpawnSection([], false, index == 2, index == 3, null);
        WorldPersistenceDocument updated = original.WithReplacements(
            WorldPersistenceSection.Create(WorldFileTimePolicySection.SectionId, policy),
            WorldPersistenceSection.Create(WorldFileSpawnSection.SectionId, spawn));
        WorldDataPacket projected = WorldDataPacketProjection.Create(updated);
        byte[] frame = steamBinding.Encode(projected);
        var decoded = (WorldDataPacket)steamBinding.Decode(frame.AsMemory(3));
        Verify.That(decoded.WorldFlagGroups[10] == expectedSeedFlags[index]
            && decoded.DungeonX == 3 && decoded.DungeonY == 4,
            "Steam packet 7 must carry seasonal/lightning state and dungeon coordinates.");
        Verify.That(frame.Length == legacyBinding.Encode(projected).Length + 4
            && BinaryPrimitives.ReadInt16LittleEndian(frame.AsSpan(frame.Length - 4)) == 3
            && BinaryPrimitives.ReadInt16LittleEndian(frame.AsSpan(frame.Length - 2)) == 4,
            "Steam's two signed dungeon coordinates must follow the extra-spawn table.");
        Verify.Throws<PacketProtocolException>(() =>
            steamBinding.Decode(frame.AsMemory(3, frame.Length - 4)));
        Verify.Throws<PacketProtocolException>(() =>
            steamBinding.Decode(frame.AsMemory(3, frame.Length - 7)));
        byte[] trailing = frame.AsSpan(3).ToArray().Append((byte)0).ToArray();
        Verify.Throws<PacketProtocolException>(() => steamBinding.Decode(trailing));
      }
    }

    private static WorldPersistenceDocument CreateMinimalWorldDocument(int size) {
      var sections = new WorldPersistenceSection[] {
        WorldPersistenceSection.Create(WorldFileHeaderSection.SectionId,
            new WorldFileHeaderSection("session-tcp-fixture", "session", 0, Guid.NewGuid(),
                1, 0, size, 0, size, size, size, 0, false, false, false, false, false, false,
                false, false, false, DateTime.UtcNow, DateTime.UtcNow)),
        WorldPersistenceSection.Create(WorldFileEnvironmentSection.SectionId,
            new WorldFileEnvironmentSection(0, new int[3], new int[4], new int[3], new int[4],
                0, 0, 0, size / 2, size / 2, size / 2, size * 3 / 4, 0, true, 0, false,
                false, 3, 4, false)),
        WorldPersistenceSection.Create(WorldFileProgressionSection.SectionId,
            WorldFileProgressionSection.Empty),
        WorldPersistenceSection.Create(WorldFileBossProgressionSection.SectionId,
            WorldFileBossProgressionSection.Empty),
        WorldPersistenceSection.Create(WorldFilePartySection.SectionId,
            WorldFilePartySection.Empty),
        WorldPersistenceSection.Create(WorldFileSandstormSection.SectionId,
            WorldFileSandstormSection.Empty),
        WorldPersistenceSection.Create(WorldFileDefenderEventSection.SectionId,
            WorldFileDefenderEventSection.Empty),
        WorldPersistenceSection.Create(WorldFileEventSection.SectionId,
            WorldFileEventSection.Empty),
        WorldPersistenceSection.Create(WorldFileSeasonalSection.SectionId,
            WorldFileSeasonalSection.Empty),
        WorldPersistenceSection.Create(WorldFileNpcUnlockSection.SectionId,
            WorldFileNpcUnlockSection.Empty),
        WorldPersistenceSection.Create(WorldFileTimePolicySection.SectionId,
            WorldFileTimePolicySection.Empty),
        WorldPersistenceSection.Create(WorldFileBackgroundSection.SectionId,
            WorldFileBackgroundSection.Empty),
         WorldPersistenceSection.Create(WorldFileTreeTopsSection.SectionId,
             new WorldFileTreeTopsSection(new int[13])),
        WorldPersistenceSection.Create(WorldFileSpawnSection.SectionId,
            WorldFileSpawnSection.Empty)
      };
      return new WorldPersistenceDocument(319, sections);
    }

    public ValueTask DisposeAsync() {
      return Host.DisposeAsync();
    }
  }
}
