namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HousingAlternateSpotDecision(
  bool HasAlternateSpot,
  bool IsAlreadyTrying,
  bool CanTryAlternateSpot);
