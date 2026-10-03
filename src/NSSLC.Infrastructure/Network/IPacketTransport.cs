namespace NSSLC.Infrastructure.Network;

internal interface IPacketTransport {
  bool Submit(ReadOnlySpan<byte> frame);
  void Close();
}
