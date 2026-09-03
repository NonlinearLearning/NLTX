namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct FoodPlatterValidationResult(
  bool IsValid,
  bool ShouldKill,
  bool HasBottomSupport);
