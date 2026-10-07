namespace Terraria.Npc;

public readonly record struct NpcTaskTerminationResult(
  NpcTaskTerminationOutcome Outcome,
  NpcTaskTerminationRejectionReason RejectionReason,
  NpcTaskEndReason RequestedEndReason,
  NpcTaskEndReason EndReason,
  NpcTaskLifecycleResult PreviousTask,
  NpcTaskLifecycleResult CurrentTask)
{
  public bool Accepted => Outcome != NpcTaskTerminationOutcome.Rejected;

  public bool Changed => Outcome == NpcTaskTerminationOutcome.Ended;
}
