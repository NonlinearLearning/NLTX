using System;

namespace Terraria.WorldGeneration.Components;

public sealed class MountainCaveHistoryComponent
{
  public const int Capacity = 30;

  private readonly int[] _xOrigins = new int[Capacity];
  private readonly int[] _yOrigins = new int[Capacity];

  public MountainCaveHistoryComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public int Count { get; private set; }

  internal bool TryAppend(int x, int y)
  {
    if (Count >= Capacity)
    {
      return false;
    }

    _xOrigins[Count] = x;
    _yOrigins[Count] = y;
    Count++;
    return true;
  }

  public void Clear()
  {
    Count = 0;
  }

  public MountainCaveHistorySnapshot CreateSnapshot()
  {
    int[] xCopy = new int[Count];
    int[] yCopy = new int[Count];
    Array.Copy(_xOrigins, xCopy, Count);
    Array.Copy(_yOrigins, yCopy, Count);
    return new MountainCaveHistorySnapshot(
      GenerationId,
      Capacity,
      Count,
      Array.AsReadOnly(xCopy),
      Array.AsReadOnly(yCopy));
  }
}
