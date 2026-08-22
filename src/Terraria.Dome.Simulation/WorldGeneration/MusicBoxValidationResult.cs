namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct MusicBoxValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand,
  bool HasSupport);
