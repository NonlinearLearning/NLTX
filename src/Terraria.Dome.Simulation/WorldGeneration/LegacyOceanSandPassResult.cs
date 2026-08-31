using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyOceanSandPassResult(
  bool Skipped,
  IReadOnlyList<LegacyOceanSandBandPlan> Bands,
  IReadOnlyList<LegacyOceanSandColumnPlan> Columns,
  IReadOnlyList<(int X, int Y)> PyramidCandidates,
  IReadOnlyList<LegacyOceanSandTileWrite> TileWrites,
  long RandomSamplesConsumed);
