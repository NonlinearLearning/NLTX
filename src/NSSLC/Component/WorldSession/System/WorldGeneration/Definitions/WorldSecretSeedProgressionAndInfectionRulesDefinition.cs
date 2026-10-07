using System;

namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedProgressionAndInfectionRulesDefinition(
  WorldSecretSeedDefinition ErrorWorld,
  WorldSecretSeedDefinition GraveyardBloodmoonStart,
  WorldSecretSeedDefinition RandomSpawn,
  WorldSecretSeedDefinition StartInHardmode,
  WorldSecretSeedDefinition NoInfection,
  WorldSecretSeedDefinition HallowOnTheSurface,
  WorldSecretSeedDefinition WorldIsInfected,
  WorldSecretSeedDefinition SurfaceIsMushrooms,
  WorldSecretSeedDefinition SurfaceIsDesert,
  WorldSecretSeedDefinition PooEverywhere,
  WorldSecretSeedDefinition Vampirism,
  WorldSecretSeedDefinition TeamBasedSpawns)
{
  public static WorldSecretSeedProgressionAndInfectionRulesDefinition Create(
    WorldSecretSeedRegistryDefinitionsProjection projection)
  {
    ArgumentNullException.ThrowIfNull(projection);
    return new WorldSecretSeedProgressionAndInfectionRulesDefinition(
      Find(projection, "error-world"),
      Find(projection, "graveyard-bloodmoon-start"),
      Find(projection, "random-spawn"),
      Find(projection, "start-in-hardmode"),
      Find(projection, "no-infection"),
      Find(projection, "hallow-on-the-surface"),
      Find(projection, "world-is-infected"),
      Find(projection, "surface-is-mushrooms"),
      Find(projection, "surface-is-desert"),
      Find(projection, "poo-everywhere"),
      Find(projection, "vampirism"),
      Find(projection, "team-based-spawns"));
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
