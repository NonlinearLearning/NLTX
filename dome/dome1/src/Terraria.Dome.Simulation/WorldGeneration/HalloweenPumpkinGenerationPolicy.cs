namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HalloweenPumpkinGenerationPolicy
{
  public static HalloweenPumpkinGenerationDecision Evaluate(
    bool halloweenGenerationEnabled,
    bool endlessHalloweenEnabled)
  {
    return new HalloweenPumpkinGenerationDecision(
      halloweenGenerationEnabled,
      endlessHalloweenEnabled,
      halloweenGenerationEnabled || endlessHalloweenEnabled);
  }
}
