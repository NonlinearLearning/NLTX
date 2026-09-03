using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel.Systems;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldProgressionSystem
{
  public const int DefaultSlimeRainWarningDelayTicks =
    WorldSlimeRainWarningPolicy.DefaultDelayTicks;

  public WorldProgressionState Advance(
    WorldClockSnapshot clock,
    WorldProgressionState progression,
    IReadOnlyList<WorldEventStartCommand> eventRequests,
    IReadOnlyList<WorldLanternNightScheduleCommand> lanternNightScheduleRequests,
    IReadOnlyList<WorldInvasionStartCommand> invasionStartRequests,
    IReadOnlyList<WorldInvasionProgressCommand> invasionProgressRequests,
    IReadOnlyList<WorldSlimeRainStartCommand> slimeRainStartRequests,
    IReadOnlyList<WorldSlimeRainStopCommand> slimeRainStopRequests)
  {
    ArgumentNullException.ThrowIfNull(eventRequests);
    ArgumentNullException.ThrowIfNull(lanternNightScheduleRequests);
    ArgumentNullException.ThrowIfNull(invasionStartRequests);
    ArgumentNullException.ThrowIfNull(invasionProgressRequests);
    ArgumentNullException.ThrowIfNull(slimeRainStartRequests);
    ArgumentNullException.ThrowIfNull(slimeRainStopRequests);
    WorldProgressionState delayProgression = AdvanceInvasionDelay(clock, progression);
    WorldProgressionState scheduledProgression = TryScheduleLanternNight(
      delayProgression,
      lanternNightScheduleRequests);
    WorldProgressionState eventProgression;
    if (clock.IsDayTime)
    {
      WorldProgressionState daytimeProgression = scheduledProgression.IsBloodMoon
        ? scheduledProgression.WithBloodMoon(false)
        : scheduledProgression;
      daytimeProgression = daytimeProgression.IsLanternNight
        ? daytimeProgression.WithLanternNight(false)
        : daytimeProgression;
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
      WorldProgressionState nighttimeProgression = scheduledProgression.IsEclipse
        ? scheduledProgression.WithEclipse(false)
        : scheduledProgression;
      if (nighttimeProgression.IsBloodMoon)
      {
        eventProgression = nighttimeProgression;
      }
      else
      {
        WorldProgressionState bloodMoonProgression = TryStartBloodMoon(
          nighttimeProgression,
          eventRequests);
        WorldProgressionState scheduledLanternNightProgression =
          TryStartScheduledLanternNight(bloodMoonProgression);
        eventProgression = scheduledLanternNightProgression.IsLanternNight
          ? scheduledLanternNightProgression
          : TryStartLanternNight(scheduledLanternNightProgression, eventRequests);
      }
    }

    WorldProgressionState invasionProgression = AdvanceInvasion(
      eventProgression,
      invasionStartRequests,
      invasionProgressRequests);
    long lanternNightScheduleSequence = scheduledProgression.LanternNightScheduleSequence;
    WorldProgressionState result = AdvanceSlimeRain(
      clock,
      invasionProgression,
      slimeRainStartRequests,
      slimeRainStopRequests);
    return result.LanternNightScheduleSequence == lanternNightScheduleSequence
      ? result
      : result.WithLanternNightScheduleSequence(lanternNightScheduleSequence);
  }

  private static WorldProgressionState AdvanceInvasionDelay(
    WorldClockSnapshot clock,
    WorldProgressionState progression)
  {
    if (!clock.IsDayTime || clock.TimeOfDay != 0 || progression.InvasionDelayTicks == 0)
    {
      return progression;
    }

    return progression.WithInvasionDelayTicks(
      new WorldInvasionDelaySystem().AdvanceAtDayStart(progression.InvasionDelayTicks));
  }

  private static WorldProgressionState TryScheduleLanternNight(
    WorldProgressionState progression,
    IReadOnlyList<WorldLanternNightScheduleCommand> requests)
  {
    if (progression.IsNextNightLanternNight)
    {
      return progression;
    }

    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return progression
          .WithNextNightLanternNight(true)
          .WithLanternNightScheduleSequence(requests[index].Sequence);
      }
    }

    return progression;
  }

  private static WorldProgressionState TryStartBloodMoon(
    WorldProgressionState progression,
    IReadOnlyList<WorldEventStartCommand> requests)
  {
    if (progression.IsLanternNight || progression.IsMeteorScheduled ||
        progression.InvasionType != 0)
    {
      return progression;
    }

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

  private static WorldProgressionState TryStartLanternNight(
    WorldProgressionState progression,
    IReadOnlyList<WorldEventStartCommand> requests)
  {
    if (progression.IsBloodMoon || progression.IsMeteorScheduled ||
        progression.InvasionType != 0)
    {
      return progression;
    }

    for (int index = 0; index < requests.Count; index++)
    {
      WorldEventStartCommand request = requests[index];
      if (request.IsValid && request.Kind == WorldEventKind.LanternNight)
      {
        return progression.WithLanternNight(true);
      }
    }

    return progression;
  }

  private static WorldProgressionState TryStartScheduledLanternNight(
    WorldProgressionState progression)
  {
    if (!progression.IsNextNightLanternNight || progression.IsLanternNight ||
        progression.IsBloodMoon || progression.InvasionType != 0 ||
        progression.IsMeteorScheduled)
    {
      return progression;
    }

    return progression
      .WithNextNightLanternNight(false)
      .WithLanternNight(true);
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
    IReadOnlyList<WorldSlimeRainStartCommand> startRequests,
    IReadOnlyList<WorldSlimeRainStopCommand> stopRequests)
  {
    WorldProgressionState next;
    if (progression.IsSlimeRaining)
    {
      for (int index = 0; index < stopRequests.Count; index++)
      {
        WorldSlimeRainStopCommand request = stopRequests[index];
        if (request.IsValid)
        {
          next = progression
            .WithSlimeRain(0)
            .WithSlimeRainCooldown(request.CooldownTicks);
          if (request.Announce)
          {
            next = next.WithSlimeRainWarning(DefaultSlimeRainWarningDelayTicks);
          }

          return AdvanceSlimeRainWarning(next);
        }
      }

      int remainingTicks = Math.Max(0, progression.SlimeRainTimeTicks - clock.TicksPerUpdate);
      return AdvanceSlimeRainWarning(progression.WithSlimeRain(remainingTicks));
    }

    if (progression.IsSlimeRainCoolingDown)
    {
      int remainingCooldown = Math.Max(
        0,
        progression.SlimeRainCooldownTicks - clock.TicksPerUpdate);
      return AdvanceSlimeRainWarning(progression.WithSlimeRainCooldown(remainingCooldown));
    }

    for (int index = 0; index < startRequests.Count; index++)
    {
      WorldSlimeRainStartCommand request = startRequests[index];
      if (request.IsValid)
      {
        next = progression.WithSlimeRain(request.DurationTicks);
        if (request.Announce)
        {
          next = next.WithSlimeRainWarning(DefaultSlimeRainWarningDelayTicks);
        }

        return AdvanceSlimeRainWarning(next);
      }
    }

    return AdvanceSlimeRainWarning(progression);
  }

  private static WorldProgressionState AdvanceSlimeRainWarning(
    WorldProgressionState progression)
  {
    if (progression.SlimeRainWarningTicks == 0)
    {
      return progression;
    }

    return progression.WithSlimeRainWarning(progression.SlimeRainWarningTicks - 1);
  }
}
