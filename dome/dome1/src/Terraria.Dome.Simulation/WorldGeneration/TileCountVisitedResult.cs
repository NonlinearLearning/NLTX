namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileCountVisitedResult(
  TileCountVisitedSnapshot State,
  bool WasAlreadyVisited);
