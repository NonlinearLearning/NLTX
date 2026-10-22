using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Modules.Creative;

/// <summary>
/// Adapts target-specific transport receipts to the Creative owner acknowledgement boundary.
/// The router invokes this only from the server commit loop.
/// </summary>
public sealed class CreativePowerEffectDeliverySettlement : IOutboundDeliverySettlement
{
  private readonly CreativeModuleHandler _owner;
  private readonly CreativePowerEffectDeliveryBatch _batch;

  public CreativePowerEffectDeliverySettlement(
    CreativeModuleHandler owner,
    CreativePowerEffectDeliveryBatch batch)
  {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    _batch = batch ?? throw new ArgumentNullException(nameof(batch));
  }

  public string OwnerKey => CreativePowerEffectDeliveryBatch.DeliveryOwnerKey;

  public IReadOnlyList<OutboundPacketIntent> Intents => _batch.Intents;

  /// <summary>All original intents were locally accepted or their target epochs retired.</summary>
  public bool IsSettled => _owner.IsEffectDeliverySettled(_batch);

  /// <summary>
  /// After the current attempt reports, captures only pending original intents with fresh tokens.
  /// Retain this settlement until IsSettled and submit each returned handle once.
  /// Unknown outcomes may duplicate messages; retry scheduling belongs to the caller.
  /// </summary>
  public CreativePowerEffectDeliverySettlement? CapturePendingDelivery() {
    CreativePowerEffectDeliveryBatch? batch = _owner.CapturePendingEffectDelivery(_batch);
    return batch is null ? null : new CreativePowerEffectDeliverySettlement(_owner, batch);
  }

  public OutboundDeliverySettlementResult Settle(
    IReadOnlyList<OutboundIntentDelivery> deliveries)
  {
    ArgumentNullException.ThrowIfNull(deliveries);
    CreativePowerEffectDeliveryResult result = _owner.SettleEffectDelivery(
      _batch,
      deliveries);
    return new OutboundDeliverySettlementResult(OwnerKey, result.IsSettled, result.Detail);
  }
}
