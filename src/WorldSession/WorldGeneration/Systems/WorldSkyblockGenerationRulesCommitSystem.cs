using System;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Systems;

public static class WorldSkyblockGenerationRulesCommitSystem
{
  public static WorldSkyblockGenerationRulesCommitResult Commit(
    WorldSkyblockGenerationScanComponent component,
    WorldSkyblockGenerationRulesSelection rules,
    bool previousLowTiles,
    ISkyblockGenerationEffectsPort effects)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(effects);
    if (component.Lifecycle != WorldSkyblockGenerationScanLifecycle.Scanning)
    {
      throw new InvalidOperationException(
        "Skyblock rules can only be committed for an active scan.");
    }

    if (component.GenerationId != rules.GenerationId ||
        component.ScanVersion != rules.ScanVersion)
    {
      throw new ArgumentException(
        "Skyblock rules must belong to the active scan session.",
        nameof(rules));
    }

    bool lowTilesChanged = previousLowTiles != rules.LowTiles;
    component.ResetAfterCommit();
    if (rules.NoDungeon)
    {
      effects.ClearDungeonCoordinates();
    }

    if (lowTilesChanged)
    {
      effects.NotifyLowTilesChanged(rules.LowTiles);
    }

    return new WorldSkyblockGenerationRulesCommitResult(
      rules.GenerationId,
      rules.ScanVersion,
      lowTilesChanged,
      rules.NoDungeon);
  }
}
