using System;

namespace Terraria.Dome.Simulation.WorldModel;

public static class WorldClockQuery
{
  public static bool IsDayTime(WorldClock clock)
  {
    ArgumentNullException.ThrowIfNull(clock);
    return clock.IsDayTime;
  }

  public static bool IsPaused(WorldClock clock)
  {
    ArgumentNullException.ThrowIfNull(clock);
    return clock.IsPaused;
  }

  public static long TickNumber(WorldClock clock)
  {
    ArgumentNullException.ThrowIfNull(clock);
    return clock.TickNumber;
  }

  public static double TimeOfDay(WorldClock clock)
  {
    ArgumentNullException.ThrowIfNull(clock);
    return clock.TimeOfDay;
  }

  public static byte MoonPhase(WorldClock clock)
  {
    ArgumentNullException.ThrowIfNull(clock);
    return clock.MoonPhase;
  }
}
