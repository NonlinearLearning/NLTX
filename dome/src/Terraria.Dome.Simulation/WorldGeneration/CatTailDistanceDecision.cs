namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct CatTailDistanceDecision(
  int Distance,
  int MaximumPlacementDistance,
  int MinimumPlacementDistance,
  bool IsValidPlacementDistance,
  bool ExceedsCleanupDistance);
