using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class OwnerTimeoutVerification {
  public static async Task NoncooperativeOwnerAsync() {
    var time = new ManualTimeProvider();
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var authority = new RecordingAuthority();
    await using var gateway = new PacketGateway(profile, authority,
        new PacketGatewayOptions { OwnerTimeout = TimeSpan.FromSeconds(2) }, time);
    GatewayVerification.RegisterProgression(gateway);
    var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var lateResult = new TaskCompletionSource<PacketHandlingResult>(
        TaskCreationOptions.RunContinuationsAsynchronously);
    gateway.Register(new PacketPolicy(30, NetworkSessionStage.Active),
        new RecordingHandler<TogglePVPPacket>((_, _, _) => {
          entered.TrySetResult();
          return new ValueTask<PacketHandlingResult>(lateResult.Task);
        }));
    await using var peer = new GatewayPeer(gateway, profile);
    await peer.JoinAsync();
    peer.Receive(new TogglePVPPacket());
    await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
    time.Advance(TimeSpan.FromSeconds(3));
    await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(peer.Session.Stage == NetworkSessionStage.Closed && authority.Releases == 1,
        "A handler ignoring cancellation must not prevent deadline cleanup or binding release.");
    int sentBeforeLateResult = peer.Transport.FrameCount;
    lateResult.TrySetResult(new PacketHandlingResult(true, new[] {
      new OutboundDispatch(new PlayerLifeManaPacket(), PacketDispatchKind.Single,
          new[] { peer.Connection.Identity })
    }));
    await gateway.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(peer.Transport.FrameCount == sentBeforeLateResult,
        "A late owner completion must not resurrect output or reestablish the closed session.");
  }
}
