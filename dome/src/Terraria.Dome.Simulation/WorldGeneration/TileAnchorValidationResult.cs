namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileAnchorValidationResult(
  bool IsValid,
  bool ShouldKill,
  int CheckedSides,
  int CheckedTiles,
  int FailedTiles);
