using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules.Bestiary;

/// <summary>Settles one captured projection on the server commit thread.</summary>
public sealed class BestiarySnapshotDeliverySettlement : IOutboundDeliverySettlement {
  private readonly BestiaryModuleHandler _owner;
  private readonly BestiarySnapshotIntentBatch _batch;

  public BestiarySnapshotDeliverySettlement(
      BestiaryModuleHandler owner, BestiarySnapshotIntentBatch batch) {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    _batch = batch ?? throw new ArgumentNullException(nameof(batch));
  }

  public string OwnerKey => BestiarySnapshotIntentBatch.DeliveryOwnerKey;

  public IReadOnlyList<OutboundPacketIntent> Intents => _batch.Intents;

  public OutboundDeliverySettlementResult Settle(
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    ModuleDeliveryResult result = _owner.AcknowledgeSnapshot(_batch, deliveries);
    return new OutboundDeliverySettlementResult(OwnerKey, result.IsSettled, result.Detail);
  }
}
