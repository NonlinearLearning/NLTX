namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct BambooValidationResult(
  bool IsValid,
  bool ShouldKill,
  bool HasSupport,
  bool HasBambooAbove,
  int ExpectedFrameBand);
