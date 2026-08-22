namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile3x2WallValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand);
