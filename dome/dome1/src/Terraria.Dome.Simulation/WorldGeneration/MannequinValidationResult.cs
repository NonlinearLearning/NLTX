namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct MannequinValidationResult(
  bool IsValid,
  bool ShouldKill,
  int OriginX,
  int OriginY,
  ushort TileType);
