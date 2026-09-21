using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSecretSeedVisualAndSurfaceRulesQuery
{
  public static WorldSecretSeedVisualAndSurfaceRulesSelection Evaluate(
    WorldSecretSeedVisualAndSurfaceRulesDefinition definition,
    IReadOnlySet<string> enabledVariants)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    return new WorldSecretSeedVisualAndSurfaceRulesSelection(
      enabledVariants.Contains(definition.PaintEverythingGray.Variant),
      enabledVariants.Contains(definition.PaintEverythingNegative.Variant),
      enabledVariants.Contains(definition.CoatEverythingEcho.Variant),
      enabledVariants.Contains(definition.CoatEverythingIlluminant.Variant),
      enabledVariants.Contains(definition.NoSurface.Variant),
      enabledVariants.Contains(definition.SurfaceIsInSpace.Variant),
      enabledVariants.Contains(definition.RainsForAYear.Variant),
      enabledVariants.Contains(definition.RainbowStuff.Variant),
      enabledVariants.Contains(definition.WorldIsFrozen.Variant));
  }
}
