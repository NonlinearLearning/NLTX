namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile2xXValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int Height,
  int FrameBand,
  bool UsesTopSupport);
