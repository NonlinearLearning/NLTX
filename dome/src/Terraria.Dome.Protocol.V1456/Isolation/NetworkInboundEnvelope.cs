using System;
using System.IO;
using Terraria.Dome.Protocol.V1456.Protocol;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public sealed class NetworkInboundEnvelope
{
  private NetworkInboundEnvelope(
    byte playerSlot,
    TerrariaMessageId messageId,
    ReadOnlyMemory<byte> frameBytes,
    ReadOnlyMemory<byte> payload)
  {
    PlayerSlot = playerSlot;
    MessageId = messageId;
    FrameBytes = frameBytes;
    Payload = payload;
  }

  public long Sequence { get; internal set; }

  public byte PlayerSlot { get; }

  public TerrariaMessageId MessageId { get; }

  public ReadOnlyMemory<byte> FrameBytes { get; }

  public ReadOnlyMemory<byte> Payload { get; }

  public static NetworkInboundEnvelope FromFrame(
    byte playerSlot,
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    return new NetworkInboundEnvelope(
      playerSlot,
      frame.MessageId,
      frameBytes.ToArray(),
      frame.Payload.ToArray());
  }
}
