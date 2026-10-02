namespace Terraria.Player;

public sealed class PlayerCombatProcQuery
{
  public PlayerCombatProcSnapshot Snapshot(PlayerCombatProcStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    return new PlayerCombatProcSnapshot(
      component.LifeSteal,
      component.GhostDmg,
      component.EocDash,
      component.EocHit,
      component.InfernoCounter,
      component.StarCloakCooldown,
      component.OnHitDodge,
      component.OnHitRegen,
      component.OnHitPetal,
      component.OnHitTitaniumStorm,
      component.TitaniumStormCooldown,
      component.HasTitaniumStormBuff,
      component.PetalTimer,
      component.BoneGloveTimer,
      component.PhantomPhoneixCounter);
  }
}
