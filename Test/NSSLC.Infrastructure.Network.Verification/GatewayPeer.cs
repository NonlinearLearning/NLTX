using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal sealed class GatewayPeer : IAsyncDisposable {
  private readonly ProtocolProfile _profile;
  private readonly Task _run;

  public PacketConnection Connection { get; }
  public RecordingTransport Transport { get; } = new();
  public PacketByteBudget Budget { get; } = new(4096);
  public NetworkSession Session { get; }
  public Task Run => _run;

  public GatewayPeer(PacketGateway gateway, ProtocolProfile profile,
      PacketConnectionOptions? options = null, TimeProvider? timeProvider = null) {
    _profile = profile;
    Connection = new(new(Guid.NewGuid(), 1), Transport, profile,
        PacketDirection.ClientToServer, options ?? ConnectionVerification.Options(), Budget,
        timeProvider);
    Transport.OnSubmit = frame => {
      Connection.NotifySent(Connection.Identity.Epoch, frame.Length);
      return true;
    };
    _run = gateway.HandleAsync(Connection);
    Session = gateway.Sessions.Single(item => item.Identity == Connection.Identity);
  }

  public void Receive<TPacket>(TPacket packet) where TPacket : notnull {
    byte[] frame = _profile.Find(PacketDirection.ClientToServer, typeof(TPacket)).Encode(packet);
    Connection.ReceiveBytes(Connection.Identity.Epoch, frame);
  }

  public void ReceiveRaw(byte[] frame) {
    Connection.ReceiveBytes(Connection.Identity.Epoch, frame);
  }

  public async Task JoinAsync(string? password = null) {
    Receive(new Packet1Packet { Version = "Terraria319" });
    await Verify.EventuallyAsync(() => Session.Stage is NetworkSessionStage.AwaitPlayerData
        or NetworkSessionStage.AwaitPassword, "Hello did not progress to authentication or player data.");
    if (password is not null) {
      Receive(new Packet38Packet { Password = password });
      await Verify.EventuallyAsync(() => Session.Stage == NetworkSessionStage.AwaitPlayerData,
          "Correct password did not bind the session.");
    }
    Receive(new Packet6Packet());
    await Verify.EventuallyAsync(() => Session.Stage == NetworkSessionStage.AwaitSectionRequest,
        "The world-data owner did not confirm its state transition.");
    Receive(new Packet8Packet());
    await Verify.EventuallyAsync(() => Session.Stage == NetworkSessionStage.Synchronizing,
        "The section owner did not confirm its state transition.");
    Receive(new Packet12Packet());
    await Verify.EventuallyAsync(() => Session.Stage == NetworkSessionStage.Active,
        "The spawn owner did not confirm active admission.");
  }

  public async ValueTask DisposeAsync() {
    await Connection.DisposeAsync();
    await _run.WaitAsync(TimeSpan.FromSeconds(5));
  }
}
