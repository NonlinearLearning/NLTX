namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapPersistenceSnapshot
{
  public MapPersistenceSnapshot(
    int maxWidth,
    int maxHeight,
    int blackEdgeWidth,
    uint sourceRevision,
    ReadOnlySpan<MapTileSnapshotValue> tiles)
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

    int expectedCount = checked(maxWidth * maxHeight);
    if (tiles.Length != expectedCount)
    {
      throw new ArgumentException("The tile snapshot does not match the map dimensions.", nameof(tiles));
    }

    MaxWidth = maxWidth;
    MaxHeight = maxHeight;
    BlackEdgeWidth = blackEdgeWidth;
    SourceRevision = sourceRevision;
    Tiles = tiles.ToArray();
  }

  public int MaxWidth { get; }

  public int MaxHeight { get; }

  public int BlackEdgeWidth { get; }

  public uint SourceRevision { get; }

  public ReadOnlyMemory<MapTileSnapshotValue> Tiles { get; }
}
