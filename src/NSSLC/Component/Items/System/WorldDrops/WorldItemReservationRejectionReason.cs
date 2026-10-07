namespace Terraria.Items;

public enum WorldItemReservationRejectionReason : byte
{
  None,
  StaleWorldItem,
  InactiveWorldItem,
  ExpiredWorldItem,
  InvalidPlayer,
  InvalidReservation,
  InvalidTick,
  InvalidDuration,
  ReservationRevisionConflict,
  ReservedByAnotherPlayer,
  NotReservationOwner,
  ReservationIdentityChanged,
  ReservationRevisionExhausted
}
