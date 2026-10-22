using MinimalImplementation.Contracts;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Modules.Banners;
using MinimalImplementation.Server.Modules.Bestiary;

namespace MinimalImplementation.Server.Host;

/// <summary>Captures versioned module batches on the owner loop for receipt settlement.</summary>
public sealed class ModulePeriodicSyncOwner : IServerTickPhaseOwner {
  private readonly ServerComposition _composition;
  private readonly ServerOutboundDeliveryPump _delivery;
  private readonly WorldItemProjectionProducer _items;
  private bool _npcDeliveryPending;

  public ModulePeriodicSyncOwner(
      ServerComposition composition, ServerOutboundDeliveryPump delivery) {
    _composition = composition ?? throw new ArgumentNullException(nameof(composition));
    _delivery = delivery ?? throw new ArgumentNullException(nameof(delivery));
    _items = new WorldItemProjectionProducer(composition.WorldItemStore.Projections,
      composition.Version4WireProfile,
      new ActiveSessionPlayerSlotResolver(composition.SessionAdmission));
  }

  public ServerTickPhase Phase => ServerTickPhase.PeriodicSync;
  public object TransactionIdentity => this;

  public void Advance(long gameTickIndex) {
    ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);
    var npcTargets = _composition.SessionAdmission.CaptureActiveSessionEpochs();
    if (gameTickIndex % 6 == 0 && !_npcDeliveryPending && npcTargets.Count > 0) {
      var snapshots = _composition.NpcManager.CaptureDirty();
      if (snapshots.Count > 0) {
        var intents = snapshots.Select(npc =>
            _composition.NpcNetwork.CreateSync(npc, npcTargets)).ToArray();
        _npcDeliveryPending = true;
        _delivery.TryEnqueue(new NpcSnapshotDeliverySettlement(
            _composition.NpcManager, snapshots, intents, () => _npcDeliveryPending = false));
      }
    }
    WorldScopeId scope = new(_composition.IdentityRegistry.CurrentWorldScope);
    if (scope.IsValid) {
      _composition.WorldItemStore.Projections.SetTargets(scope,
        _composition.SessionAdmission.CaptureActiveSessionEpochs());
    }
    foreach (SessionEpoch epoch in _composition.SessionAdmission.CaptureActiveSessionEpochs()) {
      foreach (WorldItemProjection item in
          _composition.WorldItemStore.Projections.CapturePending(epoch)) {
        if (_items.TryPrepare(Guid.NewGuid(), item, out IOutboundDeliverySettlement? settlement)
            && settlement is not null) {
          if (!_delivery.TryEnqueue(settlement)) {
            break;
          }
        }
      }
      BestiarySnapshotIntentBatch? bestiary = _composition.BestiaryModuleHandler
          .CaptureDirtySnapshot(epoch, BestiaryNpcCatalog.NetIds);
      if (bestiary is not null) {
        _delivery.TryEnqueue(new BestiarySnapshotDeliverySettlement(
            _composition.BestiaryModuleHandler, bestiary));
      }
      BannerSnapshotIntentBatch? banner = _composition.BannerModuleHandler.CaptureDirtySnapshot(epoch);
      if (banner is not null) {
        _delivery.TryEnqueue(new BannerSnapshotDeliverySettlement(
            _composition.BannerModuleHandler, banner));
      }
      CreativePermissionDeliverySettlement? permissions =
          CreativePermissionDeliverySettlement.Capture(
              _composition.CreativeModuleHandler, _composition.Packet82IntentProducer, epoch);
      if (permissions is not null) {
        _delivery.TryEnqueue(permissions);
      }
    }
  }

  public static void ValidateNpcCatalog(BestiarySnapshot snapshot) {
    BestiaryNpcCatalog.Validate(snapshot);
  }
}
