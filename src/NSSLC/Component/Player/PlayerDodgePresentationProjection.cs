namespace Terraria.Player;

public sealed class PlayerDodgePresentationProjection
{
  public PlayerDodgePresentationSnapshot Snapshot(
    PlayerDodgeAndImmunityStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    return new PlayerDodgePresentationSnapshot(
      component.BrainOfConfusionDodgeAnimationCounter,
      component.ShadowDodge);
  }
}
