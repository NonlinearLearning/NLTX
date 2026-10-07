namespace Terraria.Npc;

public readonly record struct NpcSpawnInvasionSelectionInputs(
  bool Invaders,
  int InvasionType,
  int SpawnTileX,
  int SpawnTileY,
  bool HardMode,
  int InvasionSize,
  int InvasionSizeStart);
