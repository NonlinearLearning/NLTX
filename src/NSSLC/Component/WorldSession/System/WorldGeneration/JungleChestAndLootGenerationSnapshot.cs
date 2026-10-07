using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct JungleChestAndLootGenerationSnapshot(
  long GenerationId,
  int JungleItemCount,
  bool GennedLivingMahoganyWands,
  int Capacity,
  int Count,
  IReadOnlyList<int> ChestXPositions,
  IReadOnlyList<int> ChestYPositions);
