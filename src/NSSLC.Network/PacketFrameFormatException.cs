using Terraria.Communication;

namespace Terraria.Network;

public enum PacketFrameErrorCode
{
  InvalidLengthPrefix,
  LengthOutOfRange,
  TruncatedBody,
  TruncatedModuleFrame,
  BufferLimitExceeded,
  ReceiveProgressTimeout
}

public sealed class PacketFrameFormatException : IOException
{
  public PacketFrameErrorCode ErrorCode { get; }

  public PacketFrameFormatException(PacketFrameErrorCode errorCode, string message)
    : base(message)
  {
    ErrorCode = errorCode;
  }
}
