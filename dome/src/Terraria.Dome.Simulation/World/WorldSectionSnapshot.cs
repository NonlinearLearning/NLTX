using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldSectionSnapshot
{
  private readonly WorldTile[,] _tiles;

  public WorldSectionSnapshot(
    WorldSectionCoordinates coordinates,
    int width,
    int height,
    long version,
    WorldTile[,] tiles)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentNullException.ThrowIfNull(tiles);
    if (tiles.GetLength(0) != width || tiles.GetLength(1) != height)
    {
      throw new ArgumentException(
        "Tile dimensions must match the section dimensions.",
        nameof(tiles));
    }

    Coordinates = coordinates;
    Height = height;
    Version = version;
    Width = width;
    _tiles = (WorldTile[,])tiles.Clone();
  }

  public WorldSectionCoordinates Coordinates { get; }
  public int Height { get; }
  public long Version { get; }
  public int Width { get; }

  public WorldTile GetTile(int x, int y)
  {
    if (x < 0 || x >= Width || y < 0 || y >= Height)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    return _tiles[x, y];
  }
}
