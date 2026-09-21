using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyCavePassDefinition(
  string Name,
  bool ResetsRandomFromWorldSeed,
  bool UsesTileRunner,
  bool MutatesTiles);

public static class LegacyCavePassContractDefinition
{
  private static readonly IReadOnlyList<LegacyCavePassDefinition> DefaultSchedule =
    Array.AsReadOnly<LegacyCavePassDefinition>(
    [
      new LegacyCavePassDefinition("MountainCaves", true, true, true),
      new LegacyCavePassDefinition("DirtLayerCaves", true, true, true),
      new LegacyCavePassDefinition("RockLayerCaves", true, true, true),
      new LegacyCavePassDefinition("SurfaceCaves", true, true, true),
      new LegacyCavePassDefinition("WavyCaves", true, true, true)
    ]);

  public static IReadOnlyList<LegacyCavePassDefinition> CreateDefaultSchedule()
  {
    return DefaultSchedule;
  }
}
