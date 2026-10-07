using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct OceanCaveTreasureSnapshot(
  long GenerationId,
  int Count,
  IReadOnlyList<TilePosition> Positions);
