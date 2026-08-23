using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record WorldGenerationStageSnapshot(
  WorldGenerationStage Stage,
  WorldGridSnapshot Snapshot,
  long NextSequence);
