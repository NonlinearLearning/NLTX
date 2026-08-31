namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyPyramidFootprintMutationResult(
  int TopY,
  int BottomYExclusive,
  int TunnelWidth,
  int PillarTileCount,
  int WallTileCount);
