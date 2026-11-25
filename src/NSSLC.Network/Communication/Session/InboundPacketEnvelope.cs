using Terraria.Unshared.Session;
namespace Terraria.Network;

public sealed record InboundPacketEnvelope
{
  public SessionEpoch Epoch { get; }
  public PacketSequence Sequence { get; }
  public DateTimeOffset ReceivedAt { get; }
  public byte MessageId { get; }
  public ushort? ModuleId { get; }
  public ReadOnlyMemory<byte> RawFrame { get; }
  public string Stage { get; }
  public long? WorldScope { get; }
  public long? EntityGeneration { get; }
  public long? ExpectedRevision { get; }

  public InboundPacketEnvelope(
    SessionEpoch epoch,
    PacketSequence sequence,
    DateTimeOffset receivedAt,
    byte messageId,
    ushort? moduleId,
    ReadOnlyMemory<byte> rawFrame,
    string stage,
    long? worldScope,
    long? entityGeneration,
    long? expectedRevision)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("The envelope must carry a valid session epoch.", nameof(epoch));
    }

    if (!sequence.IsValid)
    {
      throw new ArgumentException("The envelope sequence cannot be negative.", nameof(sequence));
    }

    if (rawFrame.IsEmpty)
    {
      throw new ArgumentException("An inbound envelope must retain the bounded raw frame.", nameof(rawFrame));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(stage);
    Epoch = epoch;
    Sequence = sequence;
    ReceivedAt = receivedAt;
    MessageId = messageId;
    ModuleId = moduleId;
    RawFrame = rawFrame.ToArray();
    Stage = stage;
    WorldScope = worldScope;
    EntityGeneration = entityGeneration;
    ExpectedRevision = expectedRevision;
  }
}
