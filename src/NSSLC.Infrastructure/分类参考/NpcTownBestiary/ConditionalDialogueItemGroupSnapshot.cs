namespace Terraria.NpcTownBestiary;

public sealed class ConditionalDialogueItemGroupSnapshot
{
  internal ConditionalDialogueItemGroupSnapshot(
    string key,
    IEnumerable<int> itemTypes)
  {
    Key = key;
    ItemTypes = new ReadOnlySet<int>(itemTypes.ToHashSet());
  }

  public string Key { get; }

  public IReadOnlySet<int> ItemTypes { get; }

  public bool Contains(int itemType)
  {
    return ItemTypes.Contains(itemType);
  }
}
