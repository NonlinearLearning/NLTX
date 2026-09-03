using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonHallGenerationSettings
{
  public DungeonHallGenerationSettings(
    DungeonHallType hallType,
    int randomSeed,
    int overridePaintTile = -1,
    int overridePaintWall = -1,
    double crackedBrickChance = 0.166,
    bool placeOverProtectedBricks = false,
    double zigzagChance = 0.66,
    bool forceStyleForDoorsAndPlatforms = false,
    bool carveOnly = false,
    int overrideInnerBoundsSize = 0,
    int overrideOuterBoundsSize = 0)
  {
    if (!Enum.IsDefined(hallType))
    {
      throw new ArgumentOutOfRangeException(nameof(hallType));
    }

    if (overridePaintTile < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(overridePaintTile));
    }

    if (overridePaintWall < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(overridePaintWall));
    }

    ValidateProbability(crackedBrickChance, nameof(crackedBrickChance));
    ValidateProbability(zigzagChance, nameof(zigzagChance));
    ArgumentOutOfRangeException.ThrowIfNegative(overrideInnerBoundsSize);
    ArgumentOutOfRangeException.ThrowIfNegative(overrideOuterBoundsSize);

    HallType = hallType;
    RandomSeed = randomSeed;
    OverridePaintTile = overridePaintTile;
    OverridePaintWall = overridePaintWall;
    CrackedBrickChance = crackedBrickChance;
    PlaceOverProtectedBricks = placeOverProtectedBricks;
    ZigzagChance = zigzagChance;
    ForceStyleForDoorsAndPlatforms = forceStyleForDoorsAndPlatforms;
    CarveOnly = carveOnly;
    OverrideInnerBoundsSize = overrideInnerBoundsSize;
    OverrideOuterBoundsSize = overrideOuterBoundsSize;
  }

  public DungeonHallType HallType { get; }

  public int RandomSeed { get; }

  public int OverridePaintTile { get; }

  public int OverridePaintWall { get; }

  public double CrackedBrickChance { get; }

  public bool PlaceOverProtectedBricks { get; }

  public double ZigzagChance { get; }

  public bool ForceStyleForDoorsAndPlatforms { get; }

  public bool CarveOnly { get; }

  public int OverrideInnerBoundsSize { get; }

  public int OverrideOuterBoundsSize { get; }

  private static void ValidateProbability(double value, string parameterName)
  {
    if (double.IsNaN(value) || value < 0 || value > 1)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
