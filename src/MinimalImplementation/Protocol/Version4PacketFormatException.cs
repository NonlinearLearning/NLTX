using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Protocol;

public sealed class Version4PacketFormatException : IOException
{
  public Version4PacketFormatException(
    byte messageId,
    PacketReadErrorCode? errorCode,
    int offset,
    string message)
    : base(message)
  {
    MessageId = messageId;
    ErrorCode = errorCode;
    Offset = offset;
  }

  public byte MessageId { get; }

  public PacketReadErrorCode? ErrorCode { get; }

  public int Offset { get; }
}
