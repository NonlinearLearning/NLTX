using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct FloatingIslandPlacementSnapshot(
  long GenerationId,
  int SkyLakes,
  int SkyIslandHouseCount,
  int Capacity,
  int Count,
  IReadOnlyList<FloatingIslandHouseSnapshot> Houses);
