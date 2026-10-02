using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct SurfaceTunnelHistorySnapshot(
  long GenerationId,
  int Capacity,
  int EffectiveAppendCapacity,
  int Count,
  IReadOnlyList<int> CenterX)
{
  public int EffectiveEntryLimit => EffectiveAppendCapacity;

  public IReadOnlyList<int> CenterXPositions => CenterX;
}
