namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HousingScoreEligibilityDecision(
  int Score,
  bool CanSpawn,
  bool IsRejected);
