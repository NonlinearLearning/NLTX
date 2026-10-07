using Terraria.Items;

namespace Terraria.WorldStorage;

public sealed class WorldContainerStore
{
  private WorldChestState?[] _chests = Array.Empty<WorldChestState?>();
  private Dictionary<TileCoordinate, ChestSlot> _chestSlotsByAnchor = new();
  private int _activeChestCount;
  private long _mutationRevision;

  public int Capacity => _chests.Length;
  public int ActiveChestCount => _activeChestCount;
  public long MutationRevision => _mutationRevision;

  public bool TrySetChestItem(
    TileCoordinate anchor,
    int itemIndex,
    ItemState item)
  {
    if (!_chestSlotsByAnchor.TryGetValue(anchor, out ChestSlot slot))
    {
      return false;
    }

    WorldChestState chest = _chests[slot.Value] ??
      throw new InvalidOperationException("A registered chest slot is empty.");
    if ((uint)itemIndex >= (uint)chest.Items.Length || item.Type < 0 || item.Stack < 0)
    {
      return false;
    }

    ItemState committedItem = item.IsEmpty ? default : item;
    if (chest.Items[itemIndex] == committedItem)
    {
      return true;
    }

    chest.Items[itemIndex] = committedItem;
    _mutationRevision++;
    return true;
  }

  public IReadOnlyList<WorldChestSnapshot> CreateSnapshot() {
    return Array.AsReadOnly(_chests.Where(chest => chest is not null)
        .Select(chest => new WorldChestSnapshot(chest!.Anchor, chest.Name, chest.Items)).ToArray());
  }

  internal void Replace(IReadOnlyList<WorldChestSnapshot> snapshots) {
    var chests = new WorldChestState?[snapshots.Count];
    var anchors = new Dictionary<TileCoordinate, ChestSlot>();
    for (int index = 0; index < snapshots.Count; index++) {
      WorldChestSnapshot snapshot = snapshots[index];
      var slot = new ChestSlot(index);
      anchors.Add(snapshot.Anchor, slot);
      chests[index] = new WorldChestState {
        Slot = slot, Anchor = snapshot.Anchor, Name = snapshot.Name,
        Items = snapshot.Items.ToArray(), ItemCapacity = snapshot.Items.Count
      };
    }
    _chests = chests;
    _chestSlotsByAnchor = anchors;
    _activeChestCount = snapshots.Count;
    _mutationRevision++;
  }
}
