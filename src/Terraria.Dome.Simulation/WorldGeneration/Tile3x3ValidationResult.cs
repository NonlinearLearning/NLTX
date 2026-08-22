namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile3x3ValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand,
  bool UsesBottomSupport);
