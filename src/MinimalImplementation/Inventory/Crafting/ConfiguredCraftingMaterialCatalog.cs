using System.Collections.ObjectModel;
using MinimalImplementation.Server.Inventory.Content;

namespace MinimalImplementation.Server.Inventory.Crafting;

/// <summary>
/// Resolves direct item requirements plus explicitly configured recipe-group memberships.
/// Unknown Version4 groups remain unsupported until supplied from a reviewed content snapshot.
/// </summary>
public sealed class ConfiguredCraftingMaterialCatalog : ICraftingMaterialCatalog
{
  private readonly IReadOnlyDictionary<int, IReadOnlySet<int>> _recipeGroups;

  public ConfiguredCraftingMaterialCatalog(
    IReadOnlyDictionary<int, IReadOnlyCollection<int>> recipeGroups)
  {
    ArgumentNullException.ThrowIfNull(recipeGroups);
    var copy = new Dictionary<int, IReadOnlySet<int>>();
    foreach (KeyValuePair<int, IReadOnlyCollection<int>> pair in recipeGroups)
    {
      ArgumentNullException.ThrowIfNull(pair.Value);
      if (pair.Key >= 0 || pair.Value.Count == 0
        || pair.Value.Any(itemType => !ItemContentFacts.IsKnownItemType(itemType)))
      {
        throw new ArgumentException(
          "Configured recipe groups use negative IDs and nonempty sets of known item IDs.",
          nameof(recipeGroups));
      }

      copy.Add(pair.Key, new HashSet<int>(pair.Value));
    }

    _recipeGroups = new ReadOnlyDictionary<int, IReadOnlySet<int>>(copy);
  }

  public bool IsSupportedRequirement(int itemIdOrRecipeGroup)
  {
    return itemIdOrRecipeGroup >= 0
      ? ItemContentFacts.IsKnownItemType(itemIdOrRecipeGroup)
      : _recipeGroups.ContainsKey(itemIdOrRecipeGroup);
  }

  public bool Matches(int itemIdOrRecipeGroup, int itemType)
  {
    if (!ItemContentFacts.IsKnownItemType(itemType))
    {
      return false;
    }

    return itemIdOrRecipeGroup >= 0
      ? itemIdOrRecipeGroup == itemType
      : _recipeGroups.TryGetValue(itemIdOrRecipeGroup, out IReadOnlySet<int>? members)
        && members.Contains(itemType);
  }
}
