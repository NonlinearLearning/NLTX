namespace Terraria.Items;

/// <summary>
/// Applies reservation transitions to world-item components supplied by their ECS owner.
/// The owner resolves the item through the shared EntityRuntime, verifies active world
/// presence, and calls this system on the runtime's owner thread.
/// </summary>
public sealed class WorldItemReservationSystem
{
  public WorldItemReservationResult Reserve(
    in WorldItemReservationCommand command,
    RuntimeEntityId itemEntity,
    bool isActive,
    WorldItemStateComponent itemState,
    WorldItemComponent worldItem,
    WorldItemReservationComponent reservation)
  {
    ArgumentNullException.ThrowIfNull(itemState);
    ArgumentNullException.ThrowIfNull(worldItem);
    ArgumentNullException.ThrowIfNull(reservation);

    if (!MatchesItem(command.ItemEntity, itemEntity, command.ReplicationId, itemState))
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.StaleWorldItem);
    }

    if (!isActive)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.InactiveWorldItem);
    }

    if (command.CurrentTick < 0 ||
      command.CurrentTick < worldItem.SpawnedAtTick)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.InvalidTick);
    }

    if (worldItem.IsExpiredAt(command.CurrentTick))
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.ExpiredWorldItem);
    }

    if (!IsPlayerEntity(command.PlayerEntity, itemEntity.Reference.RuntimeId))
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.InvalidPlayer);
    }

    if (!command.ReservationId.IsAssigned)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.InvalidReservation);
    }

    if (command.DurationTicks <= 0 ||
      command.CurrentTick > long.MaxValue - command.DurationTicks)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.InvalidDuration);
    }

    if (reservation.ReservationRevision < 0)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.ReservationRevisionConflict);
    }

    if (reservation.HasReservationAt(command.CurrentTick))
    {
      if (reservation.ReservedFor == command.PlayerEntity &&
        reservation.ReservationId == command.ReservationId)
      {
        return new WorldItemReservationResult(
          Applied: true,
          IsIdempotent: true,
          ReservationRevision: reservation.ReservationRevision,
          RejectionReason: WorldItemReservationRejectionReason.None);
      }

      return Reject(reservation,
        WorldItemReservationRejectionReason.ReservedByAnotherPlayer);
    }

    if (command.ExpectedReservationRevision !=
      reservation.ReservationRevision)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.ReservationRevisionConflict);
    }

    if (reservation.ReservationRevision == long.MaxValue)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.ReservationRevisionExhausted);
    }

    reservation.ReservationId = command.ReservationId;
    reservation.ReservedFor = command.PlayerEntity;
    reservation.ReservationExpiresAt =
      command.CurrentTick + command.DurationTicks;
    reservation.ReservationRevision++;
    return new WorldItemReservationResult(
      Applied: true,
      IsIdempotent: false,
      ReservationRevision: reservation.ReservationRevision,
      RejectionReason: WorldItemReservationRejectionReason.None);
  }

  public WorldItemReservationResult Release(
    in WorldItemReservationReleaseCommand command,
    RuntimeEntityId itemEntity,
    bool isActive,
    WorldItemStateComponent itemState,
    WorldItemComponent worldItem,
    WorldItemReservationComponent reservation)
  {
    ArgumentNullException.ThrowIfNull(itemState);
    ArgumentNullException.ThrowIfNull(worldItem);
    ArgumentNullException.ThrowIfNull(reservation);

    if (!MatchesItem(command.ItemEntity, itemEntity, command.ReplicationId, itemState))
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.StaleWorldItem);
    }

    if (!isActive)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.InactiveWorldItem);
    }

    if (command.CurrentTick < 0 ||
      command.CurrentTick < worldItem.SpawnedAtTick)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.InvalidTick);
    }

    if (worldItem.IsExpiredAt(command.CurrentTick))
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.ExpiredWorldItem);
    }

    if (!IsPlayerEntity(command.PlayerEntity, itemEntity.Reference.RuntimeId))
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.InvalidPlayer);
    }

    if (!command.ReservationId.IsAssigned)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.InvalidReservation);
    }

    if (reservation.ReservationRevision < 0)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.ReservationRevisionConflict);
    }

    if (reservation.ReservedFor != command.PlayerEntity)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.NotReservationOwner);
    }

    if (reservation.ReservationId != command.ReservationId)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.ReservationIdentityChanged);
    }

    if (command.ExpectedReservationRevision !=
      reservation.ReservationRevision)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.ReservationRevisionConflict);
    }

    if (reservation.ReservationRevision == long.MaxValue)
    {
      return Reject(reservation,
        WorldItemReservationRejectionReason.ReservationRevisionExhausted);
    }

    reservation.ReservationId = null;
    reservation.ReservedFor = null;
    reservation.ReservationExpiresAt = null;
    reservation.ReservationRevision++;
    return new WorldItemReservationResult(
      Applied: true,
      IsIdempotent: false,
      ReservationRevision: reservation.ReservationRevision,
      RejectionReason: WorldItemReservationRejectionReason.None);
  }

  private static bool MatchesItem(
    RuntimeEntityId expectedItemEntity,
    RuntimeEntityId itemEntity,
    ReplicationId replicationId,
    WorldItemStateComponent itemState)
  {
    return IsItemEntity(expectedItemEntity) &&
      IsItemEntity(itemEntity) &&
      itemEntity == expectedItemEntity &&
      replicationId.IsAssigned &&
      itemState.ReplicationId == replicationId;
  }

  private static bool IsItemEntity(RuntimeEntityId entity) =>
    !entity.IsEmpty &&
    entity.Reference.Scope == Terraria.Relationships.EntityReferenceScope.Item;

  private static bool IsPlayerEntity(
    RuntimeEntityId entity,
    Terraria.Relationships.EntityRuntimeId expectedRuntimeId)
  {
    return !entity.IsEmpty &&
      entity.Reference.Scope == Terraria.Relationships.EntityReferenceScope.Player &&
      entity.Reference.RuntimeId == expectedRuntimeId;
  }

  private static WorldItemReservationResult Reject(
    WorldItemReservationComponent reservation,
    WorldItemReservationRejectionReason reason)
  {
    return WorldItemReservationResult.Rejected(
      reservation.ReservationRevision,
      reason);
  }
}
