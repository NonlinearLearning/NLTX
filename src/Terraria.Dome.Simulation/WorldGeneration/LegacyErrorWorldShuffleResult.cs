namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyErrorWorldShuffleResult(
  int RandomBlockRewrites,
  int SingleTileSwaps,
  int RectangleSwaps,
  int AxisTrails,
  bool CompletedWithoutRejection);
