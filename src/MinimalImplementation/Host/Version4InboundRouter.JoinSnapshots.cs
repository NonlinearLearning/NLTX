using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Combat.TagEffects;
using MinimalImplementation.Contracts;
using MinimalImplementation.Modules.Creative;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Modules.Protocol;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using PlayerEntity = MinimalImplementation.Server.Player.Player;

namespace MinimalImplementation.Server.Host;

public sealed partial class Version4InboundRouter {
  private IReadOnlyList<OutboundPacketIntent> CapturePlayerJoinIntents(
      SessionEpoch target, IReadOnlyList<ProjectileView> projectiles) {
    var intents = new List<OutboundPacketIntent>();
    foreach (SessionBinding binding in _composition.SessionBindings.CaptureBindings()) {
      if (binding.Epoch == target ||
          !_composition.SessionAdmission.TryGetStage(binding.Epoch, out SessionStage stage) ||
          stage != SessionStage.Active) {
        continue;
      }

      if (!_composition.Players.TryGetForBinding(binding, out PlayerEntity? player) ||
          player is null) {
        throw new InvalidOperationException("An active player's join snapshot is unavailable.");
      }

      PlayerJoinSnapshot snapshot = PlayerJoinSnapshot.Capture(binding, player);
      SyncProjectileTrackersPacket trackers = _composition.CapturePlayerProjectileTrackers(binding);
      foreach (PlayerPacketProjection projection in PlayerPacketProjection.From(snapshot, trackers)) {
        intents.Add(_composition.Version4WireProfile.CreateOutboundIntent(
          projection.Packet, [target], null, player.Version));
        if (projection.MessageId == SessionAdmissionCoordinator.SyncLoadoutMessageId &&
            CapturePlayerTagEffectIntent(binding, target) is OutboundPacketIntent tagEffect) {
          intents.Add(tagEffect);
        }
      }

      foreach (ProjectileView projectile in projectiles
          .Where(projectile => projectile.Owner == binding.PlayerSlot)
          .OrderBy(projectile => projectile.RuntimeSlot)) {
        if (projectile.OwnerEpoch != binding.Epoch) {
          throw new InvalidOperationException("A join projectile belongs to a stale player epoch.");
        }

        byte[] payload = ProjectilePacketPayloadCodec.EncodeSyncBody(
          projectile, _composition.ProjectileDefinitions);
        intents.Add(new OutboundPacketIntent(
          [target], null, 27, payload, projectile.CommitVersion));
      }
    }

    return intents.AsReadOnly();
  }

  private OutboundPacketIntent? CapturePlayerTagEffectIntent(
      SessionBinding binding, SessionEpoch target) {
    if (!Version4SteamModuleIdMap.SupportsLogicalModule(
        (ushort)Packet82ModuleId.TagEffectState)) {
      return null;
    }

    long worldScope = _composition.IdentityRegistry.CurrentWorldScope;
    if (worldScope <= 0 || _composition.CombatWorldScope != worldScope) {
      throw new InvalidOperationException("TagEffect owner has not joined the current world scope.");
    }

    if (!_composition.TagEffectStateOwner.TryCapture(
        binding.PlayerSlot, out TagEffectSnapshotView? snapshot)) {
      return null;
    }

    if (snapshot is null || snapshot.Player != binding.PlayerSlot ||
        snapshot.SessionEpoch != binding.Epoch) {
      throw new InvalidOperationException("The TagEffect snapshot belongs to a stale player binding.");
    }

    return new TagEffectModuleProducer(
      _composition.Packet82IntentProducer,
      _composition.CombatIdentityAuthority).CreateFullStateIntent(snapshot, [target]);
  }

  private IReadOnlyList<OutboundPacketIntent> CaptureWorldItemJoinIntents(
      SessionEpoch target, DateTimeOffset capturedAt) {
    var worldScope = new WorldScopeId(_composition.IdentityRegistry.CurrentWorldScope);
    if (!worldScope.IsValid || !_composition.WorldOwner.IsReady) {
      throw new InvalidOperationException("World items require a published current world scope.");
    }

    var resolver = new ActiveSessionPlayerSlotResolver(_composition.SessionAdmission);
    var intents = new List<OutboundPacketIntent>();
    foreach (WorldItemSnapshot item in _composition.WorldItemStore.CaptureWorld(worldScope)) {
      byte reservedForPlayer = byte.MaxValue;
      if (_composition.WorldItemReservationStore.TryGet(
          item.Handle, capturedAt, out WorldItemReservation? reservation) &&
          reservation is not null) {
        if (!resolver.TryResolve(reservation.PlayerId, reservation.Epoch, out reservedForPlayer) ||
            !_composition.Players.TryGetBySlot(
              reservedForPlayer, _composition.SessionAdmission, out _)) {
          throw new InvalidOperationException("A world-item reservation has no active player entity.");
        }
      }

      short itemIndex = checked((short)item.Handle.SlotId);
      SyncItemPacket itemPacket =
        Inventory.Protocol.WorldItemProjectionProducer.CreateItemPacket(item);
      intents.Add(_composition.Version4WireProfile.CreateOutboundIntent(
        itemPacket, [target], null, item.Revision));
      intents.Add(_composition.Version4WireProfile.CreateOutboundIntent(
        new ItemOwnerPacket {
          ItemIndex = itemIndex,
          ReservedForPlayer = reservedForPlayer,
          TimeToKeepReservation = item.Runtime?.KeepTime ?? 0,
          GrabDelayPlayer = item.Runtime?.OwnIgnore is >= 0 and < byte.MaxValue
            ? (byte)item.Runtime.OwnIgnore : byte.MaxValue,
          GrabDelayTime = item.Runtime?.OwnTime ?? 0,
          PositionX = item.Position.X,
          PositionY = item.Position.Y
        }, [target], null, item.Revision));
    }

    return intents.AsReadOnly();
  }

  private IReadOnlyList<OutboundPacketIntent> CaptureCreativeJoinIntents(SessionEpoch target) {
    CreativePowerSnapshot snapshot = _composition.CreativePowerStateOwner.CaptureSnapshot();
    var intents = new List<OutboundPacketIntent>();
    for (ushort powerId = 0; powerId < 15; powerId++) {
      intents.Add(_composition.Packet82IntentProducer.CreateIntent(
        (ushort)Packet82ModuleId.CreativePowerPermissions,
        new Packet82PermissionData(0, powerId, (byte)snapshot.Permissions[powerId]),
        [target],
        snapshot.Version));
    }

    // Version4 sends these states in registration order. Wind and rain sliders opt out;
    // immediate time buttons and the per-player spawn-rate slider have no join packet.
    foreach (ushort powerId in new ushort[] { 0, 5, 8, 9, 10, 11, 12, 13 }) {
      object state = powerId switch {
        5 or 11 => new Packet82PerPlayerTogglePowerState(
          snapshot.PlayerToggles[powerId].ToArray()),
        8 or 12 => new Packet82SharedSliderPowerState(snapshot.SharedSliders[powerId]),
        _ => new Packet82SharedTogglePowerState(snapshot.SharedToggles[powerId])
      };
      intents.Add(_composition.Packet82IntentProducer.CreateIntent(
        (ushort)Packet82ModuleId.CreativePowers,
        new Packet82CreativePowerData(powerId, state),
        [target],
        snapshot.Version));
    }

    return intents.AsReadOnly();
  }
}
