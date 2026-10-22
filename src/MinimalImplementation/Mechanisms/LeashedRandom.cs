namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Version4 LCG32Random. The state is supplied and returned with each entity step.</summary>
internal sealed class LeashedRandom
{
  public uint State { get; private set; }

  public LeashedRandom(uint state)
  {
    State = state;
  }

  public int Next(int minimum, int maximum)
  {
    if (maximum < minimum)
    {
      throw new ArgumentOutOfRangeException(nameof(maximum));
    }
    State = unchecked((uint)((int)State * -1856014347 + 1));
    return minimum + (int)(((ulong)State * (uint)(maximum - minimum)) >> 32);
  }
}
