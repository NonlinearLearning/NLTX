using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules.Banners;

/// <summary>Settles one captured projection on the server commit thread.</summary>
public sealed class BannerSnapshotDeliverySettlement : IOutboundDeliverySettlement {
  private readonly BannerModuleHandler _owner;
  private readonly BannerSnapshotIntentBatch _batch;

  public BannerSnapshotDeliverySettlement(
      BannerModuleHandler owner, BannerSnapshotIntentBatch batch) {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    _batch = batch ?? throw new ArgumentNullException(nameof(batch));
  }

  public string OwnerKey => BannerSnapshotIntentBatch.DeliveryOwnerKey;

  public IReadOnlyList<OutboundPacketIntent> Intents => _batch.Intents;

  public OutboundDeliverySettlementResult Settle(
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    ModuleDeliveryResult result = _owner.AcknowledgeSnapshot(_batch, deliveries);
    return new OutboundDeliverySettlementResult(OwnerKey, result.IsSettled, result.Detail);
  }
}
