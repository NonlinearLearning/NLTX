using System;
using System.Collections.Generic;

namespace Terraria.SpatialSimulation;

/// <summary>
/// Provides coordinate lookup over a unique, immutable tile snapshot.
/// </summary>
public sealed class SpatialTileLookupSnapshot : ISpatialTileLookup
{
  private readonly Dictionary<(int X, int Y), SpatialTileSnapshot> _tiles;

  private SpatialTileLookupSnapshot(
    Dictionary<(int X, int Y), SpatialTileSnapshot> tiles)
  {
    _tiles = tiles;
  }

  public static bool TryCreate(
    IReadOnlyList<SpatialTileSnapshot> tiles,
    out SpatialTileLookupSnapshot lookup)
  {
    ArgumentNullException.ThrowIfNull(tiles);

    Dictionary<(int X, int Y), SpatialTileSnapshot> indexedTiles = new();
    for (int index = 0; index < tiles.Count; index++)
    {
      SpatialTileSnapshot tile = tiles[index];
      if (!indexedTiles.TryAdd((tile.X, tile.Y), tile))
      {
        lookup = null!;
        return false;
      }
    }

    lookup = new SpatialTileLookupSnapshot(indexedTiles);
    return true;
  }

  public bool TryGetTile(int x, int y, out SpatialTileSnapshot tile)
  {
    return _tiles.TryGetValue((x, y), out tile);
  }
}
