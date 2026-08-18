namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemPlacementDefinition(
  short TileType = -1,
  short WallType = -1,
  int PlaceStyle = 0,
  int TileBoost = 0);
