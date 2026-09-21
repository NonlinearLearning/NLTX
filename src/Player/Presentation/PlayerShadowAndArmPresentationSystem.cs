namespace Terraria.Player.Presentation;

public sealed class PlayerShadowAndArmPresentationSystem
{
  public PlayerShadowSnapshot Update(
    PlayerShadowAndArmPresentationComponent component,
    in PlayerShadowAndArmPresentationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ApplyInput(input);
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForSpawn(
    PlayerShadowAndArmPresentationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetAdvancedShadows();
    component.ResetSocialShadow();
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForSpawn(
    PlayerShadowAndArmPresentationComponent component,
    in PlayerShadowAndArmPresentationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetForLifecycle(input);
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForTeleport(
    PlayerShadowAndArmPresentationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetAdvancedShadows();
    component.ResetSocialShadow();
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForTeleport(
    PlayerShadowAndArmPresentationComponent component,
    in PlayerShadowAndArmPresentationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetForLifecycle(input);
    return component.ToSnapshot();
  }

  public PlayerShadowSnapshot ResetForRemoval(
    PlayerShadowAndArmPresentationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ClearForRemoval();
    return component.ToSnapshot();
  }
}
