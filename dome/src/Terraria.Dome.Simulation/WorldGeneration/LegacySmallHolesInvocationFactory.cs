using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacySmallHolesInvocationPair(
  LegacyTileRunnerRequest First,
  LegacyTileRunnerRequest Second,
  int TileType);

public static class LegacySmallHolesInvocationFactory
{
  public static LegacySmallHolesInvocationPair Create(
    LegacySmallHolesPassDefinition definition,
    LegacyPassRandomState random,
    int width,
    int height,
    int worldSurfaceHighY)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    if (width <= 0 || height <= 0 || worldSurfaceHighY < 0 || worldSurfaceHighY >= height)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int tileType = definition.SelectTileType(random);
    LegacyTileRunnerRequest first = CreateRequest(
      random,
      width,
      height,
      worldSurfaceHighY,
      definition.MinimumFirstStrength,
      definition.MaximumFirstStrengthExclusive,
      definition.MinimumFirstSteps,
      definition.MaximumFirstStepsExclusive,
      tileType);
    LegacyTileRunnerRequest second = CreateRequest(
      random,
      width,
      height,
      worldSurfaceHighY,
      definition.MinimumSecondStrength,
      definition.MaximumSecondStrengthExclusive,
      definition.MinimumSecondSteps,
      definition.MaximumSecondStepsExclusive,
      tileType);
    return new LegacySmallHolesInvocationPair(first, second, tileType);
  }

  private static LegacyTileRunnerRequest CreateRequest(
    LegacyPassRandomState random,
    int width,
    int height,
    int worldSurfaceHighY,
    int minimumStrength,
    int maximumStrengthExclusive,
    int minimumSteps,
    int maximumStepsExclusive,
    int tileType)
  {
    return new LegacyTileRunnerRequest(
      random.Next(width),
      random.Next(worldSurfaceHighY, height),
      random.Next(minimumStrength, maximumStrengthExclusive),
      random.Next(minimumSteps, maximumStepsExclusive),
      tileType,
      addTile: false,
      speedX: 0.0,
      speedY: 0.0,
      noYChange: false,
      overwrite: true,
      ignoreTileType: -1);
  }
}
