namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TallGateValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginY,
  int StyleBand,
  bool HasVerticalAnchors);
