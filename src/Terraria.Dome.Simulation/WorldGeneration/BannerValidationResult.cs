namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct BannerValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginY,
  int StyleBand,
  bool HasHangingSupport);
