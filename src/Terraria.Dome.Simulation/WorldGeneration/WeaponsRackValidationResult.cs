namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WeaponsRackValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  bool HasWallBackings);
