using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace NSSLC.NetworkVerification;

internal static class ControlReserveVerification {
  private readonly record struct ControlPacket;

  public static async Task RunAsync() {
    var normal = new PacketBinding<BindingVerification.BytePacket>(13,
        PacketDirection.ServerToClient,
        _ => PacketReadResult<BindingVerification.BytePacket>.Succeeded(new(), 61),
        _ => new MemoryStream(new byte[61]));
    var control = new PacketBinding<ControlPacket>(2, PacketDirection.ServerToClient,
        _ => PacketReadResult<ControlPacket>.Succeeded(new(), 1),
        _ => new MemoryStream(new byte[1]));
    var profile = new ProtocolProfile("reserve", "Terraria319", new PacketBinding[] { normal, control });
    var transport = new RecordingTransport();
    var budget = new PacketByteBudget(4096);
    await using var connection = new PacketConnection(new(Guid.NewGuid(), 1), transport, profile,
        PacketDirection.ClientToServer, ConnectionVerification.Options() with {
          SendBytes = 96, SendItems = 3, ControlReserveBytes = 32, ControlReserveItems = 1
        }, budget);
    Task<PacketWriteReceipt> first = connection.WritePacketAsync(new BindingVerification.BytePacket()).AsTask();
    await Verify.EventuallyAsync(() => transport.FrameCount == 1, "Normal maximum frame was not submitted.");
    Task<PacketWriteReceipt> waitingNormal = connection.WritePacketAsync(
        new BindingVerification.BytePacket()).AsTask();
    Task<PacketWriteReceipt> waitingControl = connection.WritePacketAsync(new ControlPacket()).AsTask();
    Verify.That(budget.Used == 68 && transport.FrameCount == 1
        && !waitingControl.IsCompleted && !waitingNormal.IsCompleted,
        "Control reserve must remain available inside total limits while normal quota is exhausted.");
    connection.NotifySent(1, 64);
    await first;
    await Verify.EventuallyAsync(() => transport.FrameCount == 2,
        "Reserved control frame was not submitted after the earlier normal frame completed.");
    Verify.That(transport.GetFrame(0)[2] == 13 && transport.GetFrame(1)[2] == 2,
        "Reserved control capacity must preserve FIFO rather than bypass an earlier submitted frame.");
    connection.NotifySent(1, 4);
    await waitingControl;
    await Verify.EventuallyAsync(() => transport.FrameCount == 3,
        "Normal producer did not resume after capacity was released.");
    Verify.That(transport.GetFrame(2)[2] == 13 && budget.Used <= 96,
        "Normal and control quotas together must not exceed the configured total send capacity.");
    connection.NotifySent(1, 64);
    await waitingNormal;
    Verify.That(budget.Used == 0, "Control and normal completion must release both quota types.");
  }
}
