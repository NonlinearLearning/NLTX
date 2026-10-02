using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSecretSeedTerrainAndStructureRulesQuery
{
  public static WorldSecretSeedTerrainAndStructureRulesSelection Evaluate(
    WorldSecretSeedTerrainAndStructureRulesDefinition definition,
    IReadOnlySet<string> enabledVariants)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    return new WorldSecretSeedTerrainAndStructureRulesSelection(
      enabledVariants.Contains(definition.ExtraLivingTrees.Variant),
      enabledVariants.Contains(definition.ExtraFloatingIslands.Variant),
      enabledVariants.Contains(definition.BiggerAbandonedHouses.Variant),
      enabledVariants.Contains(definition.AddTeleporters.Variant),
      enabledVariants.Contains(definition.NoSpiderCaves.Variant),
      enabledVariants.Contains(definition.ActuallyNoTraps.Variant),
      enabledVariants.Contains(definition.DigExtraHoles.Variant),
      enabledVariants.Contains(definition.RoundLandmasses.Variant),
      enabledVariants.Contains(definition.ExtraLiquid.Variant),
      enabledVariants.Contains(definition.PortalGunInChests.Variant),
      enabledVariants.Contains(definition.DualDungeons.Variant));
  }
}
