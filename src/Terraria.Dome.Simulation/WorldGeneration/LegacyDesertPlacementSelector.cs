using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyDesertPlacementState(
  LegacyDungeonSide DungeonSide,
  int FailureCount,
  int DirectionFlipCount,
  bool SkipDesertTileCheck);

public readonly record struct LegacyDesertPlacementCandidate(
  int X,
  int Y,
  LegacyDesertPlacementState State);

public static class LegacyDesertPlacementSelector
{
  private const int CenterDivision = 2;
  private const int InitialOffsetDivisor = 8;
  private const int RetryOffsetDivisor = 2;
  private const int FailureFlipThreshold = 4;
  private const int SkipCheckFlipCount = 2;

  public static LegacyDesertPlacementCandidate SelectInitial(
    int worldWidth,
    double worldSurfaceHigh,
    LegacyDungeonSide dungeonSide,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (worldWidth <= 0 || double.IsNaN(worldSurfaceHigh) ||
        double.IsInfinity(worldSurfaceHigh))
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    int center = worldWidth / CenterDivision;
    int offset = random.Next(center) / InitialOffsetDivisor + center / InitialOffsetDivisor;
    int x = center + offset * (dungeonSide == LegacyDungeonSide.Left ? -1 : 1);
    return new LegacyDesertPlacementCandidate(
      x,
      (int)worldSurfaceHigh + 25,
      new LegacyDesertPlacementState(dungeonSide, 0, 0, false));
  }

  public static LegacyDesertPlacementCandidate SelectRetry(
    int worldWidth,
    double worldSurfaceHigh,
    LegacyDesertPlacementCandidate previous,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (worldWidth <= 0 || double.IsNaN(worldSurfaceHigh) ||
        double.IsInfinity(worldSurfaceHigh))
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    int center = worldWidth / CenterDivision;
    int offset = random.Next(center) / RetryOffsetDivisor + center / InitialOffsetDivisor;
    offset += random.Next(previous.State.FailureCount / 12);
    int x = center + offset * (previous.State.DungeonSide == LegacyDungeonSide.Left ? -1 : 1);
    int failures = previous.State.FailureCount + 1;
    int flips = previous.State.DirectionFlipCount;
    LegacyDungeonSide side = previous.State.DungeonSide;
    bool skipCheck = previous.State.SkipDesertTileCheck;
    if (failures > worldWidth / FailureFlipThreshold)
    {
      side = side == LegacyDungeonSide.Left ? LegacyDungeonSide.Right : LegacyDungeonSide.Left;
      failures = 0;
      flips++;
      skipCheck = flips >= SkipCheckFlipCount;
    }

    return new LegacyDesertPlacementCandidate(
      x,
      (int)worldSurfaceHigh + 25,
      new LegacyDesertPlacementState(side, failures, flips, skipCheck));
  }
}
