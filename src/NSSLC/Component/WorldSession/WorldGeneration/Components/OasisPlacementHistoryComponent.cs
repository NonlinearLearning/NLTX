using System;

namespace Terraria.WorldGeneration.Components;

public sealed class OasisPlacementHistoryComponent
{
  private readonly TilePosition[] _centers =
    new TilePosition[OasisPlacementCapacityDefinition.Capacity];
  private readonly int[] _widths =
    new int[OasisPlacementCapacityDefinition.Capacity];

  public OasisPlacementHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  public bool TryAppend(TilePosition center, int width)
  {
    if (Count >= OasisPlacementCapacityDefinition.Capacity || width < 45 || width > 60)
    {
      return false;
    }

    _centers[Count] = center;
    _widths[Count] = width;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public OasisPlacementHistorySnapshot CreateSnapshot()
  {
    TilePosition[] centerCopy = new TilePosition[Count];
    int[] widthCopy = new int[Count];
    Array.Copy(_centers, centerCopy, Count);
    Array.Copy(_widths, widthCopy, Count);
    return new OasisPlacementHistorySnapshot(
      GenerationId,
      OasisPlacementCapacityDefinition.Capacity,
      Count,
      Array.AsReadOnly(centerCopy),
      Array.AsReadOnly(widthCopy));
  }
}
