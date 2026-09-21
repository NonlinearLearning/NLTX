using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonWormlikeRoomSettings
{
  public DungeonWormlikeRoomSettings(int firstSideIterations, int secondSideIterations)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(firstSideIterations);
    ArgumentOutOfRangeException.ThrowIfNegative(secondSideIterations);
    FirstSideIterations = firstSideIterations;
    SecondSideIterations = secondSideIterations;
  }

  public int FirstSideIterations { get; }

  public int SecondSideIterations { get; }

  public int GetBoundingRadius()
  {
    return DungeonRoomBoundingRadiusQuery.Wormlike(
      FirstSideIterations,
      SecondSideIterations);
  }
}
