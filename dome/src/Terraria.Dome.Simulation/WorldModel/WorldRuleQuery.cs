namespace Terraria.Dome.Simulation.WorldModel;

/// <summary>Read-only derived queries for legacy Main world rule properties.</summary>
public static class WorldRuleQuery
{
  public static bool IsJourneyMode(WorldRuleState rules)
  {
    return rules.GameMode == WorldGameMode.Journey;
  }

  public static bool IsExpertMode(WorldRuleState rules)
  {
    return rules.Difficulty >= 1;
  }

  public static bool IsMasterMode(WorldRuleState rules)
  {
    return rules.Difficulty >= 2;
  }

  public static bool IsRainingForever(WorldRuleState rules)
  {
    return rules.RainTimeTicks >= WorldRuleState.EndlessRainThresholdTicks;
  }
}
