using System;

namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedSeasonalRulesDefinition(
  WorldSecretSeedDefinition HalloweenGeneration,
  WorldSecretSeedDefinition EndlessHalloween,
  WorldSecretSeedDefinition EndlessChristmas)
{
  public static WorldSecretSeedSeasonalRulesDefinition Create(
    WorldSecretSeedRegistryDefinitionsProjection projection)
  {
    ArgumentNullException.ThrowIfNull(projection);
    return new WorldSecretSeedSeasonalRulesDefinition(
      Find(projection, "halloween-gen"),
      Find(projection, "endless-halloween"),
      Find(projection, "endless-christmas"));
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
