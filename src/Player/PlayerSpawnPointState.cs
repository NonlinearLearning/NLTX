namespace Terraria.Player;

public sealed class PlayerSpawnPointState
{
  public TileCoordinate? PersonalSpawnTile { get; set; }

  public WorldPosition? ReturnOriginalUsePosition { get; set; }

  public WorldPosition? ReturnHomePosition { get; set; }

  public bool HasPersonalSpawn => PersonalSpawnTile.HasValue;

  public bool HasReturnRoute => ReturnOriginalUsePosition.HasValue && ReturnHomePosition.HasValue;
}
