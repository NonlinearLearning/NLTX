namespace MinimalImplementation.Server.World;

/// <summary>Runs the Version4 eight-direction wall ray and five-ray enclosure check.</summary>
public static class UnbreakableWallScanner
{
  private const int ScanDistance = 250;
  private const ushort UnbreakableWallType = 350;
  private const byte MinimumWallColor = 16;

  private static readonly TileCoordinate[] Directions =
  [
    new(1, 0),
    new(1, 1),
    new(0, 1),
    new(-1, 1),
    new(-1, 0),
    new(-1, -1),
    new(0, -1),
    new(1, -1)
  ];

  public static bool IsInside(
    TileCoordinate origin,
    WorldGeometry geometry,
    IReadOnlyList<WorldTile> tiles)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    if (tiles.Count != geometry.TileCount)
    {
      throw new ArgumentException("The tile list does not match the world geometry.", nameof(tiles));
    }

    byte hitMask = 0;
    for (int index = 0; index < Directions.Length; index++)
    {
      if (LineEndsAtUnbreakableWall(origin, Directions[index], geometry, tiles))
      {
        hitMask |= (byte)(1 << index);
      }
    }

    for (int rotation = 0; rotation < Directions.Length; rotation++)
    {
      if ((hitMask & 0x1f) == 0)
      {
        return false;
      }

      hitMask = (byte)((hitMask << 1) | (hitMask >> 7));
    }

    return true;
  }

  public static bool TryEvaluate(
    TileCoordinate origin,
    WorldGeometry geometry,
    IReadOnlyList<WorldTile> tiles,
    out bool inside)
  {
    inside = false;
    if (!geometry.Contains(origin) || tiles.Count != geometry.TileCount)
    {
      return false;
    }

    inside = IsInside(origin, geometry, tiles);
    return true;
  }

  private static bool LineEndsAtUnbreakableWall(
    TileCoordinate origin,
    TileCoordinate direction,
    WorldGeometry geometry,
    IReadOnlyList<WorldTile> tiles)
  {
    TileCoordinate coordinate = origin;
    for (int step = 0; step < ScanDistance; step++)
    {
      if (!geometry.Contains(coordinate))
      {
        return false;
      }

      WorldTile tile = tiles[(coordinate.Y * geometry.Width) + coordinate.X];
      if (tile.Wall == UnbreakableWallType)
      {
        return tile.WallColor >= MinimumWallColor;
      }

      coordinate = new TileCoordinate(coordinate.X + direction.X, coordinate.Y + direction.Y);
    }

    return false;
  }
}
