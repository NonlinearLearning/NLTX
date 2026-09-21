using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

public sealed class ConditionalDialogueItemGroupAdapter
{
  private readonly Dictionary<string, HashSet<int>> _groups = new(StringComparer.Ordinal);

  public IReadOnlyDictionary<string, ConditionalDialogueItemGroupSnapshot> Snapshot()
  {
    Dictionary<string, ConditionalDialogueItemGroupSnapshot> snapshots = new(
      StringComparer.Ordinal);
    foreach (KeyValuePair<string, HashSet<int>> group in _groups)
    {
      snapshots[group.Key] = new ConditionalDialogueItemGroupSnapshot(
        group.Key,
        group.Value);
    }

    return new ReadOnlyDictionary<string, ConditionalDialogueItemGroupSnapshot>(snapshots);
  }

  public void RegisterStatic(string key, IEnumerable<int> itemTypes)
  {
    Register(key, itemTypes);
  }

  public void ReplaceDynamic(string key, IEnumerable<int> itemTypes)
  {
    Register(key, itemTypes);
  }

  private void Register(string key, IEnumerable<int> itemTypes)
  {
    if (string.IsNullOrWhiteSpace(key))
    {
      throw new ArgumentException("Item group key is required.", nameof(key));
    }

    ArgumentNullException.ThrowIfNull(itemTypes);
    _groups[key] = itemTypes.ToHashSet();
  }
}
