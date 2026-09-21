using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public sealed class OceanCaveTreasureStateComponent
{
  public const int Capacity = 2;

  private readonly TilePosition[] _treasure = new TilePosition[Capacity];

  public OceanCaveTreasureStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  public bool TryAppend(TilePosition position)
  {
    if (Count >= Capacity)
    {
      return false;
    }

    _treasure[Count] = position;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public OceanCaveTreasureSnapshot CreateSnapshot()
  {
    TilePosition[] copy = new TilePosition[Count];
    Array.Copy(_treasure, copy, Count);
    return new OceanCaveTreasureSnapshot(
      GenerationId,
      Count,
      Array.AsReadOnly(copy));
  }
}
