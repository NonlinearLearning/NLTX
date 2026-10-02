using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

public sealed class BestiaryCatalogComponent
{
  private readonly List<BestiaryEntryDefinition> _entries = new();
  private readonly Dictionary<NpcNetId, BestiaryEntryDefinition> _entriesByNetId = new();
  private readonly Dictionary<BestiaryEntryKey, BestiaryEntryDefinition> _entriesByKey = new();

  public bool IsFrozen { get; private set; }

  public IReadOnlyList<BestiaryEntryDefinition> Entries => _entries.AsReadOnly();

  public BestiaryEntryDefinition? FindByNetId(NpcNetId npcNetId)
  {
    return _entriesByNetId.TryGetValue(npcNetId, out BestiaryEntryDefinition? entry)
      ? entry
      : null;
  }

  public BestiaryEntryDefinition? FindByKey(BestiaryEntryKey key)
  {
    return _entriesByKey.TryGetValue(key, out BestiaryEntryDefinition? entry)
      ? entry
      : null;
  }

  internal BestiaryEntryDefinition Register(BestiaryEntryDefinition entry)
  {
    ArgumentNullException.ThrowIfNull(entry);
    if (IsFrozen)
    {
      throw new InvalidOperationException("Bestiary catalog is frozen.");
    }

    if (_entriesByKey.ContainsKey(entry.Key))
    {
      throw new InvalidOperationException($"Bestiary entry key already exists: {entry.Key.Value}");
    }

    if (_entriesByNetId.ContainsKey(entry.NpcNetId))
    {
      throw new InvalidOperationException($"Bestiary NPC net ID already exists: {entry.NpcNetId.Value}");
    }

    _entries.Add(entry);
    _entriesByKey.Add(entry.Key, entry);
    _entriesByNetId.Add(entry.NpcNetId, entry);
    return entry;
  }

  internal void Freeze()
  {
    IsFrozen = true;
  }

  internal bool TryAttachDrop(NpcNetId npcNetId, BestiaryDropRateView dropRate)
  {
    if (IsFrozen)
    {
      throw new InvalidOperationException("Bestiary catalog is frozen.");
    }

    BestiaryEntryDefinition? entry = FindByNetId(npcNetId);
    if (entry is null)
    {
      return false;
    }

    entry.InfoElements = entry.InfoElements
      .Append(new BestiaryInfoElementState(dropRate: dropRate))
      .ToArray();
    return true;
  }
}
