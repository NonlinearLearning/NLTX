using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Host;

/// <summary>Settles committed NPC snapshots and retires deaths after local submission.</summary>
public sealed class NpcSnapshotDeliverySettlement : IOutboundDeliverySettlement {
  private readonly NpcManager _manager;
  private readonly IReadOnlyList<NpcView> _snapshots;
  private readonly Action _completed;

  public NpcSnapshotDeliverySettlement(
      NpcManager manager, IReadOnlyList<NpcView> snapshots,
      IReadOnlyList<OutboundPacketIntent> intents, Action completed) {
    _manager = manager;
    _snapshots = snapshots;
    Intents = intents;
    _completed = completed;
  }

  public string OwnerKey => "NpcSnapshots";

  public IReadOnlyList<OutboundPacketIntent> Intents { get; }

  public OutboundDeliverySettlementResult Settle(
      IReadOnlyList<OutboundIntentDelivery> deliveries) {
    bool allSent = deliveries.Count == Intents.Count && deliveries.All(delivery =>
        delivery.Intent.TargetEpochs.All(target => delivery.Receipts.Any(receipt =>
            receipt.Epoch == target && receipt.Certainty == TransportSendCertainty.LocallySent)));
    if (allSent) {
      foreach (NpcView snapshot in _snapshots) {
        // Keep a zero-life actor dirty and occupying its slot until terminal sync succeeds.
        // New joins also capture that terminal state; failed sends retry in periodic sync.
        if (snapshot.Life == 0 &&
            _manager.TryGet(snapshot.Entity, out NpcView? current) && current is not null &&
            current.Revision == snapshot.Revision) {
          var retirement = _manager.PrepareRetire(snapshot.Entity);
          if (retirement.IsPrepared && retirement.Plan is not null) {
            var retired = _manager.Commit(retirement.Plan);
            if (retired.Result.Status == OwnerCommitStatus.Applied) {
              ServerLog.Info("server.npc.retired",
                  $"slot={snapshot.Slot} entity={snapshot.Entity} life=0");
              continue;
            }
          }
          // A failed retirement remains dirty for another settlement attempt.
          allSent = false;
          continue;
        }
        _manager.AcknowledgeSnapshot(snapshot);
      }
    }
    _completed();
    return new OutboundDeliverySettlementResult(OwnerKey, allSent,
        allSent ? null : "NPC snapshots remain dirty until local transport submission succeeds.");
  }
}
