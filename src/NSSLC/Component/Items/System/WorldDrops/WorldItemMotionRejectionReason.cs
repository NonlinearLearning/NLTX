namespace Terraria.Items;

public enum WorldItemMotionRejectionReason : byte
{
  None,
  StaleWorldItem,
  InvalidTick,
  ExpiredWorldItem,
  InvalidMotion,
  RevisionConflict,
  RevisionExhausted,
  InvalidSynchronizedState
}
