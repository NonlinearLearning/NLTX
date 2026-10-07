using System;

namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedTerrainAndStructureRulesDefinition(
  WorldSecretSeedDefinition ExtraLivingTrees,
  WorldSecretSeedDefinition ExtraFloatingIslands,
  WorldSecretSeedDefinition BiggerAbandonedHouses,
  WorldSecretSeedDefinition AddTeleporters,
  WorldSecretSeedDefinition NoSpiderCaves,
  WorldSecretSeedDefinition ActuallyNoTraps,
  WorldSecretSeedDefinition DigExtraHoles,
  WorldSecretSeedDefinition RoundLandmasses,
  WorldSecretSeedDefinition ExtraLiquid,
  WorldSecretSeedDefinition PortalGunInChests,
  WorldSecretSeedDefinition DualDungeons)
{
  public static WorldSecretSeedTerrainAndStructureRulesDefinition Create(
    WorldSecretSeedRegistryDefinitionsProjection projection)
  {
    ArgumentNullException.ThrowIfNull(projection);
    return new WorldSecretSeedTerrainAndStructureRulesDefinition(
      Find(projection, "extra-living-trees"),
      Find(projection, "extra-floating-islands"),
      Find(projection, "bigger-abandoned-houses"),
      Find(projection, "add-teleporters"),
      Find(projection, "no-spider-caves"),
      Find(projection, "actually-no-traps"),
      Find(projection, "dig-extra-holes"),
      Find(projection, "round-landmasses"),
      Find(projection, "extra-liquid"),
      Find(projection, "portal-gun-in-chests"),
      Find(projection, "dual-dungeons"));
  }

  private static WorldSecretSeedDefinition Find(
    WorldSecretSeedRegistryDefinitionsProjection projection,
    string variant)
  {
    if (projection.TryGet(variant, out WorldSecretSeedDefinition definition))
    {
      return definition;
    }

    throw new InvalidOperationException(
      $"The secret-seed catalog is missing the required variant '{variant}'.");
  }
}
