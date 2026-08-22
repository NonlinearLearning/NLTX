namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct OasisPlantValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  int StyleBand);
