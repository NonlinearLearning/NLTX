using System;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ExtractinatorRandom
{
  private uint _state;

  public ExtractinatorRandom(int seed)
  {
    _state = unchecked((uint)seed);
    if (_state == 0)
    {
      _state = 0x6D2B79F5;
    }
  }

  public int Next(int maximumExclusive)
  {
    if (maximumExclusive <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumExclusive));
    }

    _state = unchecked(_state * 1664525 + 1013904223);
    return (int)(_state % (uint)maximumExclusive);
  }

  public int Next(int minimumInclusive, int maximumExclusive)
  {
    if (minimumInclusive >= maximumExclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumExclusive));
    }

    return minimumInclusive + Next(maximumExclusive - minimumInclusive);
  }
}
