namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-727..P09-738, P09-10272..P09-10370
// crossSubsystemOwner: item definition evaluation and downstream Combat/renderer consumers remain integration-review
public sealed class PlayerEquipmentEffectSystem
{
  public PlayerEquipmentEffectSnapshot Rebuild(
    PlayerEquipmentEffectStateComponent component,
    in PlayerEquipmentEffectRebuildInput input)
  {
    ArgumentNullException.ThrowIfNull(component);

    component.ArmorEffectDrawShadow = input.ArmorEffectDrawShadow;
    component.ArmorEffectDrawShadowSubtle = input.ArmorEffectDrawShadowSubtle;
    component.ArmorEffectDrawOutlines = input.ArmorEffectDrawOutlines;
    component.ArmorEffectDrawShadowLokis = input.ArmorEffectDrawShadowLokis;
    component.ArmorEffectDrawShadowBasilisk = input.ArmorEffectDrawShadowBasilisk;
    component.ArmorEffectDrawOutlinesForbidden = input.ArmorEffectDrawOutlinesForbidden;
    component.ArmorEffectDrawShadowEocShield = input.ArmorEffectDrawShadowEocShield;
    component.SocialShadowRocketBoots = input.SocialShadowRocketBoots;
    component.SocialGhost = input.SocialGhost;
    component.AshWoodBonus = input.AshWoodBonus;
    component.SocialIgnoreLight = input.SocialIgnoreLight;
    return Snapshot(component);
  }

  public PlayerEquipmentEffectSnapshot ResetForTick(
    PlayerEquipmentEffectStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    component.ArmorEffectDrawShadow = false;
    component.ArmorEffectDrawShadowSubtle = false;
    component.ArmorEffectDrawOutlines = false;
    component.ArmorEffectDrawShadowLokis = false;
    component.ArmorEffectDrawShadowBasilisk = false;
    component.ArmorEffectDrawOutlinesForbidden = false;
    component.ArmorEffectDrawShadowEocShield = false;
    component.SocialShadowRocketBoots = false;
    component.SocialGhost = false;
    component.AshWoodBonus = false;
    component.SocialIgnoreLight = false;
    return Snapshot(component);
  }

  public PlayerEquipmentEffectSnapshot ResetForLifecycle(
    PlayerEquipmentEffectStateComponent component)
  {
    return ResetForTick(component);
  }

  public static PlayerEquipmentEffectSnapshot Snapshot(
    PlayerEquipmentEffectStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    return new PlayerEquipmentEffectSnapshot(
      component.ArmorEffectDrawShadow,
      component.ArmorEffectDrawShadowSubtle,
      component.ArmorEffectDrawOutlines,
      component.ArmorEffectDrawShadowLokis,
      component.ArmorEffectDrawShadowBasilisk,
      component.ArmorEffectDrawOutlinesForbidden,
      component.ArmorEffectDrawShadowEocShield,
      component.SocialShadowRocketBoots,
      component.SocialGhost,
      component.AshWoodBonus,
      component.SocialIgnoreLight);
  }
}
