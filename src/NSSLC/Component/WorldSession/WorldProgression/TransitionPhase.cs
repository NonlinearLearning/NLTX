namespace Terraria.WorldProgression.Components;

public enum TransitionPhase : byte
{
  Idle,
  Requested,
  Planned,
  Executing,
  ReadyToCommit,
  Committed,
  Failed,
  Recovering,
}
