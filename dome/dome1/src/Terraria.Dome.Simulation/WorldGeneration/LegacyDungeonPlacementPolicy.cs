using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyDungeonPlacement(
  LegacyDungeonSide Side,
  int Location);

public static class LegacyDungeonPlacementPolicy
{
  private const int DungeonBeachPadding = 50;

  public static LegacyDungeonPlacement Select(
    int worldWidth,
    LegacyBeachBounds beaches,
    LegacyPassRandomState random,
    LegacyDungeonSide initialSide,
    bool drunkWorldGen,
    bool dontStarveWorldGen,
    bool remixWorldGen,
    bool tenthAnniversaryWorldGen)
  {
    ArgumentNullException.ThrowIfNull(beaches);
    ArgumentNullException.ThrowIfNull(random);
    if (worldWidth <= 0 || !Enum.IsDefined(initialSide))
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    LegacyDungeonSide side = initialSide;
    if (drunkWorldGen && (!dontStarveWorldGen || remixWorldGen))
    {
      side = Flip(side);
    }

    int location = SampleLocation(worldWidth, beaches, random, side);
    if (drunkWorldGen && (!dontStarveWorldGen || tenthAnniversaryWorldGen))
    {
      side = Flip(side);
    }

    return new LegacyDungeonPlacement(side, location);
  }

  public static IReadOnlyList<LegacyDungeonPlacement> SelectPair(
    int worldWidth,
    LegacyBeachBounds beaches,
    LegacyPassRandomState random,
    LegacyDungeonSide initialSide,
    bool dualDungeonsEnabled,
    bool drunkWorldGen,
    bool dontStarveWorldGen,
    bool remixWorldGen,
    bool tenthAnniversaryWorldGen)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (!dualDungeonsEnabled)
    {
      return new[]
      {
        Select(
          worldWidth,
          beaches,
          random,
          initialSide,
          drunkWorldGen,
          dontStarveWorldGen,
          remixWorldGen,
          tenthAnniversaryWorldGen)
      };
    }

    LegacyDungeonPlacement first = Select(
      worldWidth,
      beaches,
      random,
      initialSide,
      drunkWorldGen,
      dontStarveWorldGen,
      remixWorldGen,
      tenthAnniversaryWorldGen);
    LegacyDungeonPlacement second = new(
      Flip(first.Side),
      SampleLocation(worldWidth, beaches, random, Flip(first.Side)));
    return new[] { first, second };
  }

  private static int SampleLocation(
    int worldWidth,
    LegacyBeachBounds beaches,
    LegacyPassRandomState random,
    LegacyDungeonSide side)
  {
    int minimum = side == LegacyDungeonSide.Right
      ? (int)(worldWidth * 0.8)
      : beaches.LeftBeachEnd + DungeonBeachPadding;
    int maximum = side == LegacyDungeonSide.Right
      ? beaches.RightBeachStart - DungeonBeachPadding
      : (int)(worldWidth * 0.2);
    if (minimum >= maximum)
    {
      throw new InvalidOperationException("Dungeon location range was empty.");
    }

    return random.Next(minimum, maximum);
  }

  private static LegacyDungeonSide Flip(LegacyDungeonSide side)
  {
    return side == LegacyDungeonSide.Left ? LegacyDungeonSide.Right : LegacyDungeonSide.Left;
  }
}
