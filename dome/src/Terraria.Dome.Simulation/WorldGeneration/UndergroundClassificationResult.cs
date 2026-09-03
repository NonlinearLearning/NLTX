namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record UndergroundClassificationResult(
  bool IsUnderground,
  int SolidTileCount,
  int ScannedTileCount,
  int WindowStartX,
  int WindowStartY,
  bool UsedDeepShortcut,
  bool UsedShallowShortcut,
  bool UsedWallFallback);
