using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldClockSystem
{
  public void Tick(WorldClock clock)
  {
    ArgumentNullException.ThrowIfNull(clock);
    clock.Advance();
  }

  public void Tick(WorldClock clock, WorldTimeRateSnapshot timeRate)
  {
    ArgumentNullException.ThrowIfNull(clock);
    if (!timeRate.IsAvailable)
    {
      clock.Advance();
      return;
    }

    clock.Advance(timeRate.Rate);
  }
}
