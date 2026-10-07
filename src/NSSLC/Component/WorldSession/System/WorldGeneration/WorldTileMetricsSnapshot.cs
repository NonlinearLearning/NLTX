namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldTileMetricsSnapshot(
  int GoodCount,
  int EvilCount,
  int BloodCount,
  int SolidCount,
  byte GoodPercent,
  byte EvilPercent,
  byte BloodPercent);
