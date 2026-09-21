using System;

namespace Terraria.WorldGeneration.Components;

public sealed class LakePlacementHistoryComponent
{
  private readonly int[] _lakeX =
    new int[LakePlacementCapacityDefinition.Capacity];

  public LakePlacementHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  public bool TryAppend(int lakeX)
  {
    if (Count >= LakePlacementCapacityDefinition.EffectiveEntryLimit)
    {
      return false;
    }

    _lakeX[Count] = lakeX;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public LakePlacementHistorySnapshot CreateSnapshot()
  {
    int[] copy = new int[Count];
    Array.Copy(_lakeX, copy, Count);
    return new LakePlacementHistorySnapshot(
      GenerationId,
      LakePlacementCapacityDefinition.Capacity,
      LakePlacementCapacityDefinition.EffectiveEntryLimit,
      Count,
      Array.AsReadOnly(copy));
  }
}
