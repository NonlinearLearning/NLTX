namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct JunglePlantValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int Width,
  int Height,
  bool UsesBottomSlopeSupport);
