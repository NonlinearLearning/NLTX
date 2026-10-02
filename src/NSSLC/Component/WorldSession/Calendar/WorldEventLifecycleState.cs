namespace Terraria.WorldSession.Calendar;

// Provisional boundary type; transition ownership remains integration-review.
public enum WorldEventLifecycleState : byte
{
  Uninitialized,
  Active,
  Ended,
  Cancelled
}
