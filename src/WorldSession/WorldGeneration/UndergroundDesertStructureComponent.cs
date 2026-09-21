using System;

namespace Terraria.WorldGeneration.Components;

public sealed class UndergroundDesertStructureComponent
{
  public UndergroundDesertStructureComponent(
    long generationId,
    UndergroundDesertRectangle undergroundDesertLocation = default,
    UndergroundDesertRectangle undergroundDesertHiveLocation = default,
    int desertHiveHigh = 0,
    int desertHiveLow = 0,
    int desertHiveLeft = 0,
    int desertHiveRight = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    ReplaceLayout(
      undergroundDesertLocation,
      undergroundDesertHiveLocation,
      desertHiveHigh,
      desertHiveLow,
      desertHiveLeft,
      desertHiveRight);
  }

  public long GenerationId { get; }

  public UndergroundDesertRectangle UndergroundDesertLocation { get; private set; }

  public UndergroundDesertRectangle UndergroundDesertHiveLocation { get; private set; }

  public int DesertHiveHigh { get; private set; }

  public int DesertHiveLow { get; private set; }

  public int DesertHiveLeft { get; private set; }

  public int DesertHiveRight { get; private set; }

  public void ReplaceLayout(
    UndergroundDesertRectangle undergroundDesertLocation,
    UndergroundDesertRectangle undergroundDesertHiveLocation,
    int desertHiveHigh,
    int desertHiveLow,
    int desertHiveLeft,
    int desertHiveRight)
  {
    UndergroundDesertLocation = undergroundDesertLocation;
    UndergroundDesertHiveLocation = undergroundDesertHiveLocation;
    DesertHiveHigh = desertHiveHigh;
    DesertHiveLow = desertHiveLow;
    DesertHiveLeft = desertHiveLeft;
    DesertHiveRight = desertHiveRight;
  }

  public UndergroundDesertStructureSnapshot CreateSnapshot()
  {
    return new UndergroundDesertStructureSnapshot(
      GenerationId,
      UndergroundDesertLocation,
      UndergroundDesertHiveLocation,
      DesertHiveHigh,
      DesertHiveLow,
      DesertHiveLeft,
      DesertHiveRight);
  }
}
