using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public sealed class PyramidPlacementStateComponent
{
  private readonly int[] _xPositions;
  private readonly int[] _yPositions;

  public PyramidPlacementStateComponent(long generationId, int capacity)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (capacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    GenerationId = generationId;
    Capacity = capacity;
    _xPositions = new int[capacity];
    _yPositions = new int[capacity];
  }

  public long GenerationId { get; }

  public int Capacity { get; }

  public int Count { get; private set; }

  public bool TryAppend(int x, int y)
  {
    if (Count >= Capacity)
    {
      return false;
    }

    _xPositions[Count] = x;
    _yPositions[Count] = y;
    Count++;
    return true;
  }

  internal void ReplaceState(
    int count,
    IReadOnlyList<int> xPositions,
    IReadOnlyList<int> yPositions)
  {
    if (count < 0 || count > Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    ArgumentNullException.ThrowIfNull(xPositions);
    ArgumentNullException.ThrowIfNull(yPositions);
    if (xPositions.Count != count || yPositions.Count != count)
    {
      throw new ArgumentException(
        "Pyramid coordinate lists must contain exactly the used coordinates.");
    }

    for (int index = 0; index < count; index++)
    {
      _xPositions[index] = xPositions[index];
      _yPositions[index] = yPositions[index];
    }
    Count = count;
  }

  public void Clear()
  {
    Count = 0;
  }

  public PyramidPlacementSnapshot CreateSnapshot()
  {
    int[] xCopy = new int[Count];
    int[] yCopy = new int[Count];
    Array.Copy(_xPositions, xCopy, Count);
    Array.Copy(_yPositions, yCopy, Count);
    return new PyramidPlacementSnapshot(
      GenerationId,
      Capacity,
      Count,
      Array.AsReadOnly(xCopy),
      Array.AsReadOnly(yCopy));
  }
}
