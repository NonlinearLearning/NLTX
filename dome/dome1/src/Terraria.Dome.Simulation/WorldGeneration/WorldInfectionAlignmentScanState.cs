namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldInfectionAlignmentScanState(
  TileCountSchedulingState Scheduling,
  WorldInfectionAlignmentAccumulator Accumulator)
{
  public static WorldInfectionAlignmentScanState Initial { get; } = new(
    TileCountSchedulingState.Initial,
    WorldInfectionAlignmentAccumulator.Empty);
}
