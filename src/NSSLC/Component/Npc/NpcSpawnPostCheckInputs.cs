namespace Terraria.Npc;

public readonly record struct NpcSpawnPostCheckInputs(
  int SpawnTileType,
  int SpawnWallType,
  bool ZoneDungeon,
  bool IsDungeonTile,
  bool DualDungeonsSeed,
  bool HasLiquidAtTileAbove,
  bool HasLiquidAtTwoTilesAbove,
  bool TileAboveIsLava,
  bool TileAboveIsShimmer,
  bool TileAboveIsHoney,
  bool BloodMoon,
  bool Eclipse,
  int InvasionType,
  bool PumpkinMoon,
  bool SnowMoon,
  bool SlimeRain);
