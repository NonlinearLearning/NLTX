using System;
using System.IO;

namespace Terraria.Dome.Protocol.V1456.Protocol;

public static class TerrariaFrameCodec
{
  private const int HeaderLength = 3;
  private const int LengthPrefixLength = 2;

  public static TerrariaFrame Decode(ReadOnlySpan<byte> frameBytes)
  {
    if (frameBytes.Length < TerrariaProtocolVersion.MinimumFrameLength)
    {
      throw new InvalidDataException("Terraria frame is shorter than its header.");
    }

    int frameLength = frameBytes[0] | frameBytes[1] << 8;
    if (frameLength < TerrariaProtocolVersion.MinimumFrameLength)
    {
      throw new InvalidDataException("Terraria frame declares an invalid length.");
    }

    if (frameLength != frameBytes.Length)
    {
      throw new InvalidDataException("Terraria frame length does not match its payload.");
    }

    TerrariaMessageId messageId = (TerrariaMessageId)frameBytes[LengthPrefixLength];
    byte[] payload = frameBytes[HeaderLength..].ToArray();
    return new TerrariaFrame(messageId, payload);
  }

  public static byte[] Encode(TerrariaFrame frame)
  {
    int frameLength = HeaderLength + frame.Payload.Length;
    if (frameLength > TerrariaProtocolVersion.MaximumFrameLength)
    {
      throw new InvalidDataException("Terraria frame exceeds the protocol maximum length.");
    }

    byte[] frameBytes = new byte[frameLength];
    frameBytes[0] = (byte)frameLength;
    frameBytes[1] = (byte)(frameLength >> 8);
    frameBytes[LengthPrefixLength] = (byte)frame.MessageId;
    frame.Payload.Span.CopyTo(frameBytes.AsSpan(HeaderLength));
    return frameBytes;
  }
}
