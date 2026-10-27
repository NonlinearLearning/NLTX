namespace Terraria.Communication;

/// <summary>Host transport seam used by the outbound delivery pump.</summary>
public interface ICommunicationTransportPort
{
  ValueTask<IReadOnlyList<SendReceipt>> SendAsync(
      OutboundPacketIntent intent,
      CancellationToken cancellationToken = default);
}
