using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class StatuePlacementCatalogQuery
{
  public static StatuePlacementOption GetOption(
    StatuePlacementCatalogDefinition catalog,
    int statueIndex)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    if (statueIndex < 0 || statueIndex >= catalog.Options.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(statueIndex));
    }

    return catalog.Options[statueIndex];
  }
}
