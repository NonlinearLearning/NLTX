namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileCountCapacityDecision(
  int Count,
  int Maximum,
  bool CanVisit,
  bool IsSaturated);
