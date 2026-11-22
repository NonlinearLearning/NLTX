using System;
using System.Collections.Generic;

namespace Terraria.World;

/// <summary>Owns the dense tile values for one world runtime.</summary>
/// <remarks>
/// Tile storage is intentionally a bounded row-major array rather than one Arch entity per tile.
/// The runtime exposes only value snapshots and checked operations across this boundary.
/// </remarks>
public sealed class WorldTileStorageComponent
{
  private readonly object _gate = new();
  private readonly WorldTileState[] _tiles;

  public WorldTileStorageComponent(int width, int height, WorldTileState defaultTile = default)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    if (!defaultTile.IsValid)
    {
      throw new ArgumentException("The default tile facts are invalid.", nameof(defaultTile));
    }

    long tileCount = (long)width * height;
    if (tileCount > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(width),
          "The world tile storage is too large for one managed array.");
    }

    Width = width;
    Height = height;
    _tiles = new WorldTileState[(int)tileCount];
    if (defaultTile != default)
    {
      Array.Fill(_tiles, defaultTile);
    }
  }

  public int Width { get; }

  public int Height { get; }

  public int Count => _tiles.Length;

  public bool Contains(WorldTileCoordinate coordinate)
  {
    return WorldTileIndexRules.Contains(Width, Height, coordinate);
  }

  public bool TryRead(WorldTileCoordinate coordinate, out WorldTileState tile)
  {
    lock (_gate)
    {
      if (!TryGetIndex(coordinate, out int index))
      {
        tile = default;
        return false;
      }

      tile = _tiles[index];
      return true;
    }
  }

  public IReadOnlyList<WorldTileState> CaptureSnapshot()
  {
    lock (_gate)
    {
      return Array.AsReadOnly(_tiles.ToArray());
    }
  }

  internal bool TryWrite(WorldTileCoordinate coordinate, WorldTileState tile)
  {
    lock (_gate)
    {
      if (!TryGetIndex(coordinate, out int index))
      {
        return false;
      }

      _tiles[index] = tile;
      return true;
    }
  }

  internal void Replace(IReadOnlyList<WorldTileState> tiles)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    if (tiles.Count != _tiles.Length)
    {
      throw new ArgumentException(
          "The replacement tile count does not match storage.", nameof(tiles));
    }

    lock (_gate)
    {
      for (int index = 0; index < _tiles.Length; index++)
      {
        _tiles[index] = tiles[index];
      }
    }
  }

  internal bool TryGetIndex(WorldTileCoordinate coordinate, out int index)
  {
    return WorldTileIndexRules.TryGetIndex(Width, Height, coordinate, out index);
  }

  internal WorldTileState ReadUnchecked(WorldTileCoordinate coordinate)
  {
    return _tiles[WorldTileIndexRules.ToIndex(Width, coordinate)];
  }

  internal void WriteUnchecked(WorldTileCoordinate coordinate, WorldTileState tile)
  {
    _tiles[WorldTileIndexRules.ToIndex(Width, coordinate)] = tile;
  }
}
