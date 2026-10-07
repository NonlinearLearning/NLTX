using System.Buffers.Binary;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class GatewayPolicyVerification {
  public static async Task PasswordSnapshotAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var authority = new MutablePasswordAuthority();
    await using var gateway = new PacketGateway(profile, authority);
    GatewayVerification.RegisterProgression(gateway);
    authority.PasswordRequired = false;
    await using (var peer = new GatewayPeer(gateway, profile)) {
      await peer.JoinAsync("correct");
      Verify.That(authority.Reads == 1 && peer.Transport.GetFrame(0)[2] == 37
          && peer.Transport.GetFrame(1)[2] == 3,
          "The startup password requirement must govern Hello and password admission consistently.");
    }
    Verify.That(authority.Releases == 1,
        "An admission using captured password capability must release its allocated actor once.");
  }

  public static async Task ProtocolDiagnosticAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var authority = new RecordingAuthority();
    await using var gateway = new PacketGateway(profile, authority);
    GatewayVerification.RegisterProgression(gateway);
    gateway.Register(new PacketPolicy(13, NetworkSessionStage.Active),
        new RecordingHandler<PlayerControlsPacket>((_, _, _) =>
            throw new InvalidOperationException("A malformed packet must never reach its owner.")));
    await using var peer = new GatewayPeer(gateway, profile);
    await peer.JoinAsync();
    byte[] valid = profile.Find(PacketDirection.ClientToServer, typeof(PlayerControlsPacket))
        .Encode(new PlayerControlsPacket());
    byte[] trailing = new byte[valid.Length + 1];
    valid.CopyTo(trailing, 0);
    trailing[^1] = 17;
    BinaryPrimitives.WriteUInt16LittleEndian(trailing, (ushort)trailing.Length);
    peer.ReceiveRaw(trailing);
    await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(GatewayVerification.ReadDiagnostics(gateway).Any(item =>
        item.Code == "TrailingBytes" && item.MessageId == 13
            && item.BodyOffset == valid.Length - 3),
        "Protocol failure diagnostics must carry the original packet ID and body-relative offset.");
    Verify.That(authority.Releases == 1 && peer.Budget.Used == 0,
        "Malformed body diagnostics must accompany complete resource cleanup.");
  }
}
