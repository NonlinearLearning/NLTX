using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct SurfaceOrePatchHistorySnapshot(
  long GenerationId,
  int Capacity,
  int EffectiveAppendCapacity,
  int Count,
  IReadOnlyList<int> PatchX)
{
  public int EffectiveEntryLimit => EffectiveAppendCapacity;

  public IReadOnlyList<int> PatchXPositions => PatchX;
}
