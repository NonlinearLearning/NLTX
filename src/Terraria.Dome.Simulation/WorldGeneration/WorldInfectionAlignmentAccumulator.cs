namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldInfectionAlignmentAccumulator(
  int TotalEvil,
  int TotalBlood,
  int TotalGood,
  int TotalSolid)
{
  public static WorldInfectionAlignmentAccumulator Empty { get; } = new(0, 0, 0, 0);
}
