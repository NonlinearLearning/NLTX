using System.Collections.Generic;

namespace Terraria.Npc;

public readonly record struct NpcSpawnScreenExclusionInputs(
  int ScreenWidthPixels,
  int ScreenHeightPixels,
  int SafeRangeX,
  int SafeRangeY,
  bool DualDungeonsSeed,
  IReadOnlyList<NpcSpawnScreenPlayerSnapshot> Players);
