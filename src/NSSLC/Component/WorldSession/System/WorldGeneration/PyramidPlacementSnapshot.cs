using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct PyramidPlacementSnapshot(
  long GenerationId,
  int Capacity,
  int Count,
  IReadOnlyList<int> XPositions,
  IReadOnlyList<int> YPositions);
