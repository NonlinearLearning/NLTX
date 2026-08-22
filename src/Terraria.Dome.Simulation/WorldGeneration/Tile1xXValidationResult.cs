namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile1xXValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginY,
  int Height,
  int FrameBand);
