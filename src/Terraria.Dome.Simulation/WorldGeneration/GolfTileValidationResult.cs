namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct GolfTileValidationResult(
  bool IsValid,
  bool ShouldKill,
  bool HasAlignedFrame,
  bool HasSolidSupport);
