namespace Terraria.Npc;

public readonly record struct NpcSpawnChosenTileFlagsResult(
  bool NoWorms,
  bool WaterTile,
  bool NearGranite,
  bool NearMarble,
  bool UnderGround,
  bool SpawnSpider,
  bool SpawnUndergroundDesert,
  bool IsSpawningInWindDirection,
  bool SurfaceSpawn,
  bool DeeperThanRockLayer,
  bool IsOcean,
  bool IsBeach,
  bool Raining,
  bool DayTime);
