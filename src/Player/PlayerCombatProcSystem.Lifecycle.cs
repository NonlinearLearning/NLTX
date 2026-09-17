namespace Terraria.Player;

public sealed partial class PlayerCombatProcSystem
{
  public void ResetEffects()
  {
    _component.OnHitDodge = false;
    _component.OnHitRegen = false;
    _component.OnHitPetal = false;
    _component.OnHitTitaniumStorm = false;
    _component.HasTitaniumStormBuff = false;
  }

  public void Reset()
  {
    _component.LifeSteal = 99999f;
    _component.GhostDmg = 0f;
    _component.EocDash = 0;
    _component.EocHit = -1;
    _component.InfernoCounter = 0;
    _component.StarCloakCooldown = 0;
    _component.OnHitDodge = false;
    _component.OnHitRegen = false;
    _component.OnHitPetal = false;
    _component.OnHitTitaniumStorm = false;
    _component.TitaniumStormCooldown = 0;
    _component.HasTitaniumStormBuff = false;
    _component.PetalTimer = 0;
    _component.BoneGloveTimer = 0;
    _component.PhantomPhoneixCounter = 0;
    _acceptedEventIds.Clear();
  }
}
