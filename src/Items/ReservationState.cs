namespace Terraria.Items;

public enum ReservationState : byte
{
  Unknown,
  Active,
  Committed,
  Released,
  Expired,
  Cancelled,
}
