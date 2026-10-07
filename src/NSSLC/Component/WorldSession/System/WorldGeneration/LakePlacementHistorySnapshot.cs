using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct LakePlacementHistorySnapshot(
  long GenerationId,
  int Capacity,
  int EffectiveEntryLimit,
  int Count,
  IReadOnlyList<int> LakeX);
