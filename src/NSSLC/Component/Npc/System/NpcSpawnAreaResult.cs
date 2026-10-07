namespace Terraria.Npc;

public readonly record struct NpcSpawnAreaResult(
  NpcSpawnTileRectangle SpawnArea,
  NpcSpawnTileRectangle SafeArea,
  int SafeRangeX,
  int SafeRangeY);
