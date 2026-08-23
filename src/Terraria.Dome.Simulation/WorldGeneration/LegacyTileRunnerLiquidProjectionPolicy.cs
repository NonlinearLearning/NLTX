using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerLiquidProjection(
  short SourceLiquidType,
  byte CommandLiquidType,
  bool SetsLava);

public static class LegacyTileRunnerLiquidProjectionPolicy
{
  private const byte LavaLiquidType = 1;

  public static LegacyTileRunnerLiquidProjection Resolve(
    short sourceLiquidType,
    bool setsLava)
  {
    if (sourceLiquidType < 0 || sourceLiquidType > 3)
    {
      throw new ArgumentOutOfRangeException(nameof(sourceLiquidType));
    }

    byte commandLiquidType = setsLava
      ? LavaLiquidType
      : checked((byte)sourceLiquidType);
    return new LegacyTileRunnerLiquidProjection(
      sourceLiquidType,
      commandLiquidType,
      setsLava);
  }
}
