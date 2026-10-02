namespace Terraria.WorldProgression.Components;

public sealed class WorldHardmodeRuleStateComponent
{
  public bool IsHardMode { get; set; }
  public HardmodeRulePhase RulePhase { get; set; } = HardmodeRulePhase.PreHardmode;
  public TransitionId? LastCommittedTransitionId { get; set; }
  public long LastCommittedSequence { get; set; }
}
