namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapTileSnapshotSystem
{
  public void Capture(
    WorldMapSnapshotComponent map,
    int x,
    int y,
    MapTileSnapshotValue value)
  {
    ArgumentNullException.ThrowIfNull(map);
    map.SetTile(x, y, value);
  }

  public void Clear(WorldMapSnapshotComponent map)
  {
    ArgumentNullException.ThrowIfNull(map);
    map.Clear();
  }
}
