namespace Terraria.Items;

/// <summary>A trusted request to release the caller's current world-item reservation.</summary>
public readonly record struct WorldItemReservationReleaseCommand(
  RuntimeEntityId ItemEntity,
  ReplicationId ReplicationId,
  RuntimeEntityId PlayerEntity,
  ReservationId ReservationId,
  long CurrentTick,
  long ExpectedReservationRevision);
