using System;

namespace Terraria.WorldGeneration.Components;

public sealed class UndergroundDesertLarvaPlacementComponent
{
  public const int Capacity = 100;

  private readonly int[] _larvaX = new int[Capacity];

  private readonly int[] _larvaY = new int[Capacity];

  public UndergroundDesertLarvaPlacementComponent(long generationId)
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

    _larvaX[Count] = position.X;
    _larvaY[Count] = position.Y;
    Count++;
    return true;
  }

  public void Clear()
  {
    Array.Clear(_larvaX, 0, _larvaX.Length);
    Array.Clear(_larvaY, 0, _larvaY.Length);
    Count = 0;
  }

  public UndergroundDesertLarvaPlacementSnapshot CreateSnapshot()
  {
    TilePosition[] copy = new TilePosition[Count];
    for (int index = 0; index < Count; index++)
    {
      copy[index] = new TilePosition(_larvaX[index], _larvaY[index]);
    }

    return new UndergroundDesertLarvaPlacementSnapshot(
      GenerationId,
      Count,
      Array.AsReadOnly(copy));
  }
}
