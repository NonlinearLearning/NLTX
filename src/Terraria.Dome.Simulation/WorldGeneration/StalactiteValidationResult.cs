namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct StalactiteValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginY,
  int Height,
  bool HangsFromTop,
  bool HasSupport);
