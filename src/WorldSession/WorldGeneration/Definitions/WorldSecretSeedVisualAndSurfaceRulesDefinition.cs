using System;

namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedVisualAndSurfaceRulesDefinition(
  WorldSecretSeedDefinition PaintEverythingGray,
  WorldSecretSeedDefinition PaintEverythingNegative,
  WorldSecretSeedDefinition CoatEverythingEcho,
  WorldSecretSeedDefinition CoatEverythingIlluminant,
  WorldSecretSeedDefinition NoSurface,
  WorldSecretSeedDefinition SurfaceIsInSpace,
  WorldSecretSeedDefinition RainsForAYear,
  WorldSecretSeedDefinition RainbowStuff,
  WorldSecretSeedDefinition WorldIsFrozen)
{
  public static WorldSecretSeedVisualAndSurfaceRulesDefinition Create(
    WorldSecretSeedRegistryDefinitionsProjection projection)
  {
    ArgumentNullException.ThrowIfNull(projection);
    return new WorldSecretSeedVisualAndSurfaceRulesDefinition(
      Find(projection, "paint-everything-gray"),
      Find(projection, "paint-everything-negative"),
      Find(projection, "coat-everything-echo"),
      Find(projection, "coat-everything-illuminant"),
      Find(projection, "no-surface"),
      Find(projection, "surface-is-in-space"),
      Find(projection, "rains-for-a-year"),
      Find(projection, "rainbow-stuff"),
      Find(projection, "world-is-frozen"));
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
