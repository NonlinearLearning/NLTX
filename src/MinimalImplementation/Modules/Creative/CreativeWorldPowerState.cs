using System;

namespace MinimalImplementation.Modules.Creative;

/// <summary>Persistent per-world values owned by the Creative Power state.</summary>
public sealed record CreativeWorldPowerState(
  bool FreezeTime,
  float ModifyTimeRate,
  bool FreezeRain,
  bool FreezeWind,
  float DifficultySlider,
  bool StopBiomeSpread)
{
  public void Validate()
  {
    if (!float.IsFinite(ModifyTimeRate) || ModifyTimeRate is < 0f or > 1f
        || !float.IsFinite(DifficultySlider) || DifficultySlider is < 0f or > 1f)
    {
      throw new ArgumentOutOfRangeException(
        nameof(ModifyTimeRate),
        "Creative world slider values must be finite.");
    }
  }
}
