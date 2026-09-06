namespace Terraria.Player;

public sealed class PlayerRegenerationAndImmunityComponent
{
  public int LifeRegenRate { get; set; }

  public int LifeRegenAccumulator { get; set; }

  public float LifeRegenElapsed { get; set; }

  public int ManaRegenRate { get; set; }

  public int ManaRegenAccumulator { get; set; }

  public float ManaRegenDelay { get; set; }

  public int ManaRegenRateBonus { get; set; }

  public float ManaRegenDelayBonus { get; set; }

  public bool HasManaRegenBuff { get; set; }

  public int GeneralImmunityRemainingTicks { get; set; }

  // The Version4 immunity slot count remains unresolved.
  public int[] CooldownImmunityRemainingTicks { get; } = [];

  public bool SuppressImmunityBlink { get; set; }
}
