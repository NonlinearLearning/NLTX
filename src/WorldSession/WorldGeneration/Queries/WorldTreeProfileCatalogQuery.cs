using System;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldTreeProfileCatalogQuery
{
  public static bool TryGetFromTreeId(
    WorldTreeProfileCatalog catalog,
    int treeTileType,
    out WorldTreeProfileDefinition profile)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.TryGetFromTreeId(treeTileType, out profile);
  }
}
