namespace Terraria.Npc;

public readonly record struct NpcSpawnScreenPlayerSnapshot(
  bool IsActive,
  float CenterXInPixels,
  float CenterYInPixels,
  bool InsideUnbreakableWalls);
