using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldWeatherSystem
{
  public WorldRuleState Advance(
    WorldClockSnapshot clock,
    WorldRuleState rules,
    IReadOnlyList<WorldRainStartCommand> rainRequests)
  {
    ArgumentNullException.ThrowIfNull(rules);
    ArgumentNullException.ThrowIfNull(rainRequests);
    if (rules.IsRaining)
    {
      return rules.AdvanceRain(clock.TicksPerUpdate);
    }

    for (int index = 0; index < rainRequests.Count; index++)
    {
      WorldRainStartCommand request = rainRequests[index];
      if (request.IsValid)
      {
        return rules.WithRain(request.DurationTicks, request.Strength);
      }
    }

    return rules;
  }
}
