namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile1x1ValidationResult(
  bool IsValid,
  bool ShouldKill,
  bool HasSolidSupport);
