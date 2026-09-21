namespace NLTX.ClientPresentation.MapCameraRendering;

public static class WorldMapQuery
{
  public static bool TryRead(
    WorldMapSnapshotComponent map,
    int x,
    int y,
    out MapTileSnapshotValue value)
  {
    ArgumentNullException.ThrowIfNull(map);
    if ((uint)x >= (uint)map.MaxWidth || (uint)y >= (uint)map.MaxHeight)
    {
      value = default;
      return false;
    }

    value = map.GetTile(x, y).ToValue();
    return true;
  }
}
