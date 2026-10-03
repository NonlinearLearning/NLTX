using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class GatewayDeadlineVerification {
  public static async Task ProgressingSelfSendAsync() {
    var time = new ManualTimeProvider();
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var authority = new RecordingAuthority();
    await using var gateway = new PacketGateway(profile, authority,
        new PacketGatewayOptions { OwnerTimeout = TimeSpan.FromSeconds(4) }, time);
    GatewayVerification.RegisterProgression(gateway);
    gateway.Register(new PacketPolicy(30, NetworkSessionStage.Active),
        new RecordingHandler<Packet30Packet>((context, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, new[] {
              new OutboundDispatch(new Packet16Packet(), PacketDispatchKind.Single,
                  new[] { context.Connection })
            }))));
    await using var peer = new GatewayPeer(gateway, profile, ConnectionOptions(), time);
    await peer.JoinAsync();
    peer.Transport.OnSubmit = _ => true;
    peer.Receive(new Packet30Packet());
    await ProgressToDeadlineAsync(peer, time);
    await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(peer.Session.Stage == NetworkSessionStage.Closed
        && peer.Transport.CloseCalls > 0 && authority.Releases == 1 && peer.Budget.Used == 0,
        "Sending progress must not keep the source session alive past the gateway owner deadline.");
    var diagnostics = new List<PacketGatewayDiagnostic>();
    await Verify.EventuallyAsync(() => {
      diagnostics.AddRange(GatewayVerification.ReadDiagnostics(gateway));
      return diagnostics.Any(item => item.SendCertainty == PacketSendCertainty.OutcomeUnknown);
    }, "A partially sent reply closed by its gateway deadline must retain unknown send outcome.");
    Verify.That(!diagnostics.Any(item => item.Code == "LocallySent" && item.MessageId == 16),
        "Gateway timeout must not manufacture a successful receipt for incomplete output.");
  }

  public static async Task ProgressingRecipientAsync() {
    var time = new ManualTimeProvider();
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var authority = new RecordingAuthority();
    await using var gateway = new PacketGateway(profile, authority,
        new PacketGatewayOptions { OwnerTimeout = TimeSpan.FromSeconds(4) }, time);
    GatewayVerification.RegisterProgression(gateway);
    gateway.Register(new PacketPolicy(30, NetworkSessionStage.Active),
        new RecordingHandler<Packet30Packet>((_, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, new[] {
              new OutboundDispatch(new Packet16Packet(), PacketDispatchKind.AllActiveExceptSender)
            }))));
    await using var sender = new GatewayPeer(gateway, profile, ConnectionOptions(), time);
    await using var slow = new GatewayPeer(gateway, profile, ConnectionOptions(), time);
    await using var normal = new GatewayPeer(gateway, profile, ConnectionOptions(), time);
    await sender.JoinAsync();
    await slow.JoinAsync();
    await normal.JoinAsync();
    slow.Transport.OnSubmit = _ => true;
    sender.Receive(new Packet30Packet());
    await Verify.EventuallyAsync(() => normal.Transport.FrameCount == 2,
        "A normal recipient must finish before the progressing slow recipient's owner deadline.");
    await ProgressToDeadlineAsync(slow, time);
    await sender.Run.WaitAsync(TimeSpan.FromSeconds(5));
    await slow.Run.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(sender.Session.Stage == NetworkSessionStage.Closed
        && slow.Session.Stage == NetworkSessionStage.Closed && slow.Budget.Used == 0
        && normal.Session.Stage == NetworkSessionStage.Active && authority.Releases == 2,
        "Gateway deadline must close its source and unfinished recipient while preserving normal peers.");
  }

  public static async Task LateAdmissionAsync() {
    foreach (bool stopGateway in new[] { false, true }) {
      var time = new ManualTimeProvider();
      ProtocolProfile profile = GatewayVerification.CreateProfile();
      var authority = new DelayedAuthority();
      await using var gateway = new PacketGateway(profile, authority,
          new PacketGatewayOptions { OwnerTimeout = TimeSpan.FromSeconds(2) }, time);
      await using var peer = new GatewayPeer(gateway, profile, ConnectionOptions(), time);
      peer.Receive(new Packet1Packet { Version = "Terraria319" });
      await authority.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
      if (stopGateway) {
        await gateway.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
      } else {
        time.Advance(TimeSpan.FromSeconds(3));
      }
      await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(peer.Session.Stage == NetworkSessionStage.Closed
          && peer.Session.Binding is null && authority.Releases == 0,
          "Closing must not invent a binding or release before the noncooperative authority returns.");
      authority.Result.TrySetResult(new SessionAdmission(authority.Binding));
      await Verify.EventuallyAsync(() => authority.Releases == 1,
          "A successful admission returned after timeout or Stop must release its late lease.");
      Verify.That(peer.Session.Stage == NetworkSessionStage.Closed
          && peer.Session.Binding is null && peer.Transport.FrameCount == 0
          && peer.Budget.Used == 0 && gateway.Sessions.Count == 0,
          "Late admission must not revive protocol state, send a slot or retain connection resources.");
    }
  }

  public static async Task ClosedSessionGuardsAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    await using var gateway = new PacketGateway(profile, new RecordingAuthority());
    await using var peer = new GatewayPeer(gateway, profile);
    peer.Receive(new Packet1Packet { Version = "Terraria319" });
    await Verify.EventuallyAsync(() => peer.Transport.FrameCount == 1,
        "The guard fixture must establish its original actor before closure.");
    SenderBinding? original = peer.Session.Binding;
    foreach (NetworkSessionStage stage in new[] {
      NetworkSessionStage.Closing, NetworkSessionStage.Closed
    }) {
      peer.Session.MoveTo(stage);
      Verify.That(!peer.Session.Bind(new SenderBinding(8, Guid.NewGuid()))
          && peer.Session.Stage == stage && peer.Session.Binding == original,
          "A closing or closed session must refuse rebinding without replacing its original lease.");
      PacketProtocolException host = Verify.Throws<PacketProtocolException>(peer.Session.SetHost);
      PacketProtocolException apply = Verify.Throws<PacketProtocolException>(() =>
          peer.Session.Apply(6, new PacketHandlingResult(true,
              nextStage: NetworkSessionStage.AwaitSectionRequest)));
      Verify.That(host.Code == "SessionClosed" && apply.Code == "SessionClosed",
          "Late host authorization and owner state application must retain terminal closure.");
    }
  }

  private static PacketConnectionOptions ConnectionOptions() {
    return ConnectionVerification.Options() with {
      PartialFrameTimeout = TimeSpan.FromMinutes(1)
    };
  }

  private static async Task ProgressToDeadlineAsync(GatewayPeer peer, ManualTimeProvider time) {
    await Verify.EventuallyAsync(() => peer.Transport.FrameCount == 2
        && time.NextTimerDelay == TimeSpan.FromSeconds(2),
        "The partial output and its sending deadline were not installed.");
    for (int second = 1; second <= 3; second++) {
      time.Advance(TimeSpan.FromSeconds(1));
      peer.Connection.NotifySent(peer.Connection.Identity.Epoch, 1);
      TimeSpan next = TimeSpan.FromSeconds(Math.Min(2, 4 - second));
      await Verify.EventuallyAsync(() => time.NextTimerDelay == next,
          "Real output progress did not reset inactivity while preserving the owner deadline.");
      Verify.That(!peer.Run.IsCompleted && peer.Transport.CloseCalls == 0,
          "The deadline fixture must keep partial sending alive with real progress until t=4.");
    }
    time.Advance(TimeSpan.FromSeconds(1));
  }
}
