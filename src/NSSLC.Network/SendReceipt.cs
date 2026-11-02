using Terraria.Communication;

namespace Terraria.Network;

public sealed record SendReceipt
{
  public TransportSendCertainty Certainty { get; }
  public SessionEpoch Epoch { get; }
  public PacketSequence Sequence { get; }
  public int BytesSubmitted { get; }
  public ProtocolErrorCode? ErrorCode { get; }

  public SendReceipt(
    TransportSendCertainty certainty,
    SessionEpoch epoch,
    PacketSequence sequence,
    int bytesSubmitted,
    ProtocolErrorCode? errorCode)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A send receipt must identify a valid session epoch.", nameof(epoch));
    }

    if (!sequence.IsValid)
    {
      throw new ArgumentException("A send receipt sequence cannot be negative.", nameof(sequence));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(bytesSubmitted);
    if (certainty == TransportSendCertainty.Failed && errorCode is null)
    {
      throw new ArgumentException("Failed sends must carry a protocol or transport error.", nameof(errorCode));
    }

    Certainty = certainty;
    Epoch = epoch;
    Sequence = sequence;
    BytesSubmitted = bytesSubmitted;
    ErrorCode = errorCode;
  }
}
