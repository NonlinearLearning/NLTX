namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile3x2ValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand,
  int FootprintHeight,
  bool HasDeferredSpecialCase);
