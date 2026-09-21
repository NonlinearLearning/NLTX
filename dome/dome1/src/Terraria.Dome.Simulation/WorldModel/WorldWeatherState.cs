using System;

namespace Terraria.Dome.Simulation.WorldModel;

public static class WorldWeatherState
{
  public static bool IsRaining(WorldRuleState rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    return rules.IsRaining;
  }

  public static int RainTime(WorldRuleState rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    return rules.RainTimeTicks;
  }

  public static float MaximumRainStrength(WorldRuleState rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    return rules.MaximumRainStrength;
  }

  public static float WindSpeedCurrent(WorldRuleState rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    return rules.WindSpeedCurrent;
  }

  public static float WindSpeedTarget(WorldRuleState rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    return rules.WindSpeedTarget;
  }

  public static bool IsRainingForever(WorldRuleState rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    return rules.RainTimeTicks >= WorldRuleState.EndlessRainThresholdTicks;
  }
}
