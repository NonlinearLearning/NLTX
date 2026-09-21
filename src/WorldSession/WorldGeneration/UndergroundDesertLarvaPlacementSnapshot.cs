using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct UndergroundDesertLarvaPlacementSnapshot(
  long GenerationId,
  int Count,
  IReadOnlyList<TilePosition> Positions);
