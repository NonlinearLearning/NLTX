using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSecretSeedProgressionAndInfectionRulesQuery
{
  public static WorldSecretSeedProgressionAndInfectionRulesSelection Evaluate(
    WorldSecretSeedProgressionAndInfectionRulesDefinition definition,
    IReadOnlySet<string> enabledVariants)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    return new WorldSecretSeedProgressionAndInfectionRulesSelection(
      enabledVariants.Contains(definition.ErrorWorld.Variant),
      enabledVariants.Contains(definition.GraveyardBloodmoonStart.Variant),
      enabledVariants.Contains(definition.RandomSpawn.Variant),
      enabledVariants.Contains(definition.StartInHardmode.Variant),
      enabledVariants.Contains(definition.NoInfection.Variant),
      enabledVariants.Contains(definition.HallowOnTheSurface.Variant),
      enabledVariants.Contains(definition.WorldIsInfected.Variant),
      enabledVariants.Contains(definition.SurfaceIsMushrooms.Variant),
      enabledVariants.Contains(definition.SurfaceIsDesert.Variant),
      enabledVariants.Contains(definition.PooEverywhere.Variant),
      enabledVariants.Contains(definition.Vampirism.Variant),
      enabledVariants.Contains(definition.TeamBasedSpawns.Variant));
  }
}
