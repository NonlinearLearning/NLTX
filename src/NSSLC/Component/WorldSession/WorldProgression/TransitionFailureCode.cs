namespace Terraria.WorldProgression.Components;

public enum TransitionFailureCode : byte
{
  Unknown,
  EligibilityRejected,
  PlanningFailed,
  MutationFailed,
  CommitConflict,
  PersistenceFailed,
  ResynchronizationFailed,
}
