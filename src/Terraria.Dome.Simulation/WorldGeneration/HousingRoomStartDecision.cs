namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HousingRoomStartDecision(
  HousingRoomStartRejectionReason RejectionReason,
  bool CanStart);
