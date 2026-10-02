using System;

namespace Terraria.SimulationRuleOverrides;

public readonly record struct PlayerRuleOverrideStateComponent
{
  public PlayerRuleOverrideStateComponent()
    : this(false, true, 0.5f)
  {
  }

  public PlayerRuleOverrideStateComponent(
    bool godmodeEnabled,
    bool farPlacementRangeEnabled,
    float spawnRateNormalized)
  {
    ValidateNormalized(spawnRateNormalized, nameof(spawnRateNormalized));

    GodmodeEnabled = godmodeEnabled;
    FarPlacementRangeEnabled = farPlacementRangeEnabled;
    SpawnRateNormalized = spawnRateNormalized;
  }

  public bool GodmodeEnabled { get; }

  public bool FarPlacementRangeEnabled { get; }

  public float SpawnRateNormalized { get; }

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
}
