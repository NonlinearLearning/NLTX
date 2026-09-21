using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct MushroomBiomeAnchorStateSnapshot(
  long GenerationId,
  int Capacity,
  int Count,
  IReadOnlyList<TilePosition> Positions);
