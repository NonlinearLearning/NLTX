namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HousingWallSafetyDecision(
  HousingWallSafetyRejectionReason RejectionReason,
  bool IsSafe);
