namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile3x6ValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand);
