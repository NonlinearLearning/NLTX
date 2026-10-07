namespace Terraria.Npc;

public readonly record struct NpcSpawnChosenTileWorldInputs(
  bool DontStarveWorld,
  float WindSpeedTarget,
  int OceanDistance,
  int BeachDistance,
  bool SpawnTileIsSand);
