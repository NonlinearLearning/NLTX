namespace Terraria.Npc;

public readonly record struct NpcTaskReferenceOperationResult(
  bool Accepted,
  bool Changed,
  NpcTaskReferenceOperationRejectionReason RejectionReason,
  NpcTaskLifecycleResult PreviousTask,
  NpcTaskLifecycleResult CurrentTask)
{
  public bool Rejected => !Accepted;
}
