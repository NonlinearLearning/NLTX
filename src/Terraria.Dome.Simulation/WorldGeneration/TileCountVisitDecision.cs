namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileCountVisitDecision(
  TileCountVisitRejectionReason RejectionReason,
  bool CountsLava);
