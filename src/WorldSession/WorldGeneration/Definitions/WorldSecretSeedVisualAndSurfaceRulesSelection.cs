namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedVisualAndSurfaceRulesSelection(
  bool PaintEverythingGray,
  bool PaintEverythingNegative,
  bool CoatEverythingEcho,
  bool CoatEverythingIlluminant,
  bool NoSurface,
  bool SurfaceIsInSpace,
  bool RainsForAYear,
  bool RainbowStuff,
  bool WorldIsFrozen);
