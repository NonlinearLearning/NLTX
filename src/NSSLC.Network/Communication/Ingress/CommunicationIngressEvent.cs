using Terraria.Share.Relationships;
using Terraria.Unshared.Session;

namespace Terraria.Network;

public sealed class CommunicationIngressEvent
{
  private readonly byte[] _frameBytes;

  internal CommunicationIngressEvent(
      CommunicationIngressEventKind kind,
      long ingressSequence,
      SessionEpoch epoch,
      PacketSequence packetSequence,
      DateTimeOffset receivedAt,
      byte messageId,
      ushort? moduleId,
      ReadOnlyMemory<byte> frameBytes,
      int payloadOffset,
      string? closeReason)
  {
    if (!Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(nameof(kind));
    }

    if (ingressSequence <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ingressSequence));
    }

    if (!epoch.IsValid)
    {
      throw new ArgumentException(nameof(epoch));
    }

    if (kind == CommunicationIngressEventKind.Packet)
    {
      if (!packetSequence.IsValid)
      {
        throw new ArgumentException(nameof(packetSequence));
      }

      if (frameBytes.IsEmpty || payloadOffset < 0 || payloadOffset > frameBytes.Length)
      {
        throw new ArgumentException(nameof(frameBytes));
      }
    }

    if (kind == CommunicationIngressEventKind.Close && string.IsNullOrWhiteSpace(closeReason))
    {
      throw new ArgumentException(nameof(closeReason));
    }

    Kind = kind;
    IngressSequence = ingressSequence;
    Epoch = epoch;
    PacketSequence = packetSequence;
    ReceivedAt = receivedAt;
    MessageId = messageId;
    ModuleId = moduleId;
    _frameBytes = frameBytes.ToArray();
    PayloadOffset = payloadOffset;
    CloseReason = closeReason;
  }

  public CommunicationIngressEventKind Kind { get; }

  public long IngressSequence { get; }

  public SessionEpoch Epoch { get; }

  public PacketSequence PacketSequence { get; }

  public DateTimeOffset ReceivedAt { get; }

  public byte MessageId { get; }

  public ushort? ModuleId { get; }

  public int PayloadOffset { get; }

  public string? CloseReason { get; }

  public ReadOnlyMemory<byte> FrameBytes => _frameBytes.ToArray();
}
