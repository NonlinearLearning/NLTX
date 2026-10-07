namespace Terraria.Player.Presentation;

public sealed class PlayerFootballPresentationStateSystem
{
  public PlayerFootballPresentationSnapshot Update(
    PlayerFootballPresentationStateComponent component,
    in PlayerFootballPresentationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ApplyInput(input);
    return component.ToSnapshot();
  }

  public PlayerFootballPresentationSnapshot ResetForSpawn(
    PlayerFootballPresentationStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
    return component.ToSnapshot();
  }

  public PlayerFootballPresentationSnapshot ResetForItemLoss(
    PlayerFootballPresentationStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
    return component.ToSnapshot();
  }
}
