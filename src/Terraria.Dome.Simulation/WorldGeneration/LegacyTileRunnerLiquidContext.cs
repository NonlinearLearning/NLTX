using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerLiquidContext(
  int WaterLine,
  int LavaLine,
  byte LiquidType,
  bool RemixWorld,
  int RockLayerY,
  int WorldHeight,
  bool IsOceanDepth)
{
  public void Validate()
  {
    if (WaterLine < 0 || LavaLine < 0 || WaterLine >= WorldHeight || LavaLine >= WorldHeight ||
        RockLayerY < 0 || RockLayerY >= WorldHeight)
    {
      throw new ArgumentOutOfRangeException(nameof(WaterLine));
    }
  }

  public byte ResolveInjectedLiquidType(int y)
  {
    bool suppressesLava = RemixWorld && y > LavaLine &&
      (y < RockLayerY - 80 || y > WorldHeight - 350) && IsOceanDepth;
    return y > LavaLine && !suppressesLava ? (byte)1 : LiquidType;
  }

  public bool ShouldInject(int tileType, bool isActive, int y)
  {
    return tileType == -2 && isActive && (y < WaterLine || y > LavaLine);
  }
}
