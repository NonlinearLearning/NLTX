namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileCountSchedulingResult(
  TileCountSchedulingState State,
  bool ShouldScanColumn,
  int ColumnX);
