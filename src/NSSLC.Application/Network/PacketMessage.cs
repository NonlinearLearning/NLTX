namespace Terraria.Network;

public sealed class PacketMessage {
  public string ProfileKey { get; }
  public PacketDirection Direction { get; }
  public byte MessageId { get; }
  public ConnectionIdentity Connection { get; }
  public long Sequence { get; }
  public object Payload { get; }

  public PacketMessage(string profileKey, PacketDirection direction, byte messageId,
      ConnectionIdentity connection, long sequence, object payload) {
    ArgumentException.ThrowIfNullOrWhiteSpace(profileKey);
    ArgumentNullException.ThrowIfNull(payload);
    ProfileKey = profileKey;
    Direction = direction;
    MessageId = messageId;
    Connection = connection;
    Sequence = sequence;
    Payload = payload;
  }

  public TPacket Get<TPacket>() {
    if (Payload.GetType() != typeof(TPacket)) {
      throw new InvalidOperationException("The message has a different packet type.");
    }
    return (TPacket)Payload;
  }
}
