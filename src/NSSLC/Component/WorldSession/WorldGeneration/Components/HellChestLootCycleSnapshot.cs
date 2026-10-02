using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct HellChestLootCycleSnapshot(
  long GenerationId,
  int CurrentIndex,
  IReadOnlyList<int> ItemSequence);
