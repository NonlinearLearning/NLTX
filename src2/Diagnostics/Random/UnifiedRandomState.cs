namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class UnifiedRandomState
{
  private const int Mbig = int.MaxValue;
  private const int Mseed = 161803398;
  private readonly int[] _seedArray = new int[56];
  private uint _inext;

  public UnifiedRandomState(int seed)
  {
    SetSeed(seed);
  }

  public void SetSeed(int seed)
  {
    Array.Clear(_seedArray);
    int subtraction = seed == int.MinValue ? int.MaxValue : Math.Abs(seed);
    int previous = Mseed - subtraction;
    _seedArray[55] = previous;
    int value = 1;
    for (int index = 1; index < 55; index++)
    {
      int target = 21 * index % 55;
      _seedArray[target] = value;
      value = previous - value;
      if (value < 0)
      {
        value += Mbig;
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
          _seedArray[index] += Mbig;
        }
      }
    }

    _inext = 0;
  }

  public int Next()
  {
    return InternalSample();
  }

  public int Next(int maxExclusive)
  {
    if (maxExclusive < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxExclusive));
    }

    return (int)(Sample() * maxExclusive);
  }

  public int Next(int minInclusive, int maxExclusive)
  {
    if (minInclusive > maxExclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(minInclusive));
    }

    long range = (long)maxExclusive - minInclusive;
    if (range > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(maxExclusive));
    }

    return (int)(Sample() * range) + minInclusive;
  }

  public double NextDouble()
  {
    return Sample();
  }

  private double Sample()
  {
    return InternalSample() * 4.656612875245797E-10;
  }

  private int InternalSample()
  {
    uint next = _inext + 1;
    if (next > 55)
    {
      next = 1;
    }

    uint second = next + 21;
    if (second > 55)
    {
      second -= 55;
    }

    int nextIndex = (int)next;
    int secondIndex = (int)second;
    int result = _seedArray[nextIndex] - _seedArray[secondIndex];
    if (result == int.MaxValue)
    {
      result--;
    }

    result = _seedArray[nextIndex] = result + ((result >> 31) & 0x7FFFFFFF);
    _inext = next;
    return result;
  }
}
