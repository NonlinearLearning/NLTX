using System.Collections.ObjectModel;
using MinimalImplementation.Server.Inventory.Content;

namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>
/// Resolves item sorting categories from a frozen server-owned Version4 content table.
/// The inspected Version4 QuickStack implementation is empty, so no guessed categories are added.
/// </summary>
public sealed class ConfiguredQuickStackCategoryResolver : IQuickStackCategoryResolver
{
  private readonly IReadOnlyDictionary<int, int> _categories;

  public ConfiguredQuickStackCategoryResolver(IReadOnlyDictionary<int, int> categories)
  {
    ArgumentNullException.ThrowIfNull(categories);
    var copy = new Dictionary<int, int>();
    foreach (KeyValuePair<int, int> pair in categories)
    {
      if (!ItemContentFacts.IsKnownItemType(pair.Key) || pair.Value < 0)
      {
        throw new ArgumentException("QuickStack categories require known item IDs and nonnegative category IDs.", nameof(categories));
      }

      copy.Add(pair.Key, pair.Value);
    }

    _categories = new ReadOnlyDictionary<int, int>(copy);
  }

  public bool TryGetCategory(int itemType, out int category)
  {
    return _categories.TryGetValue(itemType, out category);
  }
}
