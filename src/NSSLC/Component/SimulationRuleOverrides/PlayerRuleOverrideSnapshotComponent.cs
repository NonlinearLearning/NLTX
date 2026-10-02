using System;

namespace Terraria.SimulationRuleOverrides;

public readonly record struct PlayerRuleOverrideSnapshotComponent
{
  public PlayerRuleOverrideSnapshotComponent(
    long tick,
    bool godmodeEnabled,
    bool farPlacementRangeEnabled,
    float spawnRateMultiplier,
    bool disableSpawns,
    long? sourceRevision = null)
  {
    ValidateTick(tick, nameof(tick));
    ValidateRevision(sourceRevision, nameof(sourceRevision));
    ValidateRange(spawnRateMultiplier, 0.1f, 10.0f, nameof(spawnRateMultiplier));

    Tick = tick;
    SourceRevision = sourceRevision;
    GodmodeEnabled = godmodeEnabled;
    FarPlacementRangeEnabled = farPlacementRangeEnabled;
    SpawnRateMultiplier = spawnRateMultiplier;
    DisableSpawns = disableSpawns;
  }

  public long Tick { get; }

  public long? SourceRevision { get; }

  public bool GodmodeEnabled { get; }

  public bool FarPlacementRangeEnabled { get; }

  public float SpawnRateMultiplier { get; }

  public bool DisableSpawns { get; }

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
