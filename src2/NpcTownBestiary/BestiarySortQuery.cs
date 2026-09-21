namespace Terraria.NpcTownBestiary;

public static class BestiarySortQuery
{
  public static IReadOnlyList<BestiarySortView> Sort(
    IEnumerable<BestiarySortView> entries,
    BestiarySortKind kind)
  {
    ArgumentNullException.ThrowIfNull(entries);
    IOrderedEnumerable<BestiarySortView> ordered = kind switch
    {
      BestiarySortKind.NetId => entries.OrderBy(view => view.Entry.NpcNetId.Value),
      BestiarySortKind.UnlockState => entries.OrderByDescending(view => view.UnlockState),
      BestiarySortKind.BestiarySortingId => entries.OrderBy(view => view.Entry.SortingId),
      BestiarySortKind.Rarity => entries.OrderByDescending(view => view.Entry.RarityLevel),
      BestiarySortKind.Alphabetical => entries.OrderBy(
        view => view.Entry.DisplayName,
        StringComparer.OrdinalIgnoreCase),
      BestiarySortKind.Stat => entries.OrderByDescending(view => view.StatValue),
      _ => entries.OrderBy(view => 0)
    };

    return ordered
      .ThenBy(view => view.Entry.Key.Value, StringComparer.Ordinal)
      .ToArray();
  }
}
