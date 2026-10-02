namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class WorldMapSnapshotComponent
{
  public const int DefaultBlackEdgeWidth = 40;

  private readonly MapTileSnapshotComponent[,] _tiles;

  public WorldMapSnapshotComponent(
    int maxWidth,
    int maxHeight,
    int blackEdgeWidth = DefaultBlackEdgeWidth)
  {
    if (maxWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxWidth));
    }

    if (maxHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxHeight));
    }

    if (blackEdgeWidth < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(blackEdgeWidth));
    }

    MaxWidth = maxWidth;
    MaxHeight = maxHeight;
    BlackEdgeWidth = blackEdgeWidth;
    _tiles = new MapTileSnapshotComponent[maxWidth, maxHeight];
    for (int x = 0; x < maxWidth; x++)
    {
      for (int y = 0; y < maxHeight; y++)
      {
        _tiles[x, y] = new MapTileSnapshotComponent(0, 0, 0);
      }
    }
  }

  public int MaxWidth { get; }

  public int MaxHeight { get; }

  public int BlackEdgeWidth { get; }

  public uint Revision { get; private set; }

  internal MapTileSnapshotComponent GetTile(int x, int y)
  {
    ValidateCoordinates(x, y);
    return _tiles[x, y];
  }

  internal void SetTile(int x, int y, MapTileSnapshotValue value)
  {
    ValidateCoordinates(x, y);
    MapTileSnapshotComponent tile = _tiles[x, y];
    tile.Clear();
    MapTileSnapshotComponent replacement = new(value.Type, value.Light, value.Color)
    {
      IsChanged = value.IsChanged,
      UpdateQueued = value.UpdateQueued
    };
    _tiles[x, y] = replacement;
    Revision++;
  }

  internal void Clear()
  {
    for (int x = 0; x < MaxWidth; x++)
    {
      for (int y = 0; y < MaxHeight; y++)
      {
        _tiles[x, y].Clear();
      }
    }

    Revision++;
  }

  private void ValidateCoordinates(int x, int y)
  {
    if ((uint)x >= (uint)MaxWidth || (uint)y >= (uint)MaxHeight)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }
  }
}
