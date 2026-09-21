using System;

namespace Terraria.WorldGeneration.Components;

public sealed class SurfaceOrePatchHistoryComponent
{
  public const int Capacity = SurfaceOrePatchHistoryDefinition.Capacity;

  public const int EffectiveEntryLimit =
    SurfaceOrePatchHistoryDefinition.EffectiveAppendCapacity;

  private readonly int[] _patchX =
    new int[SurfaceOrePatchHistoryDefinition.Capacity];

  public SurfaceOrePatchHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  public bool TryAppend(int x)
  {
    if (Count >= SurfaceOrePatchHistoryDefinition.EffectiveAppendCapacity)
    {
      return false;
    }

    _patchX[Count] = x;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public SurfaceOrePatchHistorySnapshot CreateSnapshot()
  {
    int[] copy = new int[Count];
    Array.Copy(_patchX, copy, Count);
    return new SurfaceOrePatchHistorySnapshot(
      GenerationId,
      SurfaceOrePatchHistoryDefinition.Capacity,
      SurfaceOrePatchHistoryDefinition.EffectiveAppendCapacity,
      Count,
      Array.AsReadOnly(copy));
  }
}
