using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonLivingTreeRoomSettings
{
  public DungeonLivingTreeRoomSettings(
    int innerWidth,
    int innerHeight,
    int depth,
    int boundingRadius)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(innerWidth);
    ArgumentOutOfRangeException.ThrowIfNegative(innerHeight);
    ArgumentOutOfRangeException.ThrowIfNegative(depth);
    ArgumentOutOfRangeException.ThrowIfNegative(boundingRadius);
    InnerWidth = innerWidth;
    InnerHeight = innerHeight;
    Depth = depth;
    BoundingRadius = boundingRadius;
  }

  public int InnerWidth { get; }

  public int InnerHeight { get; }

  public int Depth { get; }

  public int BoundingRadius { get; }

  public int GetBoundingRadius()
  {
    return DungeonRoomBoundingRadiusQuery.LivingTree(BoundingRadius);
  }
}
