namespace Terraria.EntityLifecycleAttribution;

public sealed class WorldContainerStore
{
  private readonly Dictionary<int, WorldChestState> _chests = new();

  public bool TryCreateChest(int slot, int capacity, out WorldChestState chest)
  {
    chest = null!;
    if (slot < 0 || capacity <= 0 || _chests.ContainsKey(slot))
    {
      return false;
    }

    chest = new WorldChestState(slot, capacity);
    _chests.Add(slot, chest);
    return true;
  }

  public bool TryGetChest(int slot, out WorldChestState? chest)
  {
    return _chests.TryGetValue(slot, out chest);
  }

  public bool TryRemoveEmptyChest(int slot)
  {
    return _chests.TryGetValue(slot, out WorldChestState? chest) &&
           chest.IsEmpty &&
           _chests.Remove(slot);
  }
}
