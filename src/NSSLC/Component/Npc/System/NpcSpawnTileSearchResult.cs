namespace Terraria.Npc;

public readonly record struct NpcSpawnTileSearchResult(
  bool Found,
  int TileX,
  int TileY,
  bool XRange,
  bool SkyMob,
  NpcSpawnAreaResult Area);
