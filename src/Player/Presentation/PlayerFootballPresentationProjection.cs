namespace Terraria.Player.Presentation;

public sealed class PlayerFootballPresentationProjection
{
  public PlayerFootballPresentationSnapshot Project(
    PlayerFootballPresentationStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.ToSnapshot();
  }
}
