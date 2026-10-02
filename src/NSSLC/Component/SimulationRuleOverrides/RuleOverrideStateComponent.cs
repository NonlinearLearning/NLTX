using System;

namespace Terraria.SimulationRuleOverrides;

public readonly record struct RuleOverrideStateComponent
{
  public RuleOverrideStateComponent(
    float timeRateNormalized = 0.0f,
    float difficultyNormalized = 0.0f,
    float windDirectionAndStrengthNormalized = 0.0f,
    float rainStrengthNormalized = 0.0f,
    bool freezeTimeEnabled = false,
    bool freezeWindEnabled = false,
    bool freezeRainEnabled = false,
    bool stopBiomeSpreadEnabled = false,
    long? authorityRevision = null)
  {
    ValidateNormalized(timeRateNormalized, nameof(timeRateNormalized));
    ValidateNormalized(difficultyNormalized, nameof(difficultyNormalized));
    ValidateNormalized(
      windDirectionAndStrengthNormalized,
      nameof(windDirectionAndStrengthNormalized));
    ValidateNormalized(rainStrengthNormalized, nameof(rainStrengthNormalized));
    ValidateRevision(authorityRevision, nameof(authorityRevision));

    TimeRateNormalized = timeRateNormalized;
    DifficultyNormalized = difficultyNormalized;
    WindDirectionAndStrengthNormalized = windDirectionAndStrengthNormalized;
    RainStrengthNormalized = rainStrengthNormalized;
    FreezeTimeEnabled = freezeTimeEnabled;
    FreezeWindEnabled = freezeWindEnabled;
    FreezeRainEnabled = freezeRainEnabled;
    StopBiomeSpreadEnabled = stopBiomeSpreadEnabled;
    AuthorityRevision = authorityRevision;
  }

  public float TimeRateNormalized { get; }

  public float DifficultyNormalized { get; }

  public float WindDirectionAndStrengthNormalized { get; }

  public float RainStrengthNormalized { get; }

  public bool FreezeTimeEnabled { get; }

  public bool FreezeWindEnabled { get; }

  public bool FreezeRainEnabled { get; }

  public bool StopBiomeSpreadEnabled { get; }

  public long? AuthorityRevision { get; }

  private static void ValidateNormalized(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value) || value < 0.0f || value > 1.0f)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The normalized value must be finite and within [0, 1].");
    }
  }

  private static void ValidateRevision(long? value, string parameterName)
  {
    if (value is < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The revision must be non-negative when present.");
    }
  }
}
