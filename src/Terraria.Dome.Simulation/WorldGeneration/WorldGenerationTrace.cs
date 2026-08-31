using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class WorldGenerationTrace
{
  public WorldGenerationTrace(
    WorldGridSnapshot finalSnapshot,
    IReadOnlyList<WorldGenerationStageSnapshot> stages,
    WorldGenerationLifecycleSnapshot initialLifecycle,
    WorldGenerationLifecycleSnapshot lifecycle,
    WorldGenerationDistanceDefaults distanceDefaults,
    TerrainProfileComponent terrainProfile)
  {
    FinalSnapshot = finalSnapshot ?? throw new ArgumentNullException(nameof(finalSnapshot));
    ArgumentNullException.ThrowIfNull(stages);
    if (!initialLifecycle.IsGeneratingOrLoading)
    {
      throw new ArgumentException(
        "The initial lifecycle must be marked as generating or loading.",
        nameof(initialLifecycle));
    }

    Stages = new List<WorldGenerationStageSnapshot>(stages).AsReadOnly();
    InitialLifecycle = initialLifecycle;
    Lifecycle = lifecycle;
    DistanceDefaults = distanceDefaults;
    TerrainProfile = terrainProfile;
  }

  public WorldGridSnapshot FinalSnapshot { get; }

  public IReadOnlyList<WorldGenerationStageSnapshot> Stages { get; }

  public WorldGenerationLifecycleSnapshot InitialLifecycle { get; }

  public WorldGenerationLifecycleSnapshot Lifecycle { get; }

  public WorldGenerationDistanceDefaults DistanceDefaults { get; }

  public TerrainProfileComponent TerrainProfile { get; }
}
