namespace Terraria.NonAuthoritative.Diagnostics;

public static class RandomStreamFactory
{
  public static FastRandomValue CreateFast(ulong seed)
  {
    return new FastRandomValue(seed);
  }

  public static Lcg32RandomState CreateLcg(uint seed)
  {
    return new Lcg32RandomState(seed);
  }

  public static UnifiedRandomState CreateUnified(int seed)
  {
    return new UnifiedRandomState(seed);
  }
}
