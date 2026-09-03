using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldRuntimeTickSystem
{
  public WorldRuntimeSnapshot Advance(WorldRuntimeSnapshot snapshot, int ticksToAdvance)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(ticksToAdvance);
    if (!snapshot.GenerationCompleted || ticksToAdvance == 0)
    {
      return snapshot;
    }

    WorldClock clock = new(
      snapshot.Clock.TickNumber,
      snapshot.Clock.TimeOfDay,
      snapshot.Clock.IsDayTime,
      snapshot.Clock.IsPaused,
      snapshot.Clock.TicksPerUpdate,
      snapshot.Clock.DayLengthTicks,
      snapshot.Clock.NightLengthTicks,
      snapshot.Clock.MoonPhase);
    clock.Advance(ticksToAdvance);
    return new WorldRuntimeSnapshot(
      clock.CreateSnapshot(),
      snapshot.Rules,
      generationCompleted: true,
      generationCompletionTick: snapshot.GenerationCompletionTick);
  }
}
