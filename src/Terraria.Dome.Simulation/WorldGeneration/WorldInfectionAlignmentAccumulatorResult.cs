namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldInfectionAlignmentAccumulatorResult(
  WorldInfectionAlignmentAccumulator State,
  bool HasPublished,
  WorldInfectionAlignmentSnapshot Published);
