namespace Terraria.Npc;

public enum NpcTaskTerminationRejectionReason : byte
{
  None,
  InvalidReference,
  StaleEntityReference,
  StaleTaskReference,
  InvalidEndReason,
}
