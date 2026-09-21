namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class FastRandomValue
{
  private const ulong RandomMultiplier = 25214903917;
  private const ulong RandomAdd = 11;
  private const ulong RandomMask = 281474976710655;

  public FastRandomValue(ulong seed)
  {
    Seed = seed;
  }

  public ulong Seed { get; private set; }

  public float NextFloat()
  {
    return NextBits(24) * 5.9604645E-08f;
  }

  public int Next(int maxExclusive)
  {
    if (maxExclusive <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxExclusive));
    }

    return (int)(NextBits(31) * (long)maxExclusive >> 31);
  }

  public int Next(int minInclusive, int maxExclusive)
  {
    if (minInclusive > maxExclusive)
    {
      throw new ArgumentOutOfRangeException(nameof(minInclusive));
    }

    if (minInclusive == maxExclusive)
    {
      return minInclusive;
    }

    return Next(maxExclusive - minInclusive) + minInclusive;
  }

  private int NextBits(int bits)
  {
    Seed = (Seed * RandomMultiplier + RandomAdd) & RandomMask;
    return (int)(Seed >> (48 - bits));
  }
}
