using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct DungeonLayoutSnapshot(
  long GenerationId,
  int TLeft,
  int TRight,
  int TTop,
  int TBottom,
  int TRooms,
  int LAltarX,
  int LAltarY,
  IReadOnlyList<DungeonRecordSnapshot> Records,
  int CurrentDungeon);
