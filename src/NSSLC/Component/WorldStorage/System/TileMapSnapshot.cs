namespace Terraria.WorldStorage;

/// <summary>An immutable column-major terrain snapshot. Index = x * Height + y.</summary>
public sealed class TileMapSnapshot {
  private readonly TileCellState[] _tiles;
  public int Width { get; }
  public int Height { get; }

  public TileMapSnapshot(int width, int height, IReadOnlyList<TileCellState> tiles) {
    ValidateDimensions(width, height, tiles);
    Width = width;
    Height = height;
    _tiles = new TileCellState[tiles.Count];
    for (int i = 0; i < tiles.Count; i++) {
      _tiles[i] = tiles[i];
    }
  }

  private TileMapSnapshot(int width, int height, TileCellState[] tiles, bool takeOwnership) {
    ValidateDimensions(width, height, tiles);
    Width = width;
    Height = height;
    _tiles = takeOwnership ? tiles : (TileCellState[])tiles.Clone();
  }

  private static void ValidateDimensions(
      int width,
      int height,
      IReadOnlyList<TileCellState> tiles) {
    ArgumentOutOfRangeException.ThrowIfNegative(width);
    ArgumentOutOfRangeException.ThrowIfNegative(height);
    ArgumentNullException.ThrowIfNull(tiles);
    if (checked(width * height) != tiles.Count) {
      throw new ArgumentException("The tile count does not match the dimensions.", nameof(tiles));
    }
  }

  public TileCellState GetTile(int x, int y) {
    if ((uint)x >= Width || (uint)y >= Height) {
      throw new ArgumentOutOfRangeException(nameof(x), "Tile coordinates are outside the world.");
    }
    return _tiles[x * Height + y];
  }

  internal TileMapSnapshot WithChanges(IReadOnlyDictionary<int, TileCellState> changes) {
    ArgumentNullException.ThrowIfNull(changes);
    var tiles = (TileCellState[])_tiles.Clone();
    foreach ((int index, TileCellState tile) in changes) {
      if ((uint)index >= (uint)tiles.Length) {
        throw new ArgumentOutOfRangeException(nameof(changes));
      }
      tiles[index] = tile;
    }
    return new TileMapSnapshot(Width, Height, tiles, takeOwnership: true);
  }
}
