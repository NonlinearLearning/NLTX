using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

internal sealed class ReservedNoEffectPacketHandlers : IPacketHandler<Unknown15Packet>,
    IPacketHandler<Unused25Packet>, IPacketHandler<Unused26Packet>,
    IPacketHandler<Unknown44Packet>, IPacketHandler<Unknown67Packet>,
    IPacketHandler<Unused83Packet> {
  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      Unknown15Packet packet, CancellationToken cancellationToken) {
    return Consume(context, cancellationToken);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      Unused25Packet packet, CancellationToken cancellationToken) {
    return Consume(context, cancellationToken);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      Unused26Packet packet, CancellationToken cancellationToken) {
    return Consume(context, cancellationToken);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      Unknown44Packet packet, CancellationToken cancellationToken) {
    return Consume(context, cancellationToken);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      Unknown67Packet packet, CancellationToken cancellationToken) {
    return Consume(context, cancellationToken);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      Unused83Packet packet, CancellationToken cancellationToken) {
    return Consume(context, cancellationToken);
  }

  private static ValueTask<PacketHandlingResult> Consume(NetworkSessionContext context,
      CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (context.Stage != NetworkSessionStage.Active) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "ReservedNoEffectRequiresActiveSession"));
    }
    return ValueTask.FromResult(new PacketHandlingResult(true));
  }
}
