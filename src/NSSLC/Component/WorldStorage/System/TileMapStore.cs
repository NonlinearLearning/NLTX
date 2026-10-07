using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

public sealed class TileMapStore
{
  private TileMapSnapshot _tiles = new(0, 0, Array.Empty<TileCellState>());
  private readonly Dictionary<int, TileCellState> _pendingTileChanges = new();
  private long _mutationRevision;
  private bool _isDisposed;

  private TileMapLayout _layout;

  public TileMapLayout Layout
  {
    get
    {
      VerifyAccess();
      return _layout;
    }
    private set => _layout = value;
  }

  public int Width
  {
    get
    {
      VerifyAccess();
      return _tiles.Width;
    }
  }

  public int Height
  {
    get
    {
      VerifyAccess();
      return _tiles.Height;
    }
  }

  public long MutationRevision
  {
    get
    {
      VerifyAccess();
      return _mutationRevision;
    }
  }

  public TileCellState GetTile(int x, int y) {
    VerifyAccess();
    TileCellState tile = _tiles.GetTile(x, y);
    int index = x * _tiles.Height + y;
    return _pendingTileChanges.TryGetValue(index, out TileCellState changed)
      ? changed
      : tile;
  }

  public TileMapSnapshot CreateSnapshot() {
    VerifyAccess();
    if (_pendingTileChanges.Count > 0) {
      _tiles = _tiles.WithChanges(_pendingTileChanges);
      _pendingTileChanges.Clear();
    }
    return _tiles;
  }

  /// <summary>Commits one owner-thread tile mutation without copying the full world map.</summary>
  public bool CommitTile(int x, int y, TileCellState tile) {
    VerifyAccess();
    TileCellState current = GetTile(x, y);
    if (current.Equals(tile)) {
      return false;
    }
    _pendingTileChanges[x * _tiles.Height + y] = tile;
    _mutationRevision++;
    return true;
  }

  internal void Replace(TileMapSnapshot tiles) {
    VerifyAccess();
    _tiles = tiles;
    _pendingTileChanges.Clear();
    Layout = new TileMapLayout(tiles.Width, tiles.Height, 200, 150);
    _mutationRevision++;
  }

  internal void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    _tiles = new TileMapSnapshot(0, 0, Array.Empty<TileCellState>());
    _pendingTileChanges.Clear();
    _layout = default;
    _isDisposed = true;
  }

  private void VerifyAccess()
  {
    ObjectDisposedException.ThrowIf(_isDisposed, this);
  }
}
