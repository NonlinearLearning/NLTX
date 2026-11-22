
namespace Terraria.Network;

public sealed class RawPacketFrame
{
  private readonly byte[] _frameBytes;

  internal RawPacketFrame(byte[] frameBytes, byte messageId, ushort? moduleId)
  {
    _frameBytes = frameBytes;
    MessageId = messageId;
    ModuleId = moduleId;
  }

  public int TotalLength => _frameBytes.Length;
  public byte MessageId { get; }
  public ushort? ModuleId { get; }
  public ReadOnlyMemory<byte> FrameBytes => _frameBytes;
  public ReadOnlyMemory<byte> Payload => _frameBytes.AsMemory(PacketFrameCodec.HeaderBytes);
}
