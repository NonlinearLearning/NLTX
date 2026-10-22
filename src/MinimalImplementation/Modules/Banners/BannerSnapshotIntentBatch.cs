using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules.Banners;

/// <summary>
/// One Banner full-state projection captured for one target and one owner version.
/// </summary>
public sealed class BannerSnapshotIntentBatch
{
  public const string DeliveryOwnerKey = "Banner";

  public BannerSnapshotIntentBatch(
    SessionEpoch target,
    CommitVersion version,
    OutboundPacketIntent intent)
  {
    if (!target.IsValid)
    {
      throw new ArgumentException(
        "A Banner projection target must be a valid epoch.",
        nameof(target));
    }

    if (!version.IsValid)
    {
      throw new ArgumentException(
        "A Banner projection requires a valid owner version.",
        nameof(version));
    }

    ArgumentNullException.ThrowIfNull(intent);
    if (intent.CommitVersion != version || intent.TargetEpochs.Count != 1
        || intent.TargetEpochs[0] != target || intent.IgnoreEpoch == target)
    {
      throw new ArgumentException(
        "The Banner projection intent must target the batch epoch and version.",
        nameof(intent));
    }

    Target = target;
    Version = version;
    Intent = new OutboundPacketIntent(
      intent.TargetEpochs,
      intent.IgnoreEpoch,
      intent.MessageId,
      intent.Payload,
      intent.CommitVersion,
      new OutboundDeliveryToken(DeliveryOwnerKey, Guid.NewGuid(), target, 0));
    Intents = Array.AsReadOnly(new[] { Intent });
  }

  public SessionEpoch Target { get; }

  public CommitVersion Version { get; }

  public OutboundPacketIntent Intent { get; }

  public IReadOnlyList<OutboundPacketIntent> Intents { get; }
}
