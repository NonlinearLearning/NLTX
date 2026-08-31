namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyErrorWorldPassCounts(
  int RandomBlockRewrites,
  int SingleTileSwaps,
  int RectangleSwaps,
  int AxisTrails);
