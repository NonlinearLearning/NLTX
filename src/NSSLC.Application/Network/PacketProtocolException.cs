namespace Terraria.Network;

public sealed class PacketProtocolException : IOException {
  public string Code { get; }
  public byte? MessageId { get; }
  public int BodyOffset { get; }

  public PacketProtocolException(string code, byte? messageId = null, int bodyOffset = 0)
      : base($"Packet protocol failure: {code}.") {
    Code = code;
    MessageId = messageId;
    BodyOffset = bodyOffset;
  }
}
