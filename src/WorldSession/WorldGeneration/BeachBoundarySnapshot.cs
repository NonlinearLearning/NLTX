namespace Terraria.WorldGeneration.Components;

public readonly record struct BeachBoundarySnapshot(
  long GenerationId,
  int LeftBeachEnd,
  int RightBeachStart,
  int BeachBordersWidth,
  int BeachSandRandomCenter,
  int BeachSandRandomWidthRange,
  int BeachSandDungeonExtraWidth,
  int BeachSandJungleExtraWidth,
  int ShellStartXLeft,
  int ShellStartYLeft,
  int ShellStartXRight,
  int ShellStartYRight,
  int OceanWaterStartRandomMin);
