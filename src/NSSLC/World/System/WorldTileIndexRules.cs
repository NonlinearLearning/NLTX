namespace Terraria.Unshared.World;

/// <summary>
/// Pure row-major index arithmetic for dense tile storage.
/// </summary>
/// <remarks>
/// <see cref="WorldTileStorageComponent" /> owns the array and its lock; bounds checks and index
/// projection are computed here so the storage type only reads and writes.
/// </remarks>
public static class WorldTileIndexRules
{
  /// <summary>True when the coordinate lies inside a world of the supplied dimensions.</summary>
  public static bool Contains(int width, int height, WorldTileCoordinate coordinate)
  {
    return coordinate.X >= 0 && coordinate.X < width &&
        coordinate.Y >= 0 && coordinate.Y < height;
  }

  /// <summary>Projects a coordinate to its row-major index, reporting out-of-range input.</summary>
  public static bool TryGetIndex(
      int width,
      int height,
      WorldTileCoordinate coordinate,
      out int index)
  {
    if (!Contains(width, height, coordinate))
    {
      index = -1;
      return false;
    }

    index = ToIndex(width, coordinate);
    return true;
  }

  /// <summary>
  /// Projects a coordinate the caller has already validated onto its row-major index.
  /// </summary>
  public static int ToIndex(int width, WorldTileCoordinate coordinate)
  {
    return checked(coordinate.Y * width + coordinate.X);
  }
}
