namespace Terraria.Items;

public readonly record struct WorldItemReservationResult(
  bool Applied,
  bool IsIdempotent,
  long ReservationRevision,
  WorldItemReservationRejectionReason RejectionReason)
{
  public static WorldItemReservationResult Rejected(
    long currentRevision,
    WorldItemReservationRejectionReason reason)
  {
    return new WorldItemReservationResult(
      Applied: false,
      IsIdempotent: false,
      ReservationRevision: currentRevision,
      RejectionReason: reason);
  }
}
