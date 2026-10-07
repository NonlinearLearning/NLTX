namespace Terraria.Npc;

public interface INpcSpawnTileSearchPort : INpcSpawnRateRandomPort
{
  int Next(int minimumInclusive, int maximumExclusive);

  NpcSpawnAreaInputs CaptureSpawnAreaInputs(int playerIndex);

  bool IsActiveSolidTile(int tileX, int tileY);

  bool IsHouseWallTile(int tileX, int tileY);

  NpcSpawnTileSpaceFacts CaptureTileSpaceFacts(int tileX, int tileY);
}
