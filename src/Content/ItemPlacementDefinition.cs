namespace Terraria.Content;

public sealed record ItemPlacementDefinition(
  int? CreateTileTypeId,
  int? CreateWallTypeId,
  int PlaceStyle,
  bool PlaceOnLiquid);
