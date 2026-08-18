using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldProgressionSystem
{
  public WorldProgressionState Advance(
    WorldClockSnapshot clock,
    WorldProgressionState progression,
    IReadOnlyList<WorldEventStartCommand> eventRequests,
    IReadOnlyList<WorldInvasionStartCommand> invasionStartRequests,
    IReadOnlyList<WorldInvasionProgressCommand> invasionProgressRequests,
    IReadOnlyList<WorldSlimeRainStartCommand> slimeRainStartRequests)
  {
    ArgumentNullException.ThrowIfNull(eventRequests);
    ArgumentNullException.ThrowIfNull(invasionStartRequests);
    ArgumentNullException.ThrowIfNull(invasionProgressRequests);
    ArgumentNullException.ThrowIfNull(slimeRainStartRequests);
    WorldProgressionState eventProgression;
    if (clock.IsDayTime)
    {
      WorldProgressionState daytimeProgression = progression.IsBloodMoon
        ? progression.WithBloodMoon(false)
        : progression;
      if (daytimeProgression.IsEclipse)
      {
        eventProgression = daytimeProgression;
      }
      else
      {
        eventProgression = TryStartEclipse(daytimeProgression, eventRequests);
      }
    }
    else
    {
      WorldProgressionState nighttimeProgression = progression.IsEclipse
        ? progression.WithEclipse(false)
        : progression;
      if (nighttimeProgression.IsBloodMoon)
      {
        eventProgression = nighttimeProgression;
      }
      else
      {
        eventProgression = TryStartBloodMoon(nighttimeProgression, eventRequests);
      }
    }

    WorldProgressionState invasionProgression = AdvanceInvasion(
      eventProgression,
      invasionStartRequests,
      invasionProgressRequests);
    return AdvanceSlimeRain(clock, invasionProgression, slimeRainStartRequests);
  }

  private static WorldProgressionState TryStartBloodMoon(
    WorldProgressionState progression,
    IReadOnlyList<WorldEventStartCommand> requests)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      WorldEventStartCommand request = requests[index];
      if (request.IsValid && request.Kind == WorldEventKind.BloodMoon)
      {
        return progression.WithBloodMoon(true);
      }
    }

    return progression;
  }

  private static WorldProgressionState TryStartEclipse(
    WorldProgressionState progression,
    IReadOnlyList<WorldEventStartCommand> requests)
  {
    if (!progression.IsHardMode || !progression.DefeatedMechanicalBoss)
    {
      return progression;
    }

    for (int index = 0; index < requests.Count; index++)
    {
      WorldEventStartCommand request = requests[index];
      if (request.IsValid && request.Kind == WorldEventKind.Eclipse)
      {
        return progression.WithEclipse(true);
      }
    }

    return progression;
  }

  private static WorldProgressionState AdvanceInvasion(
    WorldProgressionState progression,
    IReadOnlyList<WorldInvasionStartCommand> startRequests,
    IReadOnlyList<WorldInvasionProgressCommand> progressRequests)
  {
    WorldProgressionState normalized = progression.InvasionType == 0 || progression.InvasionSize == 0
      ? progression.WithInvasion(0, 0)
      : progression;
    if (normalized.InvasionType == 0)
    {
      for (int index = 0; index < startRequests.Count; index++)
      {
        WorldInvasionStartCommand request = startRequests[index];
        if (request.IsValid)
        {
          return normalized.WithInvasion(request.Type, request.Size);
        }
      }

      return normalized;
    }

    int remainingSize = normalized.InvasionSize;
    for (int index = 0; index < progressRequests.Count; index++)
    {
      WorldInvasionProgressCommand request = progressRequests[index];
      if (request.IsValid)
      {
        remainingSize = Math.Max(0, remainingSize - request.Amount);
      }
    }

    return remainingSize == 0
      ? normalized.WithInvasion(0, 0)
      : normalized.WithInvasion(normalized.InvasionType, remainingSize);
  }

  private static WorldProgressionState AdvanceSlimeRain(
    WorldClockSnapshot clock,
    WorldProgressionState progression,
    IReadOnlyList<WorldSlimeRainStartCommand> requests)
  {
    if (progression.IsSlimeRaining)
    {
      int remainingTicks = Math.Max(0, progression.SlimeRainTimeTicks - clock.TicksPerUpdate);
      return progression.WithSlimeRain(remainingTicks);
    }

    for (int index = 0; index < requests.Count; index++)
    {
      WorldSlimeRainStartCommand request = requests[index];
      if (request.IsValid)
      {
        return progression.WithSlimeRain(request.DurationTicks);
      }
    }

    return progression;
  }
}
