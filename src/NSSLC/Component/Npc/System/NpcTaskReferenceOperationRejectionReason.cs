namespace Terraria.Npc;

public enum NpcTaskReferenceOperationRejectionReason : byte
{
  None,
  InvalidReference,
  StaleEntityReference,
  StaleTaskReference,
  NotRunning,
  TaskKindMismatch,
  InvalidTaskKind,
  InvalidFailureReason,
}
