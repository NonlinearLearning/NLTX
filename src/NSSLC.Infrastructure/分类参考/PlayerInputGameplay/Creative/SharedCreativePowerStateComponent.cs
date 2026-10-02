namespace NLTX.PlayerInputGameplay.Creative;

public sealed class SharedCreativePowerStateComponent
{
  public float SliderCurrentValueCache { get; private set; }

  public float CurrentTargetValue { get; private set; }

  public long NextTimeWeCanPushTick { get; private set; }

  public bool Enabled { get; private set; }

  public int TargetTimeRate { get; private set; }

  public float StrengthMultiplierToGiveNpcs { get; private set; } = 1f;

  public void SetEnabled(bool enabled)
  {
    Enabled = enabled;
  }

  public void SetSlider(float currentValue, float targetValue, long nextPushTick)
  {
    ValidateFinite(currentValue, nameof(currentValue));
    ValidateFinite(targetValue, nameof(targetValue));
    SliderCurrentValueCache = currentValue;
    CurrentTargetValue = targetValue;
    NextTimeWeCanPushTick = nextPushTick;
  }

  public void SetTargetTimeRate(int targetTimeRate)
  {
    if (targetTimeRate <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(targetTimeRate));
    }

    TargetTimeRate = targetTimeRate;
  }

  public void SetStrengthMultiplier(float strengthMultiplier)
  {
    ValidateFinite(strengthMultiplier, nameof(strengthMultiplier));
    if (strengthMultiplier <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(strengthMultiplier));
    }

    StrengthMultiplierToGiveNpcs = strengthMultiplier;
  }

  private static void ValidateFinite(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
