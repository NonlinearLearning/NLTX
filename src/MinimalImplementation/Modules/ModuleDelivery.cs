using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules;

/// <summary>Transport-level settlement observed for one P07 outbound intent batch.</summary>
public enum ModuleDeliveryStatus
{
  Settled,
  Queued,
  LocallySent,
  NotSubmitted,
  OutcomeUnknown,
  Failed,
  InvalidReceipt,
  StaleVersion
}

/// <summary>
/// Describes local transport settlement. A settled result is not a client acknowledgement; it
/// only means that the supplied intents entered the local outbound stream or socket.
/// </summary>
public sealed record ModuleDeliveryResult(
  ModuleDeliveryStatus Status,
  string? Detail)
{
  public bool IsSettled => Status is ModuleDeliveryStatus.Settled
    or ModuleDeliveryStatus.Queued
    or ModuleDeliveryStatus.LocallySent;

  public static ModuleDeliveryResult Invalid(string detail) => new(
    ModuleDeliveryStatus.InvalidReceipt,
    detail);

  public static ModuleDeliveryResult Stale(string detail) => new(
    ModuleDeliveryStatus.StaleVersion,
    detail);
}

/// <summary>
/// Validates the one-to-one receipt batch returned for a P07 projection batch. Callers must pass
/// the exact intent batch that was sent; the receipt type itself does not carry a commit version.
/// </summary>
public static class ModuleDeliveryReceiptEvaluator
{
  public static ModuleDeliveryResult Evaluate(
      IReadOnlyList<OutboundPacketIntent> intents,
      SessionEpoch target,
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    ArgumentNullException.ThrowIfNull(intents);
    ArgumentNullException.ThrowIfNull(deliveries);
    if (deliveries.Count != intents.Count) {
      return ModuleDeliveryResult.Invalid(
          "The settlement must contain one delivery record per captured intent.");
    }

    var receipts = new List<SendReceipt>(deliveries.Count);
    for (int index = 0; index < deliveries.Count; index++) {
      OutboundIntentDelivery? delivery = deliveries[index];
      if (delivery is null || !ReferenceEquals(delivery.Intent, intents[index])) {
        return ModuleDeliveryResult.Invalid(
            "The settlement must preserve captured intent identity and order.");
      }

      if (delivery.Receipts.Count != 1) {
        return ModuleDeliveryResult.Invalid(
            "A target-specific intent requires exactly one transport receipt.");
      }

      receipts.Add(delivery.Receipts[0]);
    }

    return Evaluate(intents, target, receipts);
  }

  public static ModuleDeliveryResult Evaluate(
    IReadOnlyList<OutboundPacketIntent> intents,
    SessionEpoch target,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    ArgumentNullException.ThrowIfNull(intents);
    ArgumentNullException.ThrowIfNull(receipts);
    if (!target.IsValid)
    {
      return ModuleDeliveryResult.Invalid(
        "A module delivery target must be a valid session epoch.");
    }

    if (intents.Count == 0)
    {
      if (receipts.Count != 0)
      {
        return ModuleDeliveryResult.Invalid(
          "A projection batch with no outbound intents cannot have transport receipts.");
      }

      return new ModuleDeliveryResult(
        ModuleDeliveryStatus.Settled,
        "The projection batch contains no outbound intents.");
    }

    CommitVersion batchVersion = default;
    for (int index = 0; index < intents.Count; index++)
    {
      OutboundPacketIntent intent = intents[index]
        ?? throw new ArgumentException(
          "A module delivery batch cannot contain a null intent.",
          nameof(intents));
      if (!intent.TargetEpochs.Contains(target)
          || intent.IgnoreEpoch == target
          || !intent.CommitVersion.IsValid)
      {
        return ModuleDeliveryResult.Invalid(
          "Every intent must target the acknowledged epoch and carry a valid commit version.");
      }

      if (index == 0)
      {
        batchVersion = intent.CommitVersion;
      }
      else if (intent.CommitVersion != batchVersion)
      {
        return ModuleDeliveryResult.Invalid(
          "A module delivery batch must use one committed version for every intent.");
      }
    }

    if (receipts.Count == 0)
    {
      return new ModuleDeliveryResult(
        ModuleDeliveryStatus.NotSubmitted,
        "No transport receipt was returned for the projection batch.");
    }

    if (receipts.Count != intents.Count)
    {
      return ModuleDeliveryResult.Invalid(
        "The transport receipt count does not match the outbound intent count.");
    }

    var sequences = new HashSet<long>();
    foreach (SendReceipt receipt in receipts)
    {
      if (receipt is null
          || receipt.Epoch != target
          || !receipt.Sequence.IsValid
          || !sequences.Add(receipt.Sequence.Value))
      {
        return ModuleDeliveryResult.Invalid(
          "Every receipt must identify the target epoch with a unique transport sequence.");
      }
    }

    bool queued = false;
    bool locallySent = true;
    foreach (SendReceipt receipt in receipts)
    {
      switch (receipt.Certainty)
      {
        case TransportSendCertainty.Failed:
          return new ModuleDeliveryResult(
            ModuleDeliveryStatus.Failed,
            receipt.ErrorCode is ProtocolErrorCode error
              ? $"The module intent failed with protocol error {error}."
              : "The module intent failed before local delivery.");
        case TransportSendCertainty.OutcomeUnknown:
          return new ModuleDeliveryResult(
            ModuleDeliveryStatus.OutcomeUnknown,
            "The transport outcome is unknown; the projection remains dirty.");
        case TransportSendCertainty.NotSubmitted:
          return new ModuleDeliveryResult(
            ModuleDeliveryStatus.NotSubmitted,
            "The module intent did not enter the local outbound stream.");
        case TransportSendCertainty.Queued:
          queued = true;
          locallySent = false;
          break;
        case TransportSendCertainty.LocallySent:
          break;
        default:
          return ModuleDeliveryResult.Invalid("The transport returned an unknown certainty value.");
      }
    }

    return new ModuleDeliveryResult(
      locallySent ? ModuleDeliveryStatus.LocallySent : ModuleDeliveryStatus.Queued,
      queued
        ? "Every module intent was queued; local socket delivery is not yet proven."
        : "Every module intent was locally sent.");
  }
}
