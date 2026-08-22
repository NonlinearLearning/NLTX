namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile4x2ValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand);
