namespace Terraria.WorldSession.Queries;

public sealed class FrameRandomSeedProjection
{
  public uint Current { get; private set; }

  public void Advance(uint seed)
  {
    Current = seed;
  }
}
