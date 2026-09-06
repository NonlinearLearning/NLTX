namespace Terraria.WorldSession.Components;

public readonly record struct WorldWeatherSnapshotValue(
  bool IsRaining,
  int RainTime,
  float MaximumRainStrength,
  float WindSpeedTarget,
  float WindSpeedCurrent);
