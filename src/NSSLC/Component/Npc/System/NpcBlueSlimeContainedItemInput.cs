namespace Terraria.Npc;

public readonly record struct NpcBlueSlimeContainedItemInput(
  bool IsBallooned,
  bool LowTiles,
  int MoonPhase,
  bool InRockLayer,
  bool HardMode,
  int NetMode);
