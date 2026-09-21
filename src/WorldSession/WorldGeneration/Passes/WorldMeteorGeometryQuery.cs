using System;

namespace Terraria.WorldGeneration.Passes;

public static class WorldMeteorGeometryQuery
{
  private const int HorizontalMargin = 150;
  private const double SpawnExclusionFraction = 0.08;
  private const double SurfaceStartFraction = 0.3;

  public static WorldMeteorSpawnGeometry Calculate(
    int maxTilesX,
    int maxTilesY,
    double worldSurface,
    double rockLayer,
    int underworldLayer,
    int spawnTileX,
    bool spawnUnderGround)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTilesX);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTilesY);

    if (maxTilesX <= HorizontalMargin * 2)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesX));
    }

    if (!double.IsFinite(worldSurface))
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurface));
    }

    if (!double.IsFinite(rockLayer))
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayer));
    }

    int verticalMinInclusive = spawnUnderGround
      ? (int)(worldSurface + rockLayer) / 2
      : (int)(worldSurface * SurfaceStartFraction);

    if (verticalMinInclusive < 0 || verticalMinInclusive >= maxTilesY)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurface));
    }

    if (spawnUnderGround &&
      (underworldLayer <= verticalMinInclusive ||
      underworldLayer > maxTilesY))
    {
      throw new ArgumentOutOfRangeException(nameof(underworldLayer));
    }

    return new WorldMeteorSpawnGeometry(
      horizontalMinInclusive: HorizontalMargin,
      horizontalMaxExclusive: maxTilesX - HorizontalMargin,
      verticalMinInclusive: verticalMinInclusive,
      verticalMaxExclusive: maxTilesY,
      spawnTileX: spawnTileX,
      spawnTileExclusionDistance: maxTilesX * SpawnExclusionFraction);
  }
}
