namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct BoulderChestValidationResult(
  bool IsBoulder,
  bool ShouldReturnEarly,
  int OriginX,
  int OriginY,
  int AboveY,
  bool HasProtectedContainer);
