using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules.Bestiary;

/// <summary>
/// All Bestiary projection intents captured for one target and one owner version.
/// </summary>
public sealed class BestiarySnapshotIntentBatch
{
  public const string DeliveryOwnerKey = "Bestiary";

  public BestiarySnapshotIntentBatch(
    SessionEpoch target,
    CommitVersion version,
    IReadOnlyList<OutboundPacketIntent> intents)
  {
    if (!target.IsValid)
    {
      throw new ArgumentException(
        "A Bestiary projection target must be a valid epoch.",
        nameof(target));
    }

    if (!version.IsValid)
    {
      throw new ArgumentException(
        "A Bestiary projection requires a valid owner version.",
        nameof(version));
    }

    ArgumentNullException.ThrowIfNull(intents);
    Guid batchId = Guid.NewGuid();
    List<OutboundPacketIntent> deliveries = new(intents.Count);
    foreach (OutboundPacketIntent intent in intents)
    {
      ArgumentNullException.ThrowIfNull(intent);
      if (intent.CommitVersion != version || intent.TargetEpochs.Count != 1
          || intent.TargetEpochs[0] != target || intent.IgnoreEpoch == target)
      {
        throw new ArgumentException(
          "Every Bestiary projection intent must target the batch epoch and version.",
          nameof(intents));
      }

      deliveries.Add(new OutboundPacketIntent(
        intent.TargetEpochs,
        intent.IgnoreEpoch,
        intent.MessageId,
        intent.Payload,
        intent.CommitVersion,
        new OutboundDeliveryToken(DeliveryOwnerKey, batchId, target, deliveries.Count)));
    }

    Target = target;
    Version = version;
    Intents = Array.AsReadOnly(deliveries.ToArray());
  }

  public SessionEpoch Target { get; }

  public CommitVersion Version { get; }

  public IReadOnlyList<OutboundPacketIntent> Intents { get; }
}
