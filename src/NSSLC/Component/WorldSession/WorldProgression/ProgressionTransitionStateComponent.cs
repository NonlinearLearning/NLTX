namespace Terraria.WorldProgression.Components;

public sealed class ProgressionTransitionStateComponent
{
  public TransitionId? TransitionId { get; set; }
  public WorldEntityId? WorldEntityId { get; set; }
  public WorldProgressionTransitionKind TransitionKind { get; set; }
  public TransitionPhase Phase { get; set; } = TransitionPhase.Idle;
  public long RequestedAtTick { get; set; }
  public PlanId? PlanId { get; set; }
  public int ActiveTransformationCount { get; set; }
  public bool IsTransforming => ActiveTransformationCount > 0;
}
