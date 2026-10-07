using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Maps the explicit search result to the legacy bool plus sentinel contract.
/// </summary>
/// <remarks>
/// This adapter only translates a caller-owned snapshot result. It does not
/// access ambient world state or become a second search implementation.
/// </remarks>
public static class WorldGenerationSearchCompatibilityAdapter
{
  public static bool TryFind(
    WorldGenerationConditionsAndSearchesQuery.ITileSnapshotReader snapshot,
    TilePosition origin,
    WorldGenerationConditionsAndSearchesQuery.SearchDefinition search,
    out TilePosition result)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(search);

    WorldGenerationConditionsAndSearchesQuery.SearchResult searchResult =
      WorldGenerationConditionsAndSearchesQuery.Find(
        snapshot,
        origin,
        search);
    if (!searchResult.Found)
    {
      result = WorldGenerationConditionsAndSearchesQuery.NOT_FOUND;
      return false;
    }

    result = searchResult.Position;
    return true;
  }
}
