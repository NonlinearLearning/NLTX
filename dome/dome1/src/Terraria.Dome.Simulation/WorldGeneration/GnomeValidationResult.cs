namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record GnomeValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginY,
  bool HasExpectedFootprint,
  bool HasSupportedGround,
  bool DestructionDeferred);
