using System;

namespace Terraria.WorldSession.Components;

public readonly record struct WorldClockSnapshotValue(
  bool DayTime,
  double Time,
  short SunModY,
  short MoonModY,
  int MoonPhase,
  long ClockRevision)
{
  public static WorldClockSnapshotValue Capture(WorldTimeWeatherState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return new WorldClockSnapshotValue(
      state.DayTime,
      state.Time,
      state.SunModY,
      state.MoonModY,
      state.MoonPhase,
      state.ClockRevision);
  }
}
