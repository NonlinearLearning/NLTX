using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public enum LegacyDungeonSide
{
  Left,
  Right
}

public sealed record LegacyBeachBounds(
  int LeftBeachEnd,
  int RightBeachStart,
  int BeachSandRandomCenter,
  int BeachSandRandomWidthRange,
  int BeachSandDungeonExtraWidth,
  int BeachSandJungleExtraWidth);

public static class LegacyBeachBoundsDefinition
{
  private const int BeachBordersWidth = 275;
  private const int BeachSandRandomCenterPadding = 5;
  private const int BeachSandRandomCenterOffset = 40;
  private const int BeachSandRandomWidthRange = 20;
  private const int BeachSandDungeonExtraWidth = 40;
  private const int BeachSandJungleExtraWidth = 20;

  public static LegacyBeachBounds Calculate(
    LegacyPassRandomState random,
    int worldWidth,
    LegacyDungeonSide dungeonSide,
    bool tenthAnniversaryWorld,
    bool remixWorld)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    int center = BeachBordersWidth + BeachSandRandomCenterPadding +
      BeachSandRandomCenterOffset;
    int leftWidth = random.Next(center - BeachSandRandomWidthRange,
      center + BeachSandRandomWidthRange);
    if (tenthAnniversaryWorld && !remixWorld)
    {
      leftWidth = center + BeachSandRandomWidthRange;
    }

    int leftBeachEnd = leftWidth + (dungeonSide == LegacyDungeonSide.Right
      ? BeachSandDungeonExtraWidth
      : BeachSandJungleExtraWidth);

    int rightWidth = random.Next(center - BeachSandRandomWidthRange,
      center + BeachSandRandomWidthRange);
    if (tenthAnniversaryWorld && !remixWorld)
    {
      rightWidth = center + BeachSandRandomWidthRange;
    }

    int rightBeachStart = worldWidth - rightWidth -
      (dungeonSide == LegacyDungeonSide.Left
        ? BeachSandDungeonExtraWidth
        : BeachSandJungleExtraWidth);
    return new LegacyBeachBounds(
      leftBeachEnd,
      rightBeachStart,
      center,
      BeachSandRandomWidthRange,
      BeachSandDungeonExtraWidth,
      BeachSandJungleExtraWidth);
  }
}
