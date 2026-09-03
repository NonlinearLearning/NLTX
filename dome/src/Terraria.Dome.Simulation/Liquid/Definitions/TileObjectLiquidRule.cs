using System;
using Terraria.Dome.Simulation.Liquid.Components;

namespace Terraria.Dome.Simulation.Liquid.Definitions;

public sealed class TileObjectLiquidRule
{
  public TileObjectLiquidRule(
    ushort tileType,
    LiquidType liquidType,
    int width,
    int height,
    short minimumOriginFrameX,
    short maximumOriginFrameX,
    bool destroysTile)
  {
    if (width <= 0 || height <= 0 || minimumOriginFrameX > maximumOriginFrameX)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    TileType = tileType;
    LiquidType = liquidType;
    Width = width;
    Height = height;
    MinimumOriginFrameX = minimumOriginFrameX;
    MaximumOriginFrameX = maximumOriginFrameX;
    DestroysTile = destroysTile;
  }

  public bool DestroysTile { get; }
  public int Height { get; }
  public LiquidType LiquidType { get; }
  public short MaximumOriginFrameX { get; }
  public short MinimumOriginFrameX { get; }
  public ushort TileType { get; }
  public int Width { get; }

  public bool MatchesOriginFrame(short frameX)
  {
    return frameX >= MinimumOriginFrameX && frameX <= MaximumOriginFrameX;
  }
}
