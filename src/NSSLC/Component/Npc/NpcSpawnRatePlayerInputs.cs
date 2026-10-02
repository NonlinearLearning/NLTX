namespace Terraria.Npc;

public readonly record struct NpcSpawnRatePlayerInputs(
  float PositionY,
  float CenterY,
  float NearbyActiveNpcSlots,
  bool ZoneUndergroundDesert,
  bool IsInvisible,
  bool IsCalmed,
  bool HasSunflower,
  bool HasAnglerSetSpawnReduction,
  bool EnemySpawnsEnabled,
  bool HasNearbyFairy);
