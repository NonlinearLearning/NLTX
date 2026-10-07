using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class GatewayVerification {
  public static async Task HandshakeAndActorAsync() {
    ProtocolProfile profile = CreateProfile();
    var authority = new RecordingAuthority();
    await using var gateway = new PacketGateway(profile, authority);
    RegisterProgression(gateway);
    NetworkSessionContext? captured = null;
    byte claimedActor = 0;
    gateway.Register(new PacketPolicy(13, NetworkSessionStage.Active),
        new RecordingHandler<PlayerControlsPacket>((context, packet, _) => {
          captured = context;
          claimedActor = packet.Player;
          var authoritative = new PlayerControlsPacket {
            Player = context.Actor.PlayerSlot,
            ControlFlags = packet.ControlFlags,
            MovementFlags = packet.MovementFlags,
            PlayerFeatureFlags = packet.PlayerFeatureFlags,
            ActionFlags = packet.ActionFlags,
            SelectedItem = packet.SelectedItem,
            Position = packet.Position,
            Velocity = packet.Velocity,
            MountType = packet.MountType,
            PotionOfReturnUsePosition = packet.PotionOfReturnUsePosition,
            PotionOfReturnHomePosition = packet.PotionOfReturnHomePosition,
            CameraTarget = packet.CameraTarget
          };
          return ValueTask.FromResult(new PacketHandlingResult(true, new[] {
            new OutboundDispatch(authoritative, PacketDispatchKind.Single,
                new[] { context.Connection })
          }));
        }));
    await using (var peer = new GatewayPeer(gateway, profile)) {
      await peer.JoinAsync();
      Verify.That(peer.Session.Binding?.PlayerSlot == 0 && authority.Admissions == 1,
          "Hello must bind only the slot allocated by application authority.");
      peer.Receive(new PlayerControlsPacket {
        Player = 201, SelectedItem = 7, Position = new(12.5f, -3.25f)
      });
      await Verify.EventuallyAsync(() => peer.Transport.FrameCount == 2,
          "The authoritative self-targeted response was not sent.");
      Verify.That(captured is not null && captured.Actor.PlayerSlot == 0 && claimedActor == 201,
          "Gateway actor identity must come from binding while preserving the wire claim for the owner.");
      PlayerControlsPacket response = (PlayerControlsPacket)profile.Find(PacketDirection.ServerToClient, (byte)13)
          .Decode(peer.Transport.GetFrame(1).AsMemory(3));
      Verify.That(response.Player == 0 && response.SelectedItem == 7
          && response.Position == new PacketVector2(12.5f, -3.25f),
          "Only the owner's committed authoritative packet may be routed back to the client.");
      Verify.Throws<InvalidOperationException>(() => gateway.Register(
          new PacketPolicy(30, NetworkSessionStage.Active),
          new RecordingHandler<TogglePVPPacket>((_, _, _) => ValueTask.FromResult(new PacketHandlingResult(true)))));
    }
    Verify.That(authority.Releases == 1 && gateway.Sessions.Count == 0,
        "Disconnect must release the exact authority binding once and remove its session.");
  }

  public static async Task DefaultDenyAsync() {
    ProtocolProfile profile = CreateProfile();
    foreach (byte id in new byte[] { 10, 85, 94, 93, 0 }) {
      var authority = new RecordingAuthority();
      await using var gateway = new PacketGateway(profile, authority);
      await using var peer = new GatewayPeer(gateway, profile);
      peer.ReceiveRaw(new byte[] { 3, 0, id });
      await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(authority.Admissions == 0 && peer.Transport.CloseCalls > 0
          && peer.Session.Stage == NetworkSessionStage.Closed && peer.Budget.Used == 0,
          $"ID {id} must be denied before decoding or binding in AwaitHello.");
      Verify.That(ReadDiagnostics(gateway).Any(item => item.Code == "AdmissionRejected"),
          "Gateway rejection must expose metadata without retaining the payload.");
    }
  }

  public static async Task VersionAndPasswordAsync() {
    ProtocolProfile profile = CreateProfile();
    var rejectedAuthority = new RecordingAuthority();
    await using (var rejectedGateway = new PacketGateway(profile, rejectedAuthority)) {
      await using var peer = new GatewayPeer(rejectedGateway, profile);
      peer.Receive(new HelloPacket { Version = "Terraria999" });
      await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(rejectedAuthority.Admissions == 0
          && ReadDiagnostics(rejectedGateway).Any(item => item.Code == "VersionRejected"),
          "A mismatched wire version must fail before authority admission.");
    }

    var authority = new RecordingAuthority { RequiresPassword = true };
    await using (var gateway = new PacketGateway(profile, authority)) {
      RegisterProgression(gateway);
      await using var peer = new GatewayPeer(gateway, profile);
      await peer.JoinAsync("correct");
      Verify.That(peer.Transport.GetFrame(0)[2] == 37 && peer.Transport.GetFrame(1)[2] == 3
          && authority.Admissions == 1,
          "Password flow must request authentication before assigning a player slot.");
    }
    var deniedAuthority = new RecordingAuthority { RequiresPassword = true };
    await using (var gateway = new PacketGateway(profile, deniedAuthority)) {
      await using var peer = new GatewayPeer(gateway, profile);
      peer.Receive(new HelloPacket { Version = "Terraria319" });
      await Verify.EventuallyAsync(() => peer.Session.Stage == NetworkSessionStage.AwaitPassword,
          "Password session was not entered.");
      peer.Receive(new SendPasswordPacket { Password = "wrong" });
      await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(deniedAuthority.Admissions == 0 && deniedAuthority.Releases == 0
          && ReadDiagnostics(gateway).Any(item => item.Code == "AdmissionDenied"),
          "Wrong passwords must close without creating or releasing a nonexistent actor binding.");
    }
  }

  public static async Task IgnoredClientVersionAsync() {
    ProtocolProfile profile = CreateProfile();
    var options = new PacketGatewayOptions { IgnoreClientVersion = true };
    var authority = new RecordingAuthority();
    await using (var gateway = new PacketGateway(profile, authority, options)) {
      await using var peer = new GatewayPeer(gateway, profile);
      peer.Receive(new HelloPacket { Version = "Terraria326" });
      await Verify.EventuallyAsync(() => peer.Transport.FrameCount > 0,
          "An ignored client version must receive the normal handshake response.");
      Verify.That(peer.Transport.GetFrame(0)[2] == 3 && authority.Admissions == 1
          && peer.Session.Stage == NetworkSessionStage.AwaitPlayerData,
          "Version opt-out must admit a Terraria326 client through the normal slot authority.");
      Verify.That(!ReadDiagnostics(gateway).Any(item => item.Code == "VersionRejected"),
          "Version opt-out must not reject the client for its hello version.");
    }
    Verify.That(authority.Releases == 1,
        "Cross-version disconnect must still release the exact authority binding.");

    var protectedAuthority = new RecordingAuthority { RequiresPassword = true };
    await using (var gateway = new PacketGateway(profile, protectedAuthority, options)) {
      await using var peer = new GatewayPeer(gateway, profile);
      peer.Receive(new HelloPacket { Version = "Terraria326" });
      await Verify.EventuallyAsync(() => peer.Transport.FrameCount > 0,
          "A cross-version client must still receive the password challenge.");
      Verify.That(peer.Transport.GetFrame(0)[2] == 37
          && peer.Session.Stage == NetworkSessionStage.AwaitPassword,
          "Version opt-out must preserve password admission.");
      peer.Receive(new SendPasswordPacket { Password = "wrong" });
      await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(protectedAuthority.Admissions == 0 && protectedAuthority.Releases == 0
          && ReadDiagnostics(gateway).Any(item => item.Code == "AdmissionDenied"),
          "Ignoring the hello version must not bypass a rejected password.");
    }
  }

  public static async Task OwnerStageAndPolicyAsync() {
    ProtocolProfile profile = CreateProfile();
    var authority = new RecordingAuthority();
    await using var gateway = new PacketGateway(profile, authority);
    var handled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
        new RecordingHandler<RequestWorldDataPacket>((_, _, _) => {
          handled.TrySetResult();
          return ValueTask.FromResult(new PacketHandlingResult(true));
        }));
    await using var peer = new GatewayPeer(gateway, profile);
    peer.Receive(new HelloPacket { Version = "Terraria319" });
    await Verify.EventuallyAsync(() => peer.Session.Stage == NetworkSessionStage.AwaitPlayerData,
        "Hello did not bind the player.");
    peer.Receive(new RequestWorldDataPacket());
    await handled.Task.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(peer.Session.Stage == NetworkSessionStage.AwaitPlayerData,
        "Receiving an accepted ID 6 without owner progress must not infer world synchronization.");
    peer.Receive(new SpawnTileDataPacket());
    await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(authority.Releases == 1 && peer.Session.Stage == NetworkSessionStage.Closed,
        "ID 8 outside its precise stage/policy must close and release the session.");
  }

  public static async Task SessionDeadlineAsync() {
    var time = new ManualTimeProvider();
    ProtocolProfile profile = CreateProfile();
    await using var gateway = new PacketGateway(profile, new RecordingAuthority(), timeProvider: time);
    await using var peer = new GatewayPeer(gateway, profile);
    time.Advance(TimeSpan.FromSeconds(11));
    await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(peer.Session.Stage == NetworkSessionStage.Closed
        && ReadDiagnostics(gateway).Any(item => item.Code == "SessionTimeout"),
        "A silent AwaitHello connection must hit the configured stage deadline and terminate.");
  }

  public static async Task PolicyRateAndModuleAsync() {
    var time = new ManualTimeProvider();
    ProtocolProfile profile = CreateProfile();
    await using var gateway = new PacketGateway(profile, new RecordingAuthority(),
        timeProvider: time);
    gateway.Register(new PacketPolicy(30, NetworkSessionStage.Active, MaximumPerWindow: 2),
        new RecordingHandler<TogglePVPPacket>((_, _, _) => ValueTask.FromResult(new PacketHandlingResult(true))));
    gateway.Register(new PacketPolicy(82, NetworkSessionStage.Active, ModuleId: 12, Action: 0),
        new RecordingHandler<NetModulesPacket>((_, _, _) => ValueTask.FromResult(new PacketHandlingResult(true))));
    await using var peer = new GatewayPeer(gateway, profile);
    peer.Receive(new HelloPacket { Version = "Terraria319" });
    await Verify.EventuallyAsync(() => peer.Session.Stage == NetworkSessionStage.AwaitPlayerData,
        "The policy fixture must establish a real authority binding first.");
    peer.Session.MoveTo(NetworkSessionStage.Active);
    Verify.That(peer.Session.Admit(30, new byte[] { 0, 1 })
        && peer.Session.Admit(30, new byte[] { 0, 1 })
        && !peer.Session.Admit(30, new byte[] { 0, 1 }),
        "Policy rate windows must count admitted frames and deny the first excess command.");
    time.Advance(TimeSpan.FromSeconds(1));
    Verify.That(peer.Session.Admit(30, new byte[] { 0, 1 }),
        "An elapsed rate window must reset bounded admission counters.");
    Verify.That(peer.Session.Admit(82, new byte[] { 12, 0, 0, 0 })
        && !peer.Session.Admit(82, new byte[] { 12, 0, 0, 2 })
        && !peer.Session.Admit(82, new byte[] { 1, 0 })
        && !peer.Session.Admit(82, new byte[] { 12 }),
        "Module and action allowlists must reject unsupported selectors before expensive decoding.");
  }

  public static Task RegistrationSafetyAsync() {
    ProtocolProfile profile = CreateProfile();
    var authority = new RecordingAuthority();
    var gateway = new PacketGateway(profile, authority);
    var handler = new RecordingHandler<PlayerControlsPacket>((_, _, _) =>
        ValueTask.FromResult(new PacketHandlingResult(true)));
    foreach (byte id in new byte[] { 0, 1, 10, 85, 93, 94, 161 }) {
      Verify.Throws<ArgumentException>(() => gateway.Register(new PacketPolicy(id,
          NetworkSessionStage.Active), handler));
    }
    Verify.Throws<ArgumentException>(() => gateway.Register(new PacketPolicy(13,
        NetworkSessionStage.AwaitHello), handler));
    Verify.Throws<ArgumentException>(() => gateway.Register(new PacketPolicy(13,
        NetworkSessionStage.Active | (NetworkSessionStage)256), handler));
    Verify.Throws<ArgumentException>(() => gateway.Register(new PacketPolicy(13,
        NetworkSessionStage.Active, ModuleId: 2), handler));
    gateway.Register(new PacketPolicy(13, NetworkSessionStage.Active), handler);
    Verify.Throws<ArgumentException>(() => gateway.Register(new PacketPolicy(13,
        NetworkSessionStage.Active), handler));
    var moduleHandler = new RecordingHandler<NetModulesPacket>((_, _, _) =>
        ValueTask.FromResult(new PacketHandlingResult(true)));
    foreach ((ushort module, byte action) in new (ushort, byte)[] {
      (4, 3), (7, 3), (9, 1), (10, 3), (12, 2), (13, 0), (13, 3)
    }) {
      Verify.Throws<ArgumentException>(() => gateway.Register(new PacketPolicy(82,
          NetworkSessionStage.Active, ModuleId: module, Action: action), moduleHandler));
    }
    foreach ((ushort module, byte action) in new (ushort, byte)[] {
      (4, 2), (7, 2), (9, 0), (10, 2), (12, 0), (12, 1), (13, 1), (13, 2)
    }) {
      gateway.Register(new PacketPolicy(82, NetworkSessionStage.Active,
          ModuleId: module, Action: action), moduleHandler);
    }
    return gateway.DisposeAsync().AsTask();
  }

  public static void RegisterProgression(PacketGateway gateway) {
    gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
        new RecordingHandler<RequestWorldDataPacket>((_, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, nextStage: NetworkSessionStage.AwaitSectionRequest))));
    gateway.Register(new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest),
        new RecordingHandler<SpawnTileDataPacket>((_, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, nextStage: NetworkSessionStage.Synchronizing))));
    gateway.Register(new PacketPolicy(12, NetworkSessionStage.Synchronizing),
        new RecordingHandler<PlayerSpawnPacket>((_, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, nextStage: NetworkSessionStage.Active))));
  }

  public static ProtocolProfile CreateProfile() {
    return TerrariaProtocolProfile.Create(GeneratedProfileVerification.CreateFacts());
  }

  public static IReadOnlyList<PacketGatewayDiagnostic> ReadDiagnostics(PacketGateway gateway) {
    var diagnostics = new List<PacketGatewayDiagnostic>();
    while (gateway.TryReadDiagnostic(out PacketGatewayDiagnostic? item)) {
      diagnostics.Add(item!);
    }
    return diagnostics;
  }
}
