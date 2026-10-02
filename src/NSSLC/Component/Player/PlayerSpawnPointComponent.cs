namespace Terraria.Player;

public sealed class PlayerSpawnPointComponent
{
  public TileCoordinate? PersonalSpawnTile { get; set; }

  // Compatibility representation retained until the Version4 mapping is closed.
  public int SpawnX { get; set; } = -1;

  public int SpawnY { get; set; } = -1;

  public WorldPosition? ReturnOriginalUsePosition { get; set; }

  public WorldPosition? ReturnHomePosition { get; set; }

  public bool HasSpawnCoordinates => SpawnX >= 0 && SpawnY >= 0;

  public bool HasReturnRoute =>
    ReturnOriginalUsePosition.HasValue && ReturnHomePosition.HasValue;
}
