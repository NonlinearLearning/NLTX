using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSecretSeedSeasonalRulesQuery
{
  public static WorldSecretSeedSeasonalRulesSelection Evaluate(
    WorldSecretSeedSeasonalRulesDefinition definition,
    IReadOnlySet<string> enabledVariants)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    return new WorldSecretSeedSeasonalRulesSelection(
      enabledVariants.Contains(definition.HalloweenGeneration.Variant),
      enabledVariants.Contains(definition.EndlessHalloween.Variant),
      enabledVariants.Contains(definition.EndlessChristmas.Variant));
  }
}
