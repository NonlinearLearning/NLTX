using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct OasisPlacementHistorySnapshot(
  long GenerationId,
  int Capacity,
  int Count,
  IReadOnlyList<TilePosition> Centers,
  IReadOnlyList<int> Widths);
