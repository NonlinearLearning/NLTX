using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldMeteorScheduleSystem
{
  public const int MeteorScheduleCutoffTime = 16200;

  public WorldProgressionState Advance(
    WorldClockSnapshot clock,
    WorldProgressionState progression,
    IReadOnlyList<WorldMeteorScheduleCommand> requests)
  {
    ArgumentNullException.ThrowIfNull(progression);
    ArgumentNullException.ThrowIfNull(requests);
    if (clock.IsDayTime && clock.TimeOfDay > MeteorScheduleCutoffTime)
    {
      return progression.IsMeteorScheduled
        ? progression.WithMeteorScheduled(false)
        : progression;
    }

    if (progression.IsMeteorScheduled || !progression.DefeatedEaterOrBrain)
    {
      return progression;
    }

    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return progression.WithMeteorScheduled(true);
      }
    }

    return progression;
  }

  public bool IsReadyForResolution(
    WorldClockSnapshot clock,
    WorldProgressionState progression,
    WorldMeteorImpactCommand impact)
  {
    ArgumentNullException.ThrowIfNull(progression);
    return clock.IsDayTime && clock.TimeOfDay > MeteorScheduleCutoffTime &&
      progression.IsMeteorScheduled && impact.IsValid;
  }
}
