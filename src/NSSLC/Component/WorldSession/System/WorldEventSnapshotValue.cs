namespace Terraria.WorldSession.Components;

public readonly record struct WorldEventSnapshotValue(
  bool BloodMoon,
  bool Eclipse,
  bool PumpkinMoon,
  bool SnowMoon,
  bool SlimeRain,
  int SlimeRainKillCount,
  int InvasionType,
  int InvasionDelay,
  int InvasionSize,
  int InvasionWarningTimer,
  int InvasionProgress,
  bool Dd2Ongoing,
  int Dd2SpawnDelay,
  bool LunarApocalypseActive,
  int MoonLordCountdown);
