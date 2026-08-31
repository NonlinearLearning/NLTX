namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct CactusWaterSafetyResult(
  long LiquidUnits,
  int ScannedCellCount,
  bool ExceedsLimit);
