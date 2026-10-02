namespace Terraria.NonAuthoritative.ContentDefinitions;

public enum ItemPrefixCategory
{
  Melee,
  Magic,
  Summon,
  GunsBows,
  SwordsHammersAxesPicks,
  SpearsMacesChainsawsDrillsPunchCannon
}

public sealed class LegacyItemPrefixCatalog
{
  private readonly Dictionary<ItemPrefixCategory, bool[]> _categoryMasks = new();

  public LegacyItemPrefixCatalog(int itemTypeCount, int prefixCount)
  {
    if (itemTypeCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemTypeCount));
    }

    if (prefixCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(prefixCount));
    }

    ItemTypeCount = itemTypeCount;
    PrefixCount = prefixCount;
  }

  public int ItemTypeCount { get; }

  public int PrefixCount { get; }

  public IReadOnlyDictionary<ItemPrefixCategory, bool[]> CategoryMasks => _categoryMasks;

  internal void SetMask(ItemPrefixCategory category, IReadOnlyList<int> prefixIds)
  {
    bool[] mask = new bool[PrefixCount];
    foreach (int prefixId in prefixIds)
    {
      if ((uint)prefixId >= (uint)PrefixCount)
      {
        throw new ArgumentOutOfRangeException(nameof(prefixIds));
      }

      mask[prefixId] = true;
    }

    _categoryMasks[category] = mask;
  }

  public bool IsAllowed(ItemPrefixCategory category, int prefixId)
  {
    if ((uint)prefixId >= (uint)PrefixCount)
    {
      throw new ArgumentOutOfRangeException(nameof(prefixId));
    }

    return _categoryMasks.TryGetValue(category, out bool[]? mask) && mask[prefixId];
  }
}

public static class LegacyPrefixRegistrationSystem
{
  public static void SetCategoryMask(
    LegacyItemPrefixCatalog catalog,
    ItemPrefixCategory category,
    IReadOnlyList<int> prefixIds)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(prefixIds);
    catalog.SetMask(category, prefixIds);
  }
}
