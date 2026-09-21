namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct Tile1x2TopValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginY,
  int StyleBand,
  bool UsesPlatformSupport,
  bool UsesRopeSupport);
