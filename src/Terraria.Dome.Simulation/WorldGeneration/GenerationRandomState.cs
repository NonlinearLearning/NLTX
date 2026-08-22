using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct GenerationRandomState(uint Value)
{
  public GenerationRandomState Advance()
  {
    return new GenerationRandomState(Value * 1664525U + 1013904223U);
  }

  public (GenerationRandomState State, int Value) NextExclusive(int maximumExclusive)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumExclusive);
    GenerationRandomState next = Advance();
    return (next, (int)(next.Value % (uint)maximumExclusive));
  }

  public (GenerationRandomState State, int Value) NextInclusive(int minimum, int maximum)
  {
    if (maximum < minimum)
    {
      throw new ArgumentOutOfRangeException(nameof(maximum));
    }

    long range = (long)maximum - minimum + 1;
    if (range > uint.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(maximum));
    }

    GenerationRandomState next = Advance();
    return (next, minimum + (int)(next.Value % (uint)range));
  }
}
