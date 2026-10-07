namespace Terraria.Player.Presentation;

public sealed class PlayerShadowPresentationSystem
{
  public PlayerShadowSnapshot Update(
    PlayerShadowPresentationComponent component,
    in PlayerShadowPresentationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ApplyInput(input);
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForSpawn(PlayerShadowPresentationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetAdvancedShadows();
    component.ResetSocialShadow();
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForSpawn(
    PlayerShadowPresentationComponent component,
    in PlayerShadowPresentationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetForLifecycle(input);
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForTeleport(PlayerShadowPresentationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetAdvancedShadows();
    component.ResetSocialShadow();
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForTeleport(
    PlayerShadowPresentationComponent component,
    in PlayerShadowPresentationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetForLifecycle(input);
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForRemoval(PlayerShadowPresentationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ClearForRemoval();
    return component.ToSnapshot();
  }
}
