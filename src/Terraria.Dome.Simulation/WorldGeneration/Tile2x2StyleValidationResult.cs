namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile2x2StyleValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand);
