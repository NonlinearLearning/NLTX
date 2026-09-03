namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemPlacementDefinition(
  short TileType = -1,
  short WallType = -1,
  int PlaceStyle = 0,
  int TileBoost = 0,
  bool CartTrack = false)
{
  public const short CartTrackTileType = 314;

  public int UseTimeTicks => WallType >= 0 ? ItemDefinition.WallPlacementUseTime : 0;
}
