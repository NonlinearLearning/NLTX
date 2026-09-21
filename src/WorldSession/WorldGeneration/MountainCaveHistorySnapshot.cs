using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct MountainCaveHistorySnapshot(
  long GenerationId,
  int Capacity,
  int Count,
  IReadOnlyList<int> XOrigins,
  IReadOnlyList<int> YOrigins);
