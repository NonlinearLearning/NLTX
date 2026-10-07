namespace Terraria.Npc;

public readonly record struct NpcSpawnSkyMobSelectionInputs(
  bool IsSkyMob,
  int SpawnTileX,
  int SpawnTileY,
  int MaxTilesX,
  bool SkyBehindPlayer,
  bool ZoneWaterCandle,
  bool Invaders,
  int InvasionType,
  bool HardMode,
  bool DownedGolemBoss,
  bool DownedMartians,
  bool NoWorms,
  bool UnlockedSlimePurpleSpawn);
