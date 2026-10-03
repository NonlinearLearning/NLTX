namespace Terraria.Network;

public sealed class PacketSendException : IOException {
  public string Code { get; }
  public PacketSendCertainty Certainty { get; }

  public PacketSendException(PacketSendCertainty certainty, string reason,
      Exception? innerException = null) : base(reason, innerException) {
    Certainty = certainty;
    Code = reason;
  }
}
