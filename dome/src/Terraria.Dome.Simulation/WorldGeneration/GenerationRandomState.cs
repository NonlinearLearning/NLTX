using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct GenerationRandomState
{
  public GenerationRandomState(uint value, int passVersion = 1, long cursor = 0)
  {
    if (passVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(passVersion));
    }

    if (cursor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursor));
    }

    Value = value;
    PassVersion = passVersion;
    Cursor = cursor;
  }

  public uint Value { get; }
  public int PassVersion { get; }
  public long Cursor { get; }

  public GenerationRandomState Advance()
  {
    return new GenerationRandomState(
      Value * 1664525U + 1013904223U,
      PassVersion,
      Cursor + 1);
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
