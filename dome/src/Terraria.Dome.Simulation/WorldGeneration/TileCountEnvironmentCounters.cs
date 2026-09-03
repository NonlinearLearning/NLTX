namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileCountEnvironmentCounters(
  int LavaCount,
  int IceCount,
  int SandCount,
  int RockCount,
  int ShroomCount)
{
  public static TileCountEnvironmentCounters Empty { get; } = new(0, 0, 0, 0, 0);
}
