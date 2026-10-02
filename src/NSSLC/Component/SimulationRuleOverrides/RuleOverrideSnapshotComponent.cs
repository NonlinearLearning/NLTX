using System;

namespace Terraria.SimulationRuleOverrides;

public readonly record struct RuleOverrideSnapshotComponent
{
  public RuleOverrideSnapshotComponent(
    long tick,
    int targetTimeRate,
    float difficultyMultiplier,
    float windSpeedTarget,
    float rainStrength,
    bool freezeTimeEnabled,
    bool freezeWindEnabled,
    bool freezeRainEnabled,
    bool allowInfectionSpread,
    long? sourceRevision = null)
  {
    ValidateTick(tick, nameof(tick));
    ValidateRevision(sourceRevision, nameof(sourceRevision));
    ValidateTimeRate(targetTimeRate, nameof(targetTimeRate));
    ValidateDifficultyMultiplier(difficultyMultiplier, nameof(difficultyMultiplier));
    ValidateRange(windSpeedTarget, -0.8f, 0.8f, nameof(windSpeedTarget));
    ValidateRange(rainStrength, 0.0f, 1.0f, nameof(rainStrength));

    Tick = tick;
    SourceRevision = sourceRevision;
    TargetTimeRate = targetTimeRate;
    DifficultyMultiplier = difficultyMultiplier;
    WindSpeedTarget = windSpeedTarget;
    RainStrength = rainStrength;
    FreezeTimeEnabled = freezeTimeEnabled;
    FreezeWindEnabled = freezeWindEnabled;
    FreezeRainEnabled = freezeRainEnabled;
    AllowInfectionSpread = allowInfectionSpread;
  }

  public long Tick { get; }

  public long? SourceRevision { get; }

  public int TargetTimeRate { get; }

  public float DifficultyMultiplier { get; }

  public float WindSpeedTarget { get; }

  public float RainStrength { get; }

  public bool FreezeTimeEnabled { get; }

  public bool FreezeWindEnabled { get; }

  public bool FreezeRainEnabled { get; }

  public bool AllowInfectionSpread { get; }

  private static void ValidateTick(long value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The tick must be non-negative.");
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

  private static void ValidateTimeRate(int value, string parameterName)
  {
    if (value < 1 || value > 24)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The target time rate must be within [1, 24].");
    }
  }

  private static void ValidateDifficultyMultiplier(float value, string parameterName)
  {
    ValidateRange(value, 0.5f, 3.0f, parameterName);

    var stepPosition = (value - 0.5f) / 0.05f;
    var nearestStep = MathF.Round(stepPosition);
    if (MathF.Abs(stepPosition - nearestStep) > 0.0001f)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The difficulty multiplier must use 0.05 increments.");
    }
  }

  private static void ValidateRange(
    float value,
    float minimum,
    float maximum,
    string parameterName)
  {
    if (float.IsNaN(value)
      || float.IsInfinity(value)
      || value < minimum
      || value > maximum)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The value must be finite and within its candidate range.");
    }
  }
}
