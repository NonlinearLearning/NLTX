namespace Terraria.Npc;

public readonly record struct NpcSpawnPerPlayerFlagsPostlude(
  int PlayerTownNpcCount,
  bool PlayerTileIsInWorld,
  bool PlayerTileHasHouseWall,
  int PlayerAfkCounter,
  int AfkTimeNeededForNoWormSpawns,
  bool PlayerTileHasLightWall,
  int PlayerTileWallType,
  bool RemixWorld,
  float PlayerCenterX,
  int MaxTilesX,
  int ArmorSlot0ItemType,
  int ArmorSlot1ItemType,
  int PlayerMaximumLife);
