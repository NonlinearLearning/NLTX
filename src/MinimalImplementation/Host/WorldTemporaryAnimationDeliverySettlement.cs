using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Host;

/// <summary>Settles the local transport outcome for one packet 77 broadcast.</summary>
public sealed class WorldTemporaryAnimationDeliverySettlement : IOutboundDeliverySettlement
{
  private const string DeliveryOwnerKey = "WorldTemporaryAnimation";

  public WorldTemporaryAnimationDeliverySettlement(
      OutboundPacketIntent intent,
      ulong worldEpoch)
  {
    ArgumentNullException.ThrowIfNull(intent);
    if (worldEpoch == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldEpoch));
    }

    if (intent.IgnoreEpoch is not null)
    {
      throw new ArgumentException(
        "A temporary-animation settlement requires a broadcast intent without an ignored epoch.",
        nameof(intent));
    }

    var targets = new HashSet<SessionEpoch>();
    foreach (SessionEpoch target in intent.TargetEpochs)
    {
      if (!target.IsValid || !targets.Add(target))
      {
        throw new ArgumentException(
          "A temporary-animation settlement requires unique valid target epochs.",
          nameof(intent));
      }
    }

    WorldEpoch = worldEpoch;
    Intents = Array.AsReadOnly(new[] { intent });
  }

  public string OwnerKey => DeliveryOwnerKey;

  public ulong WorldEpoch { get; }

  public IReadOnlyList<OutboundPacketIntent> Intents { get; }

  public OutboundDeliverySettlementResult Settle(
      IReadOnlyList<OutboundIntentDelivery> deliveries)
  {
    ArgumentNullException.ThrowIfNull(deliveries);
    if (deliveries.Count != Intents.Count)
    {
      return OutboundDeliverySettlementResult.Invalid(
        OwnerKey,
        "The temporary-animation settlement must retain one delivery record per intent.");
    }

    OutboundIntentDelivery? delivery = deliveries[0];
    OutboundPacketIntent intent = Intents[0];
    if (delivery is null || !ReferenceEquals(delivery.Intent, intent))
    {
      return OutboundDeliverySettlementResult.Invalid(
        OwnerKey,
        "The temporary-animation settlement must preserve captured intent identity and order.");
    }

    var receiptTargets = new HashSet<SessionEpoch>();
    foreach (SendReceipt? receipt in delivery.Receipts)
    {
      if (receipt is null || !intent.TargetEpochs.Contains(receipt.Epoch) ||
          !receiptTargets.Add(receipt.Epoch))
      {
        return OutboundDeliverySettlementResult.Invalid(
          OwnerKey,
          "Every temporary-animation receipt must match one unique intent target.");
      }
    }

    if (delivery.Receipts.Count != intent.TargetEpochs.Count)
    {
      return new OutboundDeliverySettlementResult(
        OwnerKey,
        false,
        "The temporary-animation transport did not return one receipt for every target.");
    }

    foreach (SendReceipt receipt in delivery.Receipts)
    {
      if (receipt.Certainty is not (TransportSendCertainty.Queued or
          TransportSendCertainty.LocallySent))
      {
        return new OutboundDeliverySettlementResult(
          OwnerKey,
          false,
          receipt.Certainty switch
          {
            TransportSendCertainty.Failed => "The temporary-animation transport reported a failure.",
            TransportSendCertainty.OutcomeUnknown =>
              "The temporary-animation transport outcome is unknown.",
            TransportSendCertainty.NotSubmitted =>
              "The temporary-animation intent did not enter the local outbound stream.",
            _ => "The temporary-animation transport returned an unsupported certainty."
          });
      }
    }

    return new OutboundDeliverySettlementResult(
      OwnerKey,
      true,
      "Every temporary-animation target was queued or locally sent.");
  }
}
