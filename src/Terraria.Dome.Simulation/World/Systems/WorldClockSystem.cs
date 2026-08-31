using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldClockSystem
{
  public WorldClockTransition? Tick(WorldClock clock)
  {
    ArgumentNullException.ThrowIfNull(clock);
    return Advance(clock, clock.TicksPerUpdate);
  }

  public WorldClockTransition? Tick(WorldClock clock, WorldTimeRateSnapshot timeRate)
  {
    ArgumentNullException.ThrowIfNull(clock);
    if (!timeRate.IsAvailable)
    {
      return Advance(clock, clock.TicksPerUpdate);
    }

    return Advance(clock, timeRate.Rate);
  }

  private static WorldClockTransition? Advance(WorldClock clock, int ticks)
  {
    bool wasDayTime = clock.IsDayTime;
    clock.Advance(ticks);
    if (wasDayTime == clock.IsDayTime)
    {
      return null;
    }

    WorldClockTransitionKind kind = clock.IsDayTime
      ? WorldClockTransitionKind.Dawn
      : WorldClockTransitionKind.Dusk;
    return new WorldClockTransition(kind, clock.TickNumber, clock.TimeOfDay, clock.MoonPhase);
  }
}
