namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HousingBlockingTileDecision(
  HousingBlockingTileReason Reason,
  bool ShouldStop);
