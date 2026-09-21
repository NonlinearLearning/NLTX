namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class Lcg32RandomState
{
  public Lcg32RandomState(uint seed)
  {
    State = seed;
  }

  public uint State { get; private set; }

  public void Advance()
  {
    State = unchecked((uint)((int)State * -1856014347 + 1));
  }

  public double NextDouble()
  {
    Advance();
    return State / 4294967296.0;
  }

  public float NextFloat()
  {
    return (float)NextDouble();
  }
}
