using System.Buffers.Binary;

namespace MinimalImplementation.Server.Protocol;

public static class PacketFrameCodec
{
  public const int LengthPrefixBytes = sizeof(ushort);
  public const int MessageIdBytes = sizeof(byte);
  public const int HeaderBytes = LengthPrefixBytes + MessageIdBytes;
  public const int ModuleHeaderBytes = HeaderBytes + sizeof(ushort);
  public const int MinimumFrameBytes = HeaderBytes;
  public const int MaximumFrameBytes = ushort.MaxValue;
  public const int MaximumPayloadBytes = MaximumFrameBytes - HeaderBytes;
  public const byte NetModulesMessageId = 82;

  public static byte[] Encode(byte messageId, ReadOnlySpan<byte> payload)
  {
    if (payload.Length > MaximumPayloadBytes)
    {
      throw new PacketFrameFormatException(
        PacketFrameErrorCode.LengthOutOfRange,
        $"Packet payload length {payload.Length} exceeds the Version4 limit of {MaximumPayloadBytes} bytes.");
    }

    if (messageId == NetModulesMessageId && payload.Length < sizeof(ushort))
    {
      throw new PacketFrameFormatException(
        PacketFrameErrorCode.TruncatedModuleFrame,
        "Packet 82 requires a UInt16 module id after its message id.");
    }

    int totalLength = HeaderBytes + payload.Length;
    byte[] frame = new byte[totalLength];
    BinaryPrimitives.WriteUInt16LittleEndian(frame, (ushort)totalLength);
    frame[LengthPrefixBytes] = messageId;
    payload.CopyTo(frame.AsSpan(HeaderBytes));
    return frame;
  }

  internal static RawPacketFrame ParseOwnedFrame(byte[] frameBytes)
  {
    byte messageId = frameBytes[LengthPrefixBytes];
    ushort? moduleId = null;
    if (messageId == NetModulesMessageId)
    {
      if (frameBytes.Length < ModuleHeaderBytes)
      {
        throw new PacketFrameFormatException(
          PacketFrameErrorCode.TruncatedModuleFrame,
          "Packet 82 requires a five-byte frame header including its module id.");
      }

      moduleId = BinaryPrimitives.ReadUInt16LittleEndian(
        frameBytes.AsSpan(HeaderBytes, sizeof(ushort)));
    }

    return new RawPacketFrame(frameBytes, messageId, moduleId);
  }
}
