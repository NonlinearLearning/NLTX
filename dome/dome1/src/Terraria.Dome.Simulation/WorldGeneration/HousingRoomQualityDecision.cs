namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HousingRoomQualityDecision(
  int Score,
  bool IsEligible,
  HousingRoomQualityFailureReason FailureReason);
