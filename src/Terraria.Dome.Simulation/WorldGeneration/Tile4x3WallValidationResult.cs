namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile4x3WallValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand);
