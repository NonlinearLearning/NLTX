namespace Terraria.Player;

public sealed class PlayerDodgeAndImmunityStateComponent
{
  public bool BlackBelt { get; internal set; }

  public ItemEntityRef BrainOfConfusionItem { get; internal set; } =
    ItemEntityRef.None;

  public int BrainOfConfusionDodgeAnimationCounter { get; internal set; }

  public bool ShadowDodge { get; internal set; }

  public int ShadowDodgeTimer { get; internal set; }
}
