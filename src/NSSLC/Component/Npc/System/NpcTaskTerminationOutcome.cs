namespace Terraria.Npc;

public enum NpcTaskTerminationOutcome : byte
{
  Rejected,
  Ended,
  AlreadyEnded,
  EarlierEndReasonPreserved,
}
