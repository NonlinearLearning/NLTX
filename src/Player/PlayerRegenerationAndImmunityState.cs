namespace Terraria.Player;

public sealed class PlayerRegenerationAndImmunityState
{
  public int LifeRegenRate { get; set; }

  public int LifeRegenAccumulator { get; set; }

  public float LifeRegenElapsed { get; set; }

  public int ManaRegenRate { get; set; }

  public int ManaRegenAccumulator { get; set; }

  public float ManaRegenDelay { get; set; }

  public float MaximumRegenDelay { get; set; }

  public int ManaRegenRateBonus { get; internal set; }

  public float ManaRegenDelayBonus { get; internal set; }

  public bool HasManaRegenBuff { get; internal set; }

  public int GeneralImmunityRemainingTicks { get; set; }

  public int[] CooldownImmunityRemainingTicks { get; } = Array.Empty<int>();

  public bool SuppressImmunityBlink { get; set; }

  public bool HasGeneralImmunity => GeneralImmunityRemainingTicks > 0;

  public bool HasAnyImmunity => HasGeneralImmunity || CooldownImmunityRemainingTicks.Any(ticks => ticks > 0);
}
