using System;

namespace Terraria.Dome.Simulation.Liquid.Systems;

public sealed class LiquidSequenceAllocator
{
  private long _nextSequence;

  public LiquidSequenceAllocator(long firstSequence)
  {
    if (firstSequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(firstSequence));
    }

    _nextSequence = firstSequence;
  }

  public long NextSequence => _nextSequence;

  public bool TryReserve(int count, out long firstSequence)
  {
    firstSequence = 0;
    if (count <= 0 || _nextSequence > long.MaxValue - count)
    {
      return false;
    }

    firstSequence = _nextSequence;
    _nextSequence += count;
    return true;
  }
}
