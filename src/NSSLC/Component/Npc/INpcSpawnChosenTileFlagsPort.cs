namespace Terraria.Npc;

public interface INpcSpawnChosenTileFlagsPort : INpcSpawnRateRandomPort
{
  int Next(int minimumInclusive, int maximumExclusive);

  NpcSpawnChosenTileWorldInputs CaptureChosenTileWorldInputs(
    int spawnTileX,
    int spawnTileY,
    int spawnTileType);

  NpcSpawnTileFacts ReadTile(int tileX, int tileY);

  bool AllowsUndergroundDesertEnemiesToSpawn(int wallType);

  bool IsOceanDepths(int tileX, int tileY);
}
