using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldEventRandomState(uint Value)
{
  public (WorldEventRandomState State, int Value) NextExclusive(int maximumExclusive)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumExclusive);
    WorldEventRandomState next = Advance();
    return (next, (int)(next.Value % (uint)maximumExclusive));
  }

  public (WorldEventRandomState State, int Value) NextInclusive(int minimum, int maximum)
  {
    if (maximum < minimum)
    {
      throw new ArgumentOutOfRangeException(nameof(maximum));
    }

    ulong range = (ulong)((long)maximum - minimum + 1);
    WorldEventRandomState next = Advance();
    return (next, (int)((long)minimum + (long)(next.Value % range)));
  }

  private WorldEventRandomState Advance()
  {
    return new WorldEventRandomState(Value * 1664525U + 1013904223U);
  }
}
