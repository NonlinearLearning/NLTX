namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct XmasTreeValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  bool HasGroundSupport,
  int ActiveTiles);
