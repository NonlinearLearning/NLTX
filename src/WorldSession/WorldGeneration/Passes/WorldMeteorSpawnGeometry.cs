using System;

namespace Terraria.WorldGeneration.Passes;

public readonly record struct WorldMeteorSpawnGeometry
{
  public WorldMeteorSpawnGeometry(
    int horizontalMinInclusive,
    int horizontalMaxExclusive,
    int verticalMinInclusive,
    int verticalMaxExclusive,
    int spawnTileX,
    double spawnTileExclusionDistance)
  {
    if (horizontalMaxExclusive <= horizontalMinInclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(horizontalMaxExclusive));
    }

    if (verticalMinInclusive < 0 ||
      verticalMaxExclusive <= verticalMinInclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(verticalMinInclusive));
    }

    if (!double.IsFinite(spawnTileExclusionDistance) ||
      spawnTileExclusionDistance < 0.0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(spawnTileExclusionDistance));
    }

    HorizontalMinInclusive = horizontalMinInclusive;
    HorizontalMaxExclusive = horizontalMaxExclusive;
    VerticalMinInclusive = verticalMinInclusive;
    VerticalMaxExclusive = verticalMaxExclusive;
    SpawnTileX = spawnTileX;
    SpawnTileExclusionDistance = spawnTileExclusionDistance;
  }

  public int HorizontalMinInclusive { get; }

  public int HorizontalMaxExclusive { get; }

  public int VerticalMinInclusive { get; }

  public int VerticalMaxExclusive { get; }

  public int SpawnTileX { get; }

  public double SpawnTileExclusionDistance { get; }

  public bool IsHorizontalCandidate(int tileX)
  {
    return tileX >= HorizontalMinInclusive &&
      tileX < HorizontalMaxExclusive;
  }

  public bool IsSpawnExcluded(int tileX)
  {
    return (double)tileX >
      (double)SpawnTileX - SpawnTileExclusionDistance &&
      (double)tileX <
      (double)SpawnTileX + SpawnTileExclusionDistance;
  }
}
