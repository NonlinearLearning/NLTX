namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile4x4ValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand,
  int FrameBand);
