namespace Terraria.Npc;

public readonly record struct NpcSpawnInvasionInputs(
  int InvasionType,
  int InvasionDelay,
  int InvasionSize,
  float PlayerPositionX,
  float PlayerPositionY,
  double WorldSurface,
  int ScreenHeightPixels,
  int SpawnTileY,
  double InvasionX,
  int MaxTilesX,
  int MaxNpcSlots);
