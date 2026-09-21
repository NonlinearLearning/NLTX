namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct CannonValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand,
  bool HasInternalSupport);
