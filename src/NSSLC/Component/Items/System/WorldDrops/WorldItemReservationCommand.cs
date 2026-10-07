namespace Terraria.Items;

/// <summary>A trusted reservation assignment resolved to current runtime identities.</summary>
public readonly record struct WorldItemReservationCommand(
  RuntimeEntityId ItemEntity,
  ReplicationId ReplicationId,
  RuntimeEntityId PlayerEntity,
  ReservationId ReservationId,
  long CurrentTick,
  long DurationTicks,
  long ExpectedReservationRevision);
