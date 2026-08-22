namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TilePileValidationResult(
  bool IsSupported,
  bool ShouldKill,
  bool RequiresTwoByOneCheck);
