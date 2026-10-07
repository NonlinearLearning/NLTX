using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSkyblockGenerationPolicyQuery
{
  public static WorldSkyblockGenerationPolicySelection Evaluate(
    bool skyblockWorld,
    WorldSecretSeedRuntimeRegistrySnapshot runtime,
    WorldSecretSeedVisualAndSurfaceRulesDefinition visualAndSurfaceRules,
    WorldSecretSeedTerrainAndStructureRulesDefinition terrainAndStructureRules,
    WorldSecretSeedProgressionAndInfectionRulesDefinition progressionAndInfectionRules)
  {
    if (!runtime.IsActive)
    {
      throw new ArgumentException(
        "Skyblock policy evaluation requires an active runtime registry snapshot.",
        nameof(runtime));
    }

    bool extraFloatingIslands = runtime.IsEnabled(
      terrainAndStructureRules.ExtraFloatingIslands.Variant);
    bool allowsSomeGeneration =
      runtime.IsEnabled(visualAndSurfaceRules.WorldIsFrozen.Variant) ||
      runtime.IsEnabled(progressionAndInfectionRules.SurfaceIsDesert.Variant) ||
      runtime.IsEnabled(progressionAndInfectionRules.SurfaceIsMushrooms.Variant) ||
      runtime.IsEnabled(progressionAndInfectionRules.WorldIsInfected.Variant) ||
      runtime.IsEnabled(progressionAndInfectionRules.HallowOnTheSurface.Variant) ||
      runtime.IsEnabled(progressionAndInfectionRules.NoInfection.Variant) ||
      extraFloatingIslands ||
      runtime.IsEnabled(terrainAndStructureRules.ExtraLiquid.Variant) ||
      runtime.IsEnabled(terrainAndStructureRules.ExtraLivingTrees.Variant);

    return new WorldSkyblockGenerationPolicySelection(
      skyblockWorld && !extraFloatingIslands,
      skyblockWorld,
      skyblockWorld && !allowsSomeGeneration,
      runtime.GenerationId,
      runtime.RuntimeVersion);
  }
}
