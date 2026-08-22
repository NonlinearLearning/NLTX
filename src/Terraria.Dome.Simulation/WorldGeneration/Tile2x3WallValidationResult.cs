namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile2x3WallValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand);
