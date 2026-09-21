namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct GrassSpreadRecursionState(int Depth)
{
  public static GrassSpreadRecursionState Initial { get; } = new(0);
}
