namespace Terraria.Items;

public sealed class WorldItemReservationComponent
{
  public WorldItemReservationComponent(
    ReservationId? reservationId = null,
    RuntimeEntityId? reservedFor = null,
    long? reservationExpiresAt = null,
    RuntimeEntityId? ignoreOwner = null,
    long? ignoreOwnerUntilTick = null,
    long noGrabUntilTick = 0,
    long enemyPickupBlockedUntilTick = 0,
    long reservationRevision = 0)
  {
    ReservationId = reservationId;
    ReservedFor = reservedFor;
    ReservationExpiresAt = reservationExpiresAt;
    IgnoreOwner = ignoreOwner;
    IgnoreOwnerUntilTick = ignoreOwnerUntilTick;
    NoGrabUntilTick = noGrabUntilTick;
    EnemyPickupBlockedUntilTick = enemyPickupBlockedUntilTick;
    ReservationRevision = reservationRevision;
  }

  public ReservationId? ReservationId;
  public RuntimeEntityId? ReservedFor;
  public long? ReservationExpiresAt;
  public RuntimeEntityId? IgnoreOwner;
  public long? IgnoreOwnerUntilTick;
  public long NoGrabUntilTick;
  public long EnemyPickupBlockedUntilTick;
  public long ReservationRevision;

  public bool HasReservationAt(long currentTick) =>
    ReservedFor.HasValue &&
    ReservationExpiresAt.HasValue &&
    currentTick < ReservationExpiresAt.Value;

  public bool CanBeGrabbedAt(long currentTick) =>
    currentTick >= NoGrabUntilTick;

  public bool IsOwnerIgnoredAt(long currentTick) =>
    IgnoreOwner.HasValue &&
    IgnoreOwnerUntilTick.HasValue &&
    currentTick < IgnoreOwnerUntilTick.Value;
}
