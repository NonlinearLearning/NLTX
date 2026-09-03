namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileReframeDecision(
  int CurrentCount,
  int MaximumDepth,
  bool ShouldReframeNeighbors);
