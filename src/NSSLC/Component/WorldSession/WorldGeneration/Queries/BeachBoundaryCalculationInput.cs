namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Explicit inputs for the Version4 beach-boundary calculation.
/// </summary>
public readonly record struct BeachBoundaryCalculationInput(
  long GenerationId,
  int MaxTilesX,
  int BeachBordersWidth,
  int BeachSandRandomCenter,
  int BeachSandRandomWidthRange,
  int BeachSandDungeonExtraWidth,
  int BeachSandJungleExtraWidth,
  int ShellStartXLeft,
  int ShellStartYLeft,
  int ShellStartXRight,
  int ShellStartYRight,
  int OceanWaterStartRandomMin,
  short DungeonSide,
  bool IsTenthAnniversaryWorld,
  bool IsRemixWorld,
  int LeftBeachRandomRoll,
  int RightBeachRandomRoll);
