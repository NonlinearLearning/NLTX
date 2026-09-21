namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HalloweenPumpkinGenerationDecision(
  bool HalloweenGenerationEnabled,
  bool EndlessHalloweenEnabled,
  bool ShouldGeneratePumpkins);
