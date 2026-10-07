using EntityEcs;
using EntityEcs.Components;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Player;
using Terraria.Relationships;
using Terraria.WorldStorage;
using ItemEntityRef = Terraria.Relationships.ItemEntityRef;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed partial class RuntimeWorldItemStore
{
  private readonly WorldItemReservationSystem _reservationSystem = new();

  internal bool TryApplyNetworkSync(
    in RuntimeWorldItemNetworkCommand command,
    RuntimePlayerStore players,
    RuntimeEntityId ownerEntity,
    byte ownerPlayerSlot,
    long currentTick,
    int itemLifetimeTicks,
    long reservationDurationTicks,
    out RuntimeWorldItemNetworkProjection projection,
    out string rejectionCode)
  {
    projection = default;
    rejectionCode = "InvalidWorldItemState";
    if (currentTick < 0 || itemLifetimeTicks <= 0 || reservationDurationTicks <= 0 ||
      command.TypeId <= 0 || command.Stack <= 0 || command.PrefixId < 0 ||
      !float.IsFinite(command.PositionX) || !float.IsFinite(command.PositionY) ||
      !float.IsFinite(command.VelocityX) || !float.IsFinite(command.VelocityY) ||
      ownerEntity.IsEmpty ||
      ownerEntity.Reference.Scope != EntityReferenceScope.Player ||
      ownerEntity.Reference.RuntimeId != _session.EntityRuntime.RuntimeId)
    {
      return false;
    }

    if (command.ItemSlot == MaximumWorldItems)
    {
      return TryCreateNetworkItem(
        command,
        players,
        ownerEntity,
        ownerPlayerSlot,
        currentTick,
        itemLifetimeTicks,
        reservationDurationTicks,
        out projection,
        out rejectionCode);
    }

    if (command.ItemSlot < 0 || command.ItemSlot >= MaximumWorldItems)
    {
      rejectionCode = "InvalidWorldItemSlot";
      return false;
    }

    EntitySlotStore<WorldEntityState, WorldItemSlot> items = _session.Storage.WorldItems;
    if (!items.TryGetOccupiedAt(
          command.ItemSlot,
          out WorldItemSlot slot,
          out uint generation,
          out WorldEntityState? state) ||
        state is not RuntimeWorldItemState worldItemState ||
        !items.TryGet(slot, generation, out WorldEntityState? currentState) ||
        !ReferenceEquals(state, currentState))
    {
      rejectionCode = "InactiveWorldItemSlot";
      return false;
    }

    if (!TryCaptureNetworkItem(
          worldItemState.Entity,
          currentTick,
          out PlayerInventoryItemSnapshot item,
          out RuntimeWorldItemProjection worldItem,
          out WorldItemStateSnapshot worldState,
          out WorldItemReservationSnapshot reservation,
          out RuntimeEntityHandle handle))
    {
      rejectionCode = "StaleWorldItemSlot";
      return false;
    }

    if (!worldItem.WorldItem.Value.IsExpiredAt(currentTick) &&
        reservation.Value.HasReservationAt(currentTick) &&
        reservation.Value.ReservedFor == ownerEntity &&
        reservation.Value.ReservationId is { IsAssigned: true })
    {
      // Continue below after the identity, lifecycle, and owner checks.
    }
    else
    {
      rejectionCode = "WorldItemReservationOwnerMismatch";
      return false;
    }

    if (!_registry.TryApplyNetworkSync(
          item.Entity,
          command.TypeId,
          command.Stack,
          command.PrefixId,
          new LocationComponent(command.PositionX, command.PositionY),
          new VelocityComponent(command.VelocityX, command.VelocityY)))
    {
      rejectionCode = "WorldItemUpdateConflict";
      return false;
    }

    projection = CreateProjection(
      command.ItemSlot,
      command,
      players,
      ownerPlayerSlot,
      reservation.Value,
      currentTick);
    _currentTick = currentTick;
    rejectionCode = string.Empty;
    return true;
  }

  private bool TryCreateNetworkItem(
    in RuntimeWorldItemNetworkCommand command,
    RuntimePlayerStore players,
    RuntimeEntityId ownerEntity,
    byte ownerPlayerSlot,
    long currentTick,
    int itemLifetimeTicks,
    long reservationDurationTicks,
    out RuntimeWorldItemNetworkProjection projection,
    out string rejectionCode)
  {
    projection = default;
    rejectionCode = "WorldItemCapacityExceeded";
    EntitySlotStore<WorldEntityState, WorldItemSlot> items = _session.Storage.WorldItems;
    if (items.ActiveCount >= MaximumWorldItems)
    {
      return false;
    }

    if (!_session.EntityRuntime.TryResolve(
          ownerEntity.Reference,
          out RuntimeEntityHandle ownerHandle) ||
        !_session.EntityRuntime.TryGetStatus(ownerHandle, out EntityRuntimeStatus ownerStatus) ||
        ownerStatus != EntityRuntimeStatus.Running)
    {
      rejectionCode = "InvalidPlayerIdentity";
      return false;
    }

    PlayerInventoryItemSnapshot item;
    try
    {
      item = _registry.CreateWorldDrop(
        command.TypeId,
        command.Stack,
        command.PositionX,
        command.PositionY,
        command.VelocityX,
        command.VelocityY,
        new ColliderComponent(16.0f, 16.0f),
        currentTick,
        itemLifetimeTicks,
        NextReplicationId(),
        command.PrefixId);
    }
    catch (ArgumentException)
    {
      rejectionCode = "InvalidWorldItemDefinition";
      return false;
    }
    catch (InvalidDataException)
    {
      rejectionCode = "UnsupportedWorldItemDefinition";
      return false;
    }

    if (!items.TryAllocate(
          new RuntimeWorldItemState(item.Entity),
          out WorldItemSlot allocatedSlot,
          out uint generation))
    {
      if (!_registry.Remove(item.Entity))
      {
        throw new InvalidOperationException(
          "An unallocated network item could not be removed from EntityRuntime.");
      }

      return false;
    }

    _currentTick = currentTick;
    if (!TryCaptureNetworkItem(
          item.Entity,
          currentTick,
          out _,
          out _,
          out WorldItemStateSnapshot worldState,
          out WorldItemReservationSnapshot reservation,
          out RuntimeEntityHandle handle))
    {
      RollBackNetworkItem(items, allocatedSlot, generation, item.Entity);
      rejectionCode = "WorldItemInitializationFailed";
      return false;
    }

    var reservationId = new ReservationId(Guid.NewGuid());
    var candidate = CreateReservationComponent(reservation.Value);
    var reserveCommand = new WorldItemReservationCommand(
      RuntimeEntityId.FromItemReference(item.Entity),
      worldState.Value.ReplicationId,
      ownerEntity,
      reservationId,
      currentTick,
      reservationDurationTicks,
      reservation.Value.ReservationRevision);
    WorldItemReservationResult reserved = _reservationSystem.Reserve(
      reserveCommand,
      RuntimeEntityId.FromItemReference(item.Entity),
      isActive: items.TryGet(allocatedSlot, generation, out WorldEntityState? activeState) &&
        activeState is RuntimeWorldItemState activeItem &&
        activeItem.Entity == item.Entity,
      new WorldItemStateComponent(
        worldState.Value.ReplicationId,
        worldState.Value.SpawnSource),
      ToWorldItem(worldState.WorldItem.WorldItem.Value),
      candidate);
    if (!reserved.Applied ||
        !_session.EntityRuntime.TryReplace<
          WorldItemReservationComponent,
          WorldItemReservationProjection>(
          handle,
          reservation.Snapshot,
          candidate))
    {
      RollBackNetworkItem(items, allocatedSlot, generation, item.Entity);
      rejectionCode = "WorldItemReservationFailed";
      return false;
    }

    projection = CreateProjection(
      allocatedSlot.Value,
      command,
      players,
      ownerPlayerSlot,
      ToReservationProjection(candidate),
      currentTick);
    rejectionCode = string.Empty;
    return true;
  }

  private bool TryCaptureNetworkItem(
    ItemEntityRef entity,
    long currentTick,
    out PlayerInventoryItemSnapshot item,
    out RuntimeWorldItemProjection worldItem,
    out WorldItemStateSnapshot worldState,
    out WorldItemReservationSnapshot reservation,
    out RuntimeEntityHandle handle)
  {
    if (!_registry.TryGet(entity, out item) ||
        !_registry.TryGetWorldItem(entity, out worldItem) ||
        !_registry.TryGetRuntimeEntityId(entity, out RuntimeEntityId itemEntity) ||
        !_session.EntityRuntime.TryResolve(entity.Reference, out handle) ||
        !_session.EntityRuntime.TryGetStatus(handle, out EntityRuntimeStatus status) ||
        status != EntityRuntimeStatus.Running ||
        !_session.EntityRuntime.TryCaptureVersioned<
          WorldItemStateComponent,
          WorldItemStateProjection>(
          handle,
          static component => new WorldItemStateProjection(
            component.ReplicationId,
            component.SpawnSource),
          out EntityComponentSnapshot<WorldItemStateProjection> stateSnapshot) ||
        !_session.EntityRuntime.TryCaptureVersioned<
          WorldItemReservationComponent,
          WorldItemReservationProjection>(
          handle,
          static component => ToReservationProjection(component),
          out EntityComponentSnapshot<WorldItemReservationProjection> reservationSnapshot) ||
        itemEntity.Reference.Scope != EntityReferenceScope.Item ||
        itemEntity.Reference.RuntimeId != _session.EntityRuntime.RuntimeId ||
        !stateSnapshot.Value.ReplicationId.IsAssigned ||
        worldItem.WorldItem.Value.IsExpiredAt(currentTick))
    {
      item = default;
      worldItem = default;
      worldState = default;
      reservation = default;
      handle = default;
      return false;
    }

    worldState = new WorldItemStateSnapshot(stateSnapshot, worldItem);
    reservation = new WorldItemReservationSnapshot(reservationSnapshot);
    return true;
  }

  private static RuntimeWorldItemNetworkProjection CreateProjection(
    int itemSlot,
    in RuntimeWorldItemNetworkCommand command,
    RuntimePlayerStore players,
    byte ownerPlayerSlot,
    WorldItemReservationProjection reservation,
    long currentTick)
  {
    long reservationRemaining = reservation.ReservationExpiresAt.HasValue
      ? Math.Max(0, reservation.ReservationExpiresAt.Value - currentTick)
      : 0;
    long grabDelayRemaining = reservation.IgnoreOwnerUntilTick.HasValue
      ? Math.Max(0, reservation.IgnoreOwnerUntilTick.Value - currentTick)
      : 0;
    byte grabDelayPlayer = byte.MaxValue;
    if (grabDelayRemaining > 0 && reservation.IgnoreOwner is RuntimeEntityId ignoredOwner)
    {
      foreach (RuntimePlayerEntity player in players.Players)
      {
        if (players.TryGetEntityReference(player.Slot, out EntityReference playerReference) &&
            playerReference == ignoredOwner.Reference)
        {
          grabDelayPlayer = checked((byte)player.Slot);
          break;
        }
      }

      if (grabDelayPlayer == byte.MaxValue)
      {
        grabDelayRemaining = 0;
      }
    }
    else
    {
      grabDelayRemaining = 0;
    }

    return new RuntimeWorldItemNetworkProjection(
      itemSlot,
      command.TypeId,
      command.Stack,
      checked((byte)command.PrefixId),
      command.PositionX,
      command.PositionY,
      command.VelocityX,
      command.VelocityY,
      ownerPlayerSlot,
      (int)Math.Min(reservationRemaining, int.MaxValue),
      grabDelayPlayer,
      (int)Math.Min(grabDelayRemaining, int.MaxValue));
  }

  private static WorldItemComponent ToWorldItem(WorldItemComponentProjection projection) =>
    new(
      projection.SpawnedAtTick,
      projection.DespawnAtTick,
      projection.IsInstanced,
      projection.IsBeingGrabbed,
      projection.IsOnConveyor,
      projection.Revision);

  private static WorldItemReservationProjection ToReservationProjection(
    WorldItemReservationComponent component) =>
    new(
      component.ReservationId,
      component.ReservedFor,
      component.ReservationExpiresAt,
      component.IgnoreOwner,
      component.IgnoreOwnerUntilTick,
      component.NoGrabUntilTick,
      component.EnemyPickupBlockedUntilTick,
      component.ReservationRevision);

  private static WorldItemReservationComponent CreateReservationComponent(
    WorldItemReservationProjection projection) =>
    new(
      projection.ReservationId,
      projection.ReservedFor,
      projection.ReservationExpiresAt,
      projection.IgnoreOwner,
      projection.IgnoreOwnerUntilTick,
      projection.NoGrabUntilTick,
      projection.EnemyPickupBlockedUntilTick,
      projection.ReservationRevision);

  private void RollBackNetworkItem(
    EntitySlotStore<WorldEntityState, WorldItemSlot> items,
    WorldItemSlot slot,
    uint generation,
    ItemEntityRef item)
  {
    if (!items.TryRelease(slot, generation, out _))
    {
      throw new InvalidOperationException("The network world item slot could not be rolled back.");
    }

    if (!_registry.Remove(item))
    {
      throw new InvalidOperationException("The network world item entity could not be rolled back.");
    }
  }

  private readonly record struct WorldItemStateSnapshot(
    EntityComponentSnapshot<WorldItemStateProjection> Snapshot,
    RuntimeWorldItemProjection WorldItem)
  {
    public WorldItemStateProjection Value => Snapshot.Value;
  }

  private readonly record struct WorldItemStateProjection(
    ReplicationId ReplicationId,
    LootSourceRef? SpawnSource);

  private readonly record struct WorldItemReservationSnapshot(
    EntityComponentSnapshot<WorldItemReservationProjection> Snapshot)
  {
    public WorldItemReservationProjection Value => Snapshot.Value;
  }

  private readonly record struct WorldItemReservationProjection(
    ReservationId? ReservationId,
    RuntimeEntityId? ReservedFor,
    long? ReservationExpiresAt,
    RuntimeEntityId? IgnoreOwner,
    long? IgnoreOwnerUntilTick,
    long NoGrabUntilTick,
    long EnemyPickupBlockedUntilTick,
    long ReservationRevision)
  {
    public bool HasReservationAt(long currentTick) =>
      ReservedFor.HasValue &&
      ReservationExpiresAt.HasValue &&
      currentTick < ReservationExpiresAt.Value;
  }
}

internal readonly record struct RuntimeWorldItemNetworkCommand(
  int ItemSlot,
  int TypeId,
  int Stack,
  int PrefixId,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY);

internal readonly record struct RuntimeWorldItemNetworkProjection(
  int ItemSlot,
  int TypeId,
  int Stack,
  byte Prefix,
  float PositionX,
  float PositionY,
  float VelocityX,
  float VelocityY,
  byte ReservedForPlayer,
  int ReservationTicks,
  byte GrabDelayPlayer,
  int IgnoreOwnerDelayTicks);
