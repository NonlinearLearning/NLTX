namespace Terraria.Npc;

public readonly record struct NpcSpawnAreaInputs(
  int ScreenWidthPixels,
  int ScreenHeightPixels,
  int PlayerTileX,
  int PlayerTileY,
  int SelectedItemType,
  bool PlayerScope,
  bool DualDungeonsSeed,
  bool ZoneOverworldHeight,
  bool ZoneSkyHeight,
  int MaxTilesX,
  int MaxTilesY);
