namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DirtCountVisitDecision(
  DirtCountRejectionReason RejectionReason,
  bool CanCount);
