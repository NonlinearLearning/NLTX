namespace Terraria.Network.Protocol;

public sealed class InboundNetworkCommand
{
  private readonly byte[] _payload;

  public InboundNetworkCommand(
    int connectionSlot,
    int messageId,
    int protocolVersion,
    byte[] payload,
    long receiveSequence)
  {
    if (connectionSlot < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(connectionSlot));
    }

    if (messageId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(messageId));
    }

    if (protocolVersion < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(protocolVersion));
    }

    if (receiveSequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(receiveSequence));
    }

    ArgumentNullException.ThrowIfNull(payload);

    ConnectionSlot = connectionSlot;
    MessageId = messageId;
    ProtocolVersion = protocolVersion;
    ReceiveSequence = receiveSequence;
    _payload = payload.ToArray();
  }

  public int ConnectionSlot { get; }

  public int MessageId { get; }

  public ReadOnlyMemory<byte> Payload => _payload;

  public int ProtocolVersion { get; }

  public long ReceiveSequence { get; }
}
