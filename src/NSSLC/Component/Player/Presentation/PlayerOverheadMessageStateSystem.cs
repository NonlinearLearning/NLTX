namespace Terraria.Player.Presentation;

public sealed class PlayerOverheadMessageStateSystem
{
  public void ReplaceMessage(
    PlayerOverheadMessageStateComponent component,
    in PlayerOverheadMessageInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Replace(input);
  }

  public bool AdvancePresentationTick(PlayerOverheadMessageStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.AdvancePresentationTick();
  }

  public void ResetForSpawn(PlayerOverheadMessageStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
  }

  public void ResetForRemoval(PlayerOverheadMessageStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
  }
}
