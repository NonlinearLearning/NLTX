using System;
using System.Collections.Generic;

namespace MinimalImplementation.Contracts;

/// <summary>
/// Identifies one target-specific entry in an outbound owner delivery batch.
/// </summary>
public sealed record OutboundDeliveryToken
{
  public OutboundDeliveryToken(
    string ownerKey,
    Guid batchId,
    SessionEpoch target,
    int intentIndex)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(ownerKey);
    if (batchId == Guid.Empty)
    {
      throw new ArgumentException(
        "An outbound delivery batch requires a non-empty id.",
        nameof(batchId));
    }

    if (!target.IsValid)
    {
      throw new ArgumentException(
        "An outbound delivery token requires a valid target.",
        nameof(target));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(intentIndex);
    OwnerKey = ownerKey;
    BatchId = batchId;
    Target = target;
    IntentIndex = intentIndex;
  }

  public string OwnerKey { get; }

  public Guid BatchId { get; }

  public SessionEpoch Target { get; }

  public int IntentIndex { get; }
}

/// <summary>Associates one submitted intent with the receipts for that intent.</summary>
public sealed class OutboundIntentDelivery
{
  public OutboundIntentDelivery(
    OutboundPacketIntent intent,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    ArgumentNullException.ThrowIfNull(intent);
    ArgumentNullException.ThrowIfNull(receipts);
    Intent = intent;
    Receipts = Array.AsReadOnly(new List<SendReceipt>(receipts).ToArray());
  }

  public OutboundPacketIntent Intent { get; }

  public IReadOnlyList<SendReceipt> Receipts { get; }
}

/// <summary>
/// Reports the result of an owner-side local transport settlement. This is not client
/// acknowledgement; it only reports whether the owner accepted the supplied transport outcome.
/// </summary>
public sealed record OutboundDeliverySettlementResult(
  string OwnerKey,
  bool IsSettled,
  string? Detail)
{
  public static OutboundDeliverySettlementResult Invalid(string ownerKey, string detail) => new(
    ownerKey,
    false,
    detail);
}

/// <summary>
/// Receives the exact target-specific transport results for one committed owner batch.
/// Implementations must perform owner mutation on the server commit boundary.
/// </summary>
public interface IOutboundDeliverySettlement
{
  string OwnerKey { get; }

  IReadOnlyList<OutboundPacketIntent> Intents { get; }

  OutboundDeliverySettlementResult Settle(
    IReadOnlyList<OutboundIntentDelivery> deliveries);
}
