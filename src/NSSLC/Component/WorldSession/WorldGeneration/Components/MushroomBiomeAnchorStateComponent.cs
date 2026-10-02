using System;

namespace Terraria.WorldGeneration.Components;

public sealed class MushroomBiomeAnchorStateComponent
{
  private readonly TilePosition[] _positions =
    new TilePosition[MushroomBiomeCapacityDefinition.Capacity];

  public MushroomBiomeAnchorStateComponent(long generationId)
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
    if (Count >= MushroomBiomeCapacityDefinition.Capacity)
    {
      return false;
    }

    _positions[Count] = position;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public MushroomBiomeAnchorStateSnapshot CreateSnapshot()
  {
    TilePosition[] copy = new TilePosition[Count];
    Array.Copy(_positions, copy, Count);
    return new MushroomBiomeAnchorStateSnapshot(
      GenerationId,
      MushroomBiomeCapacityDefinition.Capacity,
      Count,
      Array.AsReadOnly(copy));
  }
}
