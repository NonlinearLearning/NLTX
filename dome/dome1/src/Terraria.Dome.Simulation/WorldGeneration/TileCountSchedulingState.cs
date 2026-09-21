namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileCountSchedulingState(int UpdateCounter, int ColumnX)
{
  public static TileCountSchedulingState Initial { get; } = new(0, 0);
}
