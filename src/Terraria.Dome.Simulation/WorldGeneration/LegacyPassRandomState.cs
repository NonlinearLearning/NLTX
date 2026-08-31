using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class LegacyPassRandomState
{
  private const int MaximumSample = int.MaxValue;
  private const int SeedBase = 161803398;
  private readonly int[] _seedArray = new int[56];
  private uint _index;

  public LegacyPassRandomState(int seed)
  {
    int normalizedSeed = seed == int.MinValue ? int.MaxValue : Math.Abs(seed);
    int previous = SeedBase - normalizedSeed;
    _seedArray[55] = previous;
    int next = 1;
    for (int index = 1; index < 55; index++)
    {
      int target = 21 * index % 55;
      _seedArray[target] = next;
      next = previous - next;
      if (next < 0)
      {
        next += MaximumSample;
      }

      previous = _seedArray[target];
    }

    for (int pass = 1; pass < 5; pass++)
    {
      for (int index = 1; index < 56; index++)
      {
        _seedArray[index] -= _seedArray[1 + (index + 30) % 55];
        if (_seedArray[index] < 0)
        {
          _seedArray[index] += MaximumSample;
        }
      }
    }
  }

  public long SampleCount { get; private set; }

  public int Next(int minimumInclusive, int maximumExclusive)
  {
    if (minimumInclusive > maximumExclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(minimumInclusive));
    }

    long range = (long)maximumExclusive - minimumInclusive;
    if (range == 0)
    {
      return minimumInclusive;
    }

    return (int)(InternalSample() * (double)range / MaximumSample) + minimumInclusive;
  }

  public int Next(int maximumExclusive)
  {
    if (maximumExclusive < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumExclusive));
    }

    return Next(0, maximumExclusive);
  }

  public double NextDouble()
  {
    return InternalSample() * (1.0 / MaximumSample);
  }

  private int InternalSample()
  {
    if (SampleCount == long.MaxValue)
    {
      throw new InvalidOperationException("Legacy pass random sample count was exhausted.");
    }

    uint current = _index + 1;
    if (current > 55)
    {
      current = 1;
    }

    uint paired = current + 21;
    if (paired > 55)
    {
      paired -= 55;
    }

    int sample = _seedArray[current] - _seedArray[paired];
    if (sample == MaximumSample)
    {
      sample--;
    }

    sample += (sample >> 31) & MaximumSample;
    _seedArray[current] = sample;
    _index = current;
    SampleCount++;
    return sample;
  }
}
