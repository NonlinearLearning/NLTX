namespace Terraria.Network;

public sealed class PacketEncodingException : Exception {
  public PacketEncodingException(string message) : base(message) {
  }
}
