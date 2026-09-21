namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileCosmeticNeighborResult(
  int CanonicalTileType,
  int Up,
  int Down,
  int Left,
  int Right,
  int UpLeft,
  int UpRight,
  int DownLeft,
  int DownRight);
