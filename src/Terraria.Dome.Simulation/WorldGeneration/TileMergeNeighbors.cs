namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileMergeNeighbors(
  int Up,
  int Down,
  int Left,
  int Right,
  int UpLeft = -1,
  int UpRight = -1,
  int DownLeft = -1,
  int DownRight = -1);
