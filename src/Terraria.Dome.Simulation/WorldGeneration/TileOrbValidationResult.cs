namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileOrbValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY);
