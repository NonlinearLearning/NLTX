using System;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Passes;

public sealed class WorldLayerMetricsCalculator : IWorldLayerMetricsCalculator
{
  private const double InitialSurfaceFraction = 0.3d;
  private const double InitialRockLayerOffset = 0.2d;
  private const double DefaultSurfaceLowerFraction = 0.17d;
  private const double DefaultSurfaceUpperFraction = 0.26d;
  private const double SurfaceInSpaceUpperFraction = 0.2199999988079071d;
  private const double DrunkSurfaceLowerFraction = 0.15d;
  private const double DrunkSurfaceUpperFraction = 0.28d;
  private const double BeachSurfaceUpperFraction = 0.23d;
  private const double MinimumRockLayerGap = 20d;

  public WorldLayerMetricsSnapshot Calculate(
    in WorldLayerMetricsCalculationInput input,
    IGenerationRandomSource random)
  {
    ArgumentNullException.ThrowIfNull(random);

    int worldWidth = input.Configuration.WorldWidth;
    int worldHeight = input.Configuration.WorldHeight;
    double worldSurface = worldHeight * InitialSurfaceFraction;
    worldSurface *= NextTerrain(random, 90, 110) * 0.005d;
    double rockLayer = worldSurface + worldHeight * InitialRockLayerOffset;
    rockLayer *= NextTerrain(random, 90, 110) * 0.01d;

    if (input.IsRemixWorld)
    {
      rockLayer = worldHeight * (worldWidth > 2500 ? 0.6d : 0.5d);
      rockLayer *= NextTerrain(random, 95, 106) * 0.01d;
    }

    double worldSurfaceLow = worldSurface;
    double worldSurfaceHigh = worldSurface;
    double rockLayerLow = rockLayer;
    double rockLayerHigh = rockLayer;

    if (input.IsNoSurfaceWorld)
    {
      worldSurface = 25d;
      rockLayer = worldHeight * 0.4d;
      rockLayer *= NextTerrain(random, 90, 110) * 0.01d;
    }

    double beachSurfaceCap = worldHeight * BeachSurfaceUpperFraction;
    double surfaceLowerFraction = DefaultSurfaceLowerFraction;
    double surfaceUpperFraction = DefaultSurfaceUpperFraction;
    if (input.IsSurfaceInSpace)
    {
      surfaceUpperFraction = SurfaceInSpaceUpperFraction;
    }
    else if (input.IsDrunkWorld)
    {
      surfaceLowerFraction = DrunkSurfaceLowerFraction;
      surfaceUpperFraction = DrunkSurfaceUpperFraction;
    }

    if (WorldDimensionSelectionQuery.GetLegacyIndex(worldWidth) ==
        WorldSizeCatalogDefinition.SmallIndex)
    {
      surfaceLowerFraction += 0.02d;
    }

    double surfaceLowerBound = worldHeight * surfaceLowerFraction;
    double surfaceUpperBound = worldHeight * surfaceUpperFraction;
    double surfaceHistoryCap = worldHeight * 0.23d;
    int feature = 0;
    int featureDuration = input.LeftBeachEnd + input.FlatBeachPadding;

    for (int columnX = 0; columnX < worldWidth; columnX++)
    {
      worldSurfaceLow = Math.Min(worldSurface, worldSurfaceLow);
      worldSurfaceHigh = Math.Max(worldSurface, worldSurfaceHigh);
      rockLayerLow = Math.Min(rockLayer, rockLayerLow);
      rockLayerHigh = Math.Max(rockLayer, rockLayerHigh);

      if (featureDuration <= 0)
      {
        feature = NextTerrain(random, 0, 5);
        featureDuration = NextTerrain(random, 5, 40);
        if (feature == 0)
        {
          featureDuration *= (int)(NextTerrain(random, 5, 30) * 0.2d);
        }
      }

      featureDuration--;
      if (columnX > worldWidth * 0.45d &&
          columnX < worldWidth * 0.55d &&
          (feature == 3 || feature == 4))
      {
        feature = NextTerrain(random, 0, 3);
      }

      if (columnX > worldWidth * 0.48d && columnX < worldWidth * 0.52d)
      {
        feature = 0;
      }

      if (!input.IsNoSurfaceWorld)
      {
        bool specialSurfaceOffset =
          (input.IsDrunkWorld || input.IsGoodWorld || input.IsRemixWorld) &&
          NextWorldGeneration(random, 0, 2) == 0;
        worldSurface += GenerateSurfaceOffset(feature, random, specialSurfaceOffset);
      }

      bool isBeachColumn = columnX < input.LeftBeachEnd + input.FlatBeachPadding ||
        columnX > input.RightBeachStart - input.FlatBeachPadding;
      if (isBeachColumn)
      {
        worldSurface = Math.Clamp(
          worldSurface,
          surfaceLowerBound,
          surfaceHistoryCap);
      }
      else if (worldSurface < surfaceLowerBound)
      {
        worldSurface = surfaceLowerBound;
        featureDuration = 0;
      }
      else if (worldSurface > surfaceUpperBound)
      {
        worldSurface = surfaceUpperBound;
        featureDuration = 0;
      }

      while (NextTerrain(random, 0, 3) == 0)
      {
        rockLayer += NextTerrain(random, -2, 3);
      }

      if (input.IsNoSurfaceWorld)
      {
        if (rockLayer < worldSurface + worldHeight * 0.35d)
        {
          rockLayer += 1d;
        }

        if (rockLayer > worldSurface + worldHeight * 0.45d)
        {
          rockLayer -= 1d;
        }
      }
      else if (input.IsRemixWorld)
      {
        double remixRockLayerCap = worldHeight * (worldWidth > 2500 ? 0.7d : 0.6d);
        if (rockLayer > remixRockLayerCap)
        {
          rockLayer -= 1d;
        }
      }
      else
      {
        if (rockLayer < worldSurface + worldHeight * 0.06d)
        {
          rockLayer += 1d;
        }

        if (rockLayer > worldSurface + worldHeight * 0.35d)
        {
          rockLayer -= 1d;
        }
      }

      if (columnX == input.RightBeachStart - input.FlatBeachPadding &&
          worldSurface > surfaceHistoryCap)
      {
        feature = 0;
        featureDuration = worldWidth - columnX;
      }
    }

    if (rockLayerLow < worldSurfaceHigh + MinimumRockLayerGap)
    {
      double midpoint = (rockLayerLow + worldSurfaceHigh) / 2d;
      double gap = Math.Abs(rockLayerLow - worldSurfaceHigh);
      if (gap < MinimumRockLayerGap)
      {
        gap = MinimumRockLayerGap;
      }

      rockLayerLow = midpoint + gap / 2d;
      worldSurfaceHigh = midpoint - gap / 2d;
    }

    return new WorldLayerMetricsSnapshot(
      input.GenerationId,
      LowestCloud: -1,
      worldSurfaceLow,
      worldSurface,
      worldSurfaceHigh,
      rockLayerLow,
      rockLayer,
      rockLayerHigh,
      SnowTop: 0,
      SnowBottom: 0,
      SnowOriginLeft: 0,
      SnowOriginRight: 0,
      new int[worldHeight],
      new int[worldHeight]);
  }

