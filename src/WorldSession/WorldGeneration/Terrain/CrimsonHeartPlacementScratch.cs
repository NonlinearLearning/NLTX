using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Terrain;

public sealed class CrimsonHeartPlacementScratch
{
  public const int Capacity = 100;

  private readonly TilePosition[] _positions = new TilePosition[Capacity];

  public int Count { get; private set; }

  public bool TryAppend(TilePosition position)
  {
    if (Count >= Capacity)
    {
      return false;
    }

    _positions[Count] = position;
    Count++;
    return true;
  }

  public TilePosition GetPosition(int index)
  {
    if ((uint)index >= Count)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    return _positions[index];
  }

  public void Clear()
  {
    Array.Clear(_positions);
    Count = 0;
  }
}
