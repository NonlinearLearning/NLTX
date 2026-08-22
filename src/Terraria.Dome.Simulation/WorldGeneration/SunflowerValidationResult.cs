namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record SunflowerValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int CheckedTiles,
  int InvalidTiles,
  bool HasAllowedGround,
  bool DestructionDeferred);
