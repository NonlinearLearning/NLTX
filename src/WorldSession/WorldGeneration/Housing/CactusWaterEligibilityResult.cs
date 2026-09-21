namespace Terraria.WorldGeneration.Housing;

public readonly record struct CactusWaterEligibilityResult(
  long LiquidUnits,
  int ScannedCellCount,
  bool ExceedsLimit);
