using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldTimeRateSnapshot
{
  private WorldTimeRateSnapshot(bool isAvailable, int rate)
  {
    if (rate < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rate));
    }

    if (!isAvailable && rate != 0)
    {
      throw new ArgumentException("An unavailable time rate cannot carry a value.", nameof(rate));
    }

    IsAvailable = isAvailable;
    Rate = rate;
  }

  public WorldTimeRateSnapshot(int rate)
    : this(true, rate)
  {
  }

  public bool IsAvailable { get; }
  public int Rate { get; }

  public static WorldTimeRateSnapshot Unavailable => new(false, 0);

  public static WorldTimeRateSnapshot FromPersisted(bool isAvailable, int rate)
  {
    return new WorldTimeRateSnapshot(isAvailable, rate);
  }
}
