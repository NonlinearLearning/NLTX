namespace Terraria.WorldGeneration.Components;

public readonly record struct UndergroundDesertStructureSnapshot(
  long GenerationId,
  UndergroundDesertRectangle UndergroundDesertLocation,
  UndergroundDesertRectangle UndergroundDesertHiveLocation,
  int DesertHiveHigh,
  int DesertHiveLow,
  int DesertHiveLeft,
  int DesertHiveRight);
