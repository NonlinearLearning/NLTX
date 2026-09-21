namespace NLTX.ClientPresentation.MapCameraRendering;

public static class MapSaveProjection
{
  public static MapPersistenceSnapshot Capture(WorldMapSnapshotComponent map)
  {
    ArgumentNullException.ThrowIfNull(map);
    MapTileSnapshotValue[] tiles = new MapTileSnapshotValue[
      checked(map.MaxWidth * map.MaxHeight)];
    int index = 0;
    for (int y = 0; y < map.MaxHeight; y++)
    {
      for (int x = 0; x < map.MaxWidth; x++)
      {
        if (!WorldMapQuery.TryRead(map, x, y, out MapTileSnapshotValue value))
        {
          throw new InvalidOperationException("The world map snapshot changed during capture.");
        }

        tiles[index++] = value;
      }
    }

    return new MapPersistenceSnapshot(
      map.MaxWidth,
      map.MaxHeight,
      map.BlackEdgeWidth,
      map.Revision,
      tiles);
  }
}
