using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Commerce;

public sealed class SellbackMemoryState
{
  public SellbackMemoryState(IEnumerable<SellbackMemoryEntry>? entries = null)
  {
    ArgumentNullException.ThrowIfNull(entries);
    ImmutableArray<SellbackMemoryEntry> snapshot =
      (entries ?? []).ToImmutableArray();
    if (snapshot.Select(entry => (entry.ItemTypeId, entry.PrefixId)).Distinct().Count() !=
      snapshot.Length)
    {
      throw new ArgumentException(
        "Sellback memory cannot contain duplicate item and prefix keys.",
        nameof(entries));
    }

    Entries = snapshot;
  }

  public ImmutableArray<SellbackMemoryEntry> Entries { get; }

  public bool TryGetMemo(
    int itemTypeId,
    int prefixId,
    out SellbackMemoryEntry entry)
  {
    entry = Entries.FirstOrDefault(value =>
      value.ItemTypeId == itemTypeId && value.PrefixId == prefixId);
    return Entries.Any(value =>
      value.ItemTypeId == itemTypeId && value.PrefixId == prefixId);
  }

  public SellbackMemoryState WithMemo(SellbackMemoryEntry entry)
  {
    ImmutableArray<SellbackMemoryEntry>.Builder updated =
      ImmutableArray.CreateBuilder<SellbackMemoryEntry>(Entries.Length + 1);
    bool replaced = false;
    foreach (SellbackMemoryEntry existing in Entries)
    {
      if (existing.ItemTypeId == entry.ItemTypeId &&
        existing.PrefixId == entry.PrefixId)
      {
        updated.Add(entry);
        replaced = true;
      }
      else
      {
        updated.Add(existing);
      }
    }

    if (!replaced)
    {
      updated.Add(entry);
    }

    return new SellbackMemoryState(updated.ToImmutable());
  }
}
