namespace Terraria.Player;

public sealed class PlayerStatusEffectDefinition
{
  public PlayerStatusEffectDefinition(
    ContentId<BuffDefinition> effectType,
    bool isDebuff,
    int additiveTimeCap = 0,
    bool timerDecreases = true)
  {
    if (effectType.Value <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(effectType));
    }

    if (additiveTimeCap < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(additiveTimeCap));
    }

    EffectType = effectType;
    IsDebuff = isDebuff;
    AdditiveTimeCap = additiveTimeCap;
    TimerDecreases = timerDecreases;
  }

  public ContentId<BuffDefinition> EffectType { get; }

  public bool IsDebuff { get; }

  public int AdditiveTimeCap { get; }

  public bool TimerDecreases { get; }
}