  private static int NextTerrain(
    IGenerationRandomSource random,
    int minimumInclusive,
    int maximumExclusive)
  {
    return random.NextInt(
      GenerationRandomStream.Terrain,
      minimumInclusive,
      maximumExclusive);
  }

  private static int NextWorldGeneration(
    IGenerationRandomSource random,
    int minimumInclusive,
    int maximumExclusive)
  {
    return random.NextInt(
      GenerationRandomStream.WorldGeneration,
      minimumInclusive,
      maximumExclusive);
  }

  private static double GenerateSurfaceOffset(
    int feature,
    IGenerationRandomSource random,
    bool specialSurfaceOffset)
  {
    return feature switch
    {
      0 => GeneratePlateauOffset(random, specialSurfaceOffset),
      1 => GenerateHillOffset(random, specialSurfaceOffset),
      2 => GenerateDaleOffset(random, specialSurfaceOffset),
      3 => GenerateMountainOffset(random, specialSurfaceOffset),
      4 => GenerateValleyOffset(random, specialSurfaceOffset),
      _ => throw new ArgumentOutOfRangeException(nameof(feature))
    };
  }

  private static double GeneratePlateauOffset(
    IGenerationRandomSource random,
    bool specialSurfaceOffset)
  {
    double offset = 0d;
    int chance = specialSurfaceOffset ? 6 : 7;
    while (NextTerrain(random, 0, chance) == 0)
    {
      offset += NextTerrain(random, -1, 2);
    }

    return offset;
  }

  private static double GenerateHillOffset(
    IGenerationRandomSource random,
    bool specialSurfaceOffset)
  {
    double offset = 0d;
    int declineChance = specialSurfaceOffset ? 3 : 4;
    while (NextTerrain(random, 0, declineChance) == 0)
    {
      offset -= 1d;
    }

    while (NextTerrain(random, 0, 10) == 0)
    {
      offset += 1d;
    }

    return offset;
  }

  private static double GenerateDaleOffset(
    IGenerationRandomSource random,
    bool specialSurfaceOffset)
  {
    double offset = 0d;
    int inclineChance = specialSurfaceOffset ? 3 : 4;
    while (NextTerrain(random, 0, inclineChance) == 0)
    {
      offset += 1d;
    }

    while (NextTerrain(random, 0, 10) == 0)
    {
      offset -= 1d;
    }

    return offset;
  }

  private static double GenerateMountainOffset(
    IGenerationRandomSource random,
    bool specialSurfaceOffset)
  {
    double offset = 0d;
    int declineChance = specialSurfaceOffset ? 3 : 2;
    while (specialSurfaceOffset
      ? NextTerrain(random, 0, declineChance) != 0
      : NextTerrain(random, 0, declineChance) == 0)
    {
      offset -= 1d;
    }

    while (NextTerrain(random, 0, 6) == 0)
    {
      offset += 1d;
    }

    return offset;
  }

  private static double GenerateValleyOffset(
    IGenerationRandomSource random,
    bool specialSurfaceOffset)
  {
    double offset = 0d;
    int inclineChance = specialSurfaceOffset ? 3 : 2;
    while (specialSurfaceOffset
      ? NextTerrain(random, 0, inclineChance) != 0
      : NextTerrain(random, 0, inclineChance) == 0)
    {
      offset += 1d;
    }

    while (NextTerrain(random, 0, 5) == 0)
    {
      offset -= 1d;
    }

    return offset;
  }
}
