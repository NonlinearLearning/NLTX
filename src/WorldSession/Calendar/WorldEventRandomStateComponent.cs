using System;

namespace Terraria.WorldSession.Calendar;

public sealed class WorldEventRandomStateComponent
{
  public WorldEventRandomStateComponent(
    uint state,
    ulong worldSeed,
    int streamVersion,
    long consumedDrawCount = 0)
  {
    State = state;
    WorldSeed = worldSeed;
    StreamVersion = streamVersion;
    ConsumedDrawCount = consumedDrawCount;
    Validate();
  }

  public uint State;
  public ulong WorldSeed;
  public int StreamVersion;
  public long ConsumedDrawCount;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(StreamVersion);
    ArgumentOutOfRangeException.ThrowIfNegative(ConsumedDrawCount);
  }
}
