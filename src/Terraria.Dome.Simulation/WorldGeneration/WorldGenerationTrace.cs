using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldGenerationTrace
{
  public WorldGenerationTrace(
    WorldGridSnapshot finalSnapshot,
    IReadOnlyList<WorldGenerationStageSnapshot> stages)
  {
    FinalSnapshot = finalSnapshot ?? throw new ArgumentNullException(nameof(finalSnapshot));
    ArgumentNullException.ThrowIfNull(stages);
    Stages = new List<WorldGenerationStageSnapshot>(stages).AsReadOnly();
  }

  public WorldGridSnapshot FinalSnapshot { get; }

  public IReadOnlyList<WorldGenerationStageSnapshot> Stages { get; }
}
