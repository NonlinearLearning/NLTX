using MinimalImplementation.Contracts;
using MinimalImplementation.Modules.Creative;
using MinimalImplementation.Server.Modules;
using MinimalImplementation.Server.Modules.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Encodes and settles one complete Creative permission projection on the owner loop.
/// </summary>
public sealed class CreativePermissionDeliverySettlement : IOutboundDeliverySettlement {
  private const string _ownerKey = "CreativePermissions";
  private readonly CreativeModuleHandler _owner;
  private readonly CreativePermissionSnapshotBatch _batch;

  private CreativePermissionDeliverySettlement(
      CreativeModuleHandler owner,
      CreativePermissionSnapshotBatch batch,
      Packet82ModuleIntentProducer producer) {
    _owner = owner;
    _batch = batch;
    Guid batchId = Guid.NewGuid();
    var intents = new List<OutboundPacketIntent>(batch.Entries.Count);
    foreach (var entry in batch.Entries) {
      var token = new OutboundDeliveryToken(_ownerKey, batchId, batch.Target, intents.Count);
      intents.Add(producer.CreateIntent(
          (ushort)Packet82ModuleId.CreativePowerPermissions,
          entry, [batch.Target], batch.Version, token));
    }
    Intents = intents.AsReadOnly();
  }

  public string OwnerKey => _ownerKey;
  public IReadOnlyList<OutboundPacketIntent> Intents { get; }

  public static CreativePermissionDeliverySettlement? Capture(
      CreativeModuleHandler owner,
      Packet82ModuleIntentProducer producer,
      SessionEpoch target) {
    ArgumentNullException.ThrowIfNull(owner);
    ArgumentNullException.ThrowIfNull(producer);
    CreativePermissionSnapshotBatch? batch = owner.CaptureDirtyPermissionSnapshot(target);
    if (batch is null) {
      return null;
    }

    try {
      return new CreativePermissionDeliverySettlement(owner, batch, producer);
    } catch {
      // Encoding never admitted output; release the reservation before propagating the error.
      owner.AcknowledgePermissionSnapshot(batch, Array.Empty<SendReceipt>());
      throw;
    }
  }

  public OutboundDeliverySettlementResult Settle(
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    ModuleDeliveryResult validation = ModuleDeliveryReceiptEvaluator.Evaluate(
        Intents, _batch.Target, deliveries);
    if (!validation.IsSettled) {
      _owner.AcknowledgePermissionSnapshot(_batch, Array.Empty<SendReceipt>());
      return new OutboundDeliverySettlementResult(OwnerKey, false, validation.Detail);
    }

    SendReceipt[] receipts = deliveries.Select(delivery => delivery.Receipts[0]).ToArray();
    CreativePermissionDeliveryResult result = _owner.AcknowledgePermissionSnapshot(
        _batch, receipts);
    return new OutboundDeliverySettlementResult(OwnerKey, result.IsSettled, result.Detail);
  }
}
