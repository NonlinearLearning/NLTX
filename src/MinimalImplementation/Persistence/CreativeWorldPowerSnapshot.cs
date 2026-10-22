using System;

namespace MinimalImplementation.Persistence;

/// <summary>Typed Version4 per-world state for the six persistent Creative powers.</summary>
public sealed record CreativeWorldPowerSnapshot(
  bool FreezeTime,
  float ModifyTimeRate,
  bool FreezeRain,
  bool FreezeWind,
  float DifficultySlider,
  bool StopBiomeSpread)
{
  public static CreativeWorldPowerSnapshot Defaults => new(
    FreezeTime: false,
    ModifyTimeRate: 0,
    FreezeRain: false,
    FreezeWind: false,
    DifficultySlider: 0,
    StopBiomeSpread: false);

  public void Validate()
  {
    if (!float.IsFinite(ModifyTimeRate) || !float.IsFinite(DifficultySlider))
    {
      throw new ArgumentOutOfRangeException(nameof(ModifyTimeRate),
        "Creative world slider values must be finite.");
    }
  }
}
