namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile2x1ValidationResult(
  bool IsValid,
  bool ShouldKill,
  bool RequiresPileValidation,
  int OriginX,
  int OriginY,
  int StyleBand);
