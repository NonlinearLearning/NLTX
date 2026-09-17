namespace Terraria.Player;

public sealed class PlayerCombatProcStateComponent
{
  public float LifeSteal { get; set; } = 99999f;

  public float GhostDmg { get; set; }

  public int EocDash { get; set; }

  public int EocHit { get; set; } = -1;

  public int InfernoCounter { get; set; }

  public int StarCloakCooldown { get; set; }

  public bool OnHitDodge { get; set; }

  public bool OnHitRegen { get; set; }

  public bool OnHitPetal { get; set; }

  public bool OnHitTitaniumStorm { get; set; }

  public int TitaniumStormCooldown { get; set; }

  public bool HasTitaniumStormBuff { get; set; }

  public int PetalTimer { get; set; }

  public int BoneGloveTimer { get; set; }

  public int PhantomPhoneixCounter { get; set; }
}
