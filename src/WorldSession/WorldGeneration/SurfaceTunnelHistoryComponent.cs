using System;

namespace Terraria.WorldGeneration.Components;

public sealed class SurfaceTunnelHistoryComponent
{
  public const int Capacity = SurfaceTunnelHistoryDefinition.Capacity;

  public const int EffectiveEntryLimit =
    SurfaceTunnelHistoryDefinition.EffectiveAppendCapacity;

  private readonly int[] _centerX =
    new int[SurfaceTunnelHistoryDefinition.Capacity];

  public SurfaceTunnelHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  public bool TryAppend(int centerX)
  {
    if (Count >= SurfaceTunnelHistoryDefinition.EffectiveAppendCapacity)
    {
      return false;
    }

    _centerX[Count] = centerX;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public SurfaceTunnelHistorySnapshot CreateSnapshot()
  {
    int[] copy = new int[Count];
    Array.Copy(_centerX, copy, Count);
    return new SurfaceTunnelHistorySnapshot(
      GenerationId,
      SurfaceTunnelHistoryDefinition.Capacity,
      SurfaceTunnelHistoryDefinition.EffectiveAppendCapacity,
      Count,
      Array.AsReadOnly(copy));
  }
}
