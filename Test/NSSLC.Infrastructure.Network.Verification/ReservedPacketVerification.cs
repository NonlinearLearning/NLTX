using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class ReservedPacketVerification {
  private static readonly byte[] ClientSyncedInventoryFrame = [0x03, 0x00, 0x8A];

  public static async Task RunAsync() {
    await VerifyHandlerContractAsync();
    VerifyProductionDirections();
    await VerifyReservedTcpBehaviorAsync();
  }

  private static async Task VerifyHandlerContractAsync() {
    var handler = new ClientSyncedInventoryPacketHandler();
    var actor = new SenderBinding(7, Guid.NewGuid());
    var connection = new ConnectionIdentity(Guid.NewGuid(), 1);

    foreach (NetworkSessionStage stage in new[] {
      NetworkSessionStage.AwaitPlayerData, NetworkSessionStage.Active
    }) {
      foreach (bool isHost in new[] { false, true }) {
        var context = new NetworkSessionContext(connection, "reserved-profile", stage, actor,
            isHost);
        PacketHandlingResult accepted = await handler.HandleAsync(context,
            new ClientSyncedInventoryPacket(), CancellationToken.None);
        Verify.That(accepted.Accepted && accepted.Outbound.Count == 0
            && accepted.NextStage is null && accepted.Interest is null,
            "An empty ID 138 notice must not grant inventory, change stage/interest, or emit output.");
      }
    }

    var invalidContext = new NetworkSessionContext(connection, "reserved-profile",
        NetworkSessionStage.AwaitSectionRequest, actor, IsHost: true);
    PacketHandlingResult invalidStage = await handler.HandleAsync(invalidContext,
        new ClientSyncedInventoryPacket(), CancellationToken.None);
    Verify.That(!invalidStage.Accepted
        && invalidStage.RejectionCode == "InventorySyncMarkerRequiresBoundSession",
        "ID 138 must reject stages outside bound player data and active play.");

    PacketHandlingResult invalidBody = await handler.HandleAsync(
        new NetworkSessionContext(connection, "reserved-profile",
            NetworkSessionStage.AwaitPlayerData, actor, IsHost: false),
        new ClientSyncedInventoryPacket { Bytes = new byte[] { 1 } }, CancellationToken.None);
    Verify.That(!invalidBody.Accepted
        && invalidBody.RejectionCode == "InventorySyncMarkerMustBeEmpty"
        && invalidBody.Outbound.Count == 0 && invalidBody.NextStage is null,
        "ID 138 must reject a non-empty body before any state or output is produced.");
  }

  private static void VerifyProductionDirections() {
    ProtocolProfile wireProfile = GatewayVerification.CreateProfile();
    Verify.That(wireProfile.Bindings.Select(binding => binding.MessageId).Distinct().Count() == 162,
        "The raw generated catalog must retain all 162 unique message IDs.");
    ProtocolProfile productionProfile = ServerProtocolProfile.Create(wireProfile);

    PacketBinding rawZeroClient = wireProfile.Find(PacketDirection.ClientToServer, (byte)0);
    PacketBinding rawZeroServer = wireProfile.Find(PacketDirection.ServerToClient, (byte)0);
    var opaque = new NeverCalledPacket { Bytes = new byte[] { 0xA5, 0x5A } };
    byte[] opaqueFrame = rawZeroClient.Encode(opaque);
    var decodedOpaque = (NeverCalledPacket)rawZeroServer.Decode(opaqueFrame.AsMemory(3));
    Verify.That(decodedOpaque.Bytes.Span.SequenceEqual(opaque.Bytes.Span),
        "Raw protocol fixtures must retain the opaque ID 0 layout for catalog inspection.");
    Verify.Throws<PacketProtocolException>(() =>
        productionProfile.Find(PacketDirection.ClientToServer, (byte)0));
    Verify.Throws<PacketProtocolException>(() =>
        productionProfile.Find(PacketDirection.ServerToClient, (byte)0));

    foreach (byte id in new byte[] { 15, 25, 26, 44, 67, 83, 138 }) {
      _ = wireProfile.Find(PacketDirection.ClientToServer, id);
      _ = wireProfile.Find(PacketDirection.ServerToClient, id);
      _ = productionProfile.Find(PacketDirection.ClientToServer, id);
      Verify.Throws<PacketProtocolException>(() =>
          productionProfile.Find(PacketDirection.ServerToClient, id));
    }

    foreach (byte id in new byte[] { 15, 25, 26, 44, 67, 83 }) {
      byte[] frame = wireProfile.Find(PacketDirection.ClientToServer, id)
          .Encode(CreateNoEffectPacket(id));
      Verify.That(frame.SequenceEqual(new byte[] { 0x03, 0x00, id }),
          $"ID {id} must retain its strict empty-frame layout.");
      Verify.Throws<PacketProtocolException>(() =>
          wireProfile.Find(PacketDirection.ClientToServer, id).Decode(new byte[] { 1 }));
    }

    Verify.That(ReservedPacketRegistration.IsProductionDirectionEnabled(0,
            PacketDirection.ClientToServer) == false
        && ReservedPacketRegistration.IsProductionDirectionEnabled(0,
            PacketDirection.ServerToClient) == false,
        "ID 0 must be disabled in both production directions.");
    foreach (byte id in new byte[] { 15, 25, 26, 44, 67, 83, 138 }) {
      Verify.That(ReservedPacketRegistration.IsProductionDirectionEnabled(id,
              PacketDirection.ClientToServer)
          && !ReservedPacketRegistration.IsProductionDirectionEnabled(id,
              PacketDirection.ServerToClient),
          $"ID {id} must have an explicit C2S-only production direction.");
    }
  }

  private static async Task VerifyReservedTcpBehaviorAsync() {
    ProtocolProfile wireProfile = GatewayVerification.CreateProfile();
    ProtocolProfile profile = ServerProtocolProfile.Create(wireProfile);
    var authority = new PlayerSlotSessionAuthority(playerSlotCount: 2);
    var budget = new PacketByteBudget(2 * 1024 * 1024);
    await using var gateway = new PacketGateway(profile, authority,
        new PacketGatewayOptions { EnablePing = true });
    ReservedPacketRegistration.RegisterClientSyncedInventory(gateway);
    ReservedPacketRegistration.RegisterKnownNoEffectPackets(gateway);
    VerifyNoEffectRegistrationPolicies(gateway);
    // These are recording stage fixtures only; they do not represent world or spawn owners.
    GatewayVerification.RegisterProgression(gateway);
    await using PacketTcpServer server = gateway.CreateServer(IPAddress.Loopback, 0,
        budget: budget);
    server.Start();

    using (var client = new TcpClient()) {
      (NetworkStream stream, NetworkSession session, SenderBinding binding) =
          await ConnectAndAdmitAsync(client, server, gateway, profile);
      Verify.That(session.Stage == NetworkSessionStage.AwaitPlayerData
          && authority.AdmissionCount == 1 && authority.ActiveBindings == 1,
          "Hello must establish exactly one production player-slot binding before ID 138.");

      await WriteFrameAsync(stream, ClientSyncedInventoryFrame);
      await WriteFrameAsync(stream, ClientSyncedInventoryFrame);
      await WriteFrameAsync(stream, profile.Find(PacketDirection.ClientToServer,
          typeof(RequestWorldDataPacket)).Encode(new RequestWorldDataPacket()));
      await Verify.EventuallyAsync(() => gateway.Sessions.SingleOrDefault()?.Stage
          == NetworkSessionStage.AwaitSectionRequest,
          "A repeated literal ID 138 notice must leave the following legal ID 6 request processable.");
      Verify.That(session.Binding == binding && authority.AdmissionCount == 1
          && authority.ActiveBindings == 1,
          "ID 138 must retain the exact slot binding without admission or release increments.");

      await WriteFrameAsync(stream, profile.Find(PacketDirection.ClientToServer,
          typeof(SpawnTileDataPacket)).Encode(new SpawnTileDataPacket()));
      await WriteFrameAsync(stream, profile.Find(PacketDirection.ClientToServer,
          typeof(PlayerSpawnPacket)).Encode(new PlayerSpawnPacket()));
      await Verify.EventuallyAsync(() => session.Stage == NetworkSessionStage.Active,
          "The bound connection must continue through section and spawn admission after ID 138.");

      foreach (byte id in new byte[] { 15, 25, 26, 44, 67, 83 }) {
        await WriteFrameAsync(stream, new byte[] { 0x03, 0x00, id });
      }
      await WriteFrameAsync(stream, ClientSyncedInventoryFrame);
      byte[] ping = profile.Find(PacketDirection.ClientToServer, typeof(PingPacket))
          .Encode(new PingPacket());
      // Inventory changes can send this notice on consecutive client updates.
      for (int update = 0; update < 16; update++) {
        await WriteFrameAsync(stream, ClientSyncedInventoryFrame);
      }
      await WriteFrameAsync(stream, ping);
      byte[] response = await ReadFrameAsync(stream);
      Verify.That(response.SequenceEqual(ping),
          "A legal ID 138 notice must produce no extra output before the next normal ping reply.");
      Verify.That(session.Stage == NetworkSessionStage.Active && session.Binding == binding
          && authority.AdmissionCount == 1 && authority.ActiveBindings == 1,
          "Reserved no-effect notices must preserve the active binding and stage without changing admission count.");
    }

    await Verify.EventuallyAsync(() => gateway.Sessions.Count == 0
        && authority.ActiveBindings == 0 && authority.AdmissionCount == 1,
        "Closing the valid TCP client must release its original production slot binding.");

    using (var invalidClient = new TcpClient()) {
      (NetworkStream stream, _, _) =
          await ConnectAndAdmitAsync(invalidClient, server, gateway, profile);
      await WriteFrameAsync(stream, new byte[] { 0x04, 0x00, 0x8A, 0x01 });
      await Verify.EventuallyAsync(() => gateway.Sessions.Count == 0
          && authority.ActiveBindings == 0 && authority.AdmissionCount == 2,
          "A non-empty ID 138 body must reject and release the session before any owner effect.");
      Verify.That(HasDiagnostic(gateway, "InventorySyncMarkerMustBeEmpty", 138),
          "A malformed ID 138 must report the precise empty-marker rejection.");
    }

    using (var unboundClient = new TcpClient()) {
      await unboundClient.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)server.EndPoint).Port);
      await WriteFrameAsync(unboundClient.GetStream(), ClientSyncedInventoryFrame);
      await Verify.EventuallyAsync(() => gateway.Sessions.Count == 0
          && HasDiagnostic(gateway, "AdmissionRejected", 138),
          "An unbound ID 138 literal frame must be rejected by gateway stage admission.");
      Verify.That(authority.AdmissionCount == 2 && authority.ActiveBindings == 0,
          "An unbound ID 138 must not allocate or release an actor binding.");
    }

    using (var wrongStageClient = new TcpClient()) {
      (NetworkStream stream, _, _) =
          await ConnectAndAdmitAsync(wrongStageClient, server, gateway, profile);
      await WriteFrameAsync(stream, new byte[] { 0x03, 0x00, 0x0F });
      await Verify.EventuallyAsync(() => gateway.Sessions.Count == 0
          && authority.ActiveBindings == 0 && authority.AdmissionCount == 3
          && HasDiagnostic(gateway, "AdmissionRejected", 15),
          "ID 15 must be rejected before its handler outside the Active phase.");
    }

    using (var disabledZeroClient = new TcpClient()) {
      (NetworkStream stream, _, _) =
          await ConnectAndAdmitAsync(disabledZeroClient, server, gateway, profile);
      await WriteFrameAsync(stream, new byte[] { 0x03, 0x00, 0x00 });
      await Verify.EventuallyAsync(() => gateway.Sessions.Count == 0
          && authority.ActiveBindings == 0 && authority.AdmissionCount == 4
          && HasDiagnostic(gateway, "AdmissionRejected", 0),
          "A real TCP ID 0 frame must be refused and release the bound production slot.");
    }

    Verify.That(server.LastError is null,
        "Reserved valid, invalid-body, wrong-stage, unbound, and disabled-ID TCP cases must not fault the server.");
  }

  private static void VerifyNoEffectRegistrationPolicies(PacketGateway gateway) {
    byte[] ids = [15, 25, 26, 44, 67, 83];
    PacketRegistrationSnapshot[] registrations = gateway.Registrations
        .Where(registration => ids.Contains(registration.Policy.MessageId)).ToArray();
    Verify.That(registrations.Length == ids.Length
        && registrations.All(registration => registration.Policy.AllowedStages
            == NetworkSessionStage.Active
            && registration.Policy.MaximumPerWindow == 120
            && registration.Policy.MaximumBytesPerWindow == 1),
        "Each proven no-effect ID must have its own bounded C2S Active-only registration.");

    PacketPolicy markerPolicy = gateway.Registrations.Single(registration =>
        registration.Policy.MessageId == 138).Policy;
    Verify.That(markerPolicy.AllowedStages
            == (NetworkSessionStage.AwaitPlayerData | NetworkSessionStage.Active)
        && markerPolicy.MaximumPerWindow == 120 && markerPolicy.MaximumBytesPerWindow == 1,
        "ID 138 must be bounded to the client's player-data and active completion-notice phases.");
  }

  private static object CreateNoEffectPacket(byte id) {
    return id switch {
      15 => new Unknown15Packet(),
      25 => new Unused25Packet(),
      26 => new Unused26Packet(),
      44 => new Unknown44Packet(),
      67 => new Unknown67Packet(),
      83 => new Unused83Packet(),
      _ => throw new ArgumentOutOfRangeException(nameof(id))
    };
  }

  private static async Task<(NetworkStream Stream, NetworkSession Session, SenderBinding Binding)>
      ConnectAndAdmitAsync(TcpClient client, PacketTcpServer server, PacketGateway gateway,
          ProtocolProfile profile) {
    await client.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)server.EndPoint).Port);
    NetworkStream stream = client.GetStream();
    await WriteFrameAsync(stream, profile.Find(PacketDirection.ClientToServer,
        typeof(HelloPacket)).Encode(new HelloPacket { Version = profile.HelloVersion }));
    byte[] admissionFrame = await ReadFrameAsync(stream);
    object admissionPacket = profile.Find(PacketDirection.ServerToClient,
        admissionFrame[2]).Decode(admissionFrame.AsMemory(3));
    Verify.That(admissionPacket is PlayerInfoPacket { Player: 0, Accepted: false },
        "The production TCP fixture must bind the next available server-owned slot.");
    NetworkSession session = await GetSingleSessionAsync(gateway);
    SenderBinding binding = session.Binding
        ?? throw new InvalidOperationException("The TCP fixture did not retain its sender binding.");
    return (stream, session, binding);
  }

  private static async Task<NetworkSession> GetSingleSessionAsync(PacketGateway gateway) {
    NetworkSession? session = null;
    await Verify.EventuallyAsync(() => {
      session = gateway.Sessions.SingleOrDefault();
      return session is not null;
    }, "The accepted TCP session did not appear in the gateway.");
    return session!;
  }

  private static bool HasDiagnostic(PacketGateway gateway, string code, byte messageId) {
    return DrainDiagnostics(gateway).Any(diagnostic => diagnostic.Code == code
        && diagnostic.MessageId == messageId);
  }

  private static async Task WriteFrameAsync(NetworkStream stream, ReadOnlyMemory<byte> frame) {
    await stream.WriteAsync(frame);
  }

  private static async Task<byte[]> ReadFrameAsync(NetworkStream stream) {
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var prefix = new byte[2];
    await stream.ReadExactlyAsync(prefix, timeout.Token);
    ushort length = BinaryPrimitives.ReadUInt16LittleEndian(prefix);
    if (length < 3) {
      throw new InvalidOperationException($"The server returned an invalid frame length {length}.");
    }

    var frame = new byte[length];
    prefix.CopyTo(frame, 0);
    await stream.ReadExactlyAsync(frame.AsMemory(2), timeout.Token);
    return frame;
  }

  private static IReadOnlyList<PacketGatewayDiagnostic> DrainDiagnostics(PacketGateway gateway) {
    var diagnostics = new List<PacketGatewayDiagnostic>();
    while (gateway.TryReadDiagnostic(out PacketGatewayDiagnostic? diagnostic)) {
      diagnostics.Add(diagnostic!);
    }
    return diagnostics;
  }
}
