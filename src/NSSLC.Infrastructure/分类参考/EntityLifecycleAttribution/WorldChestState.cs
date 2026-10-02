namespace Terraria.EntityLifecycleAttribution;

public sealed class WorldChestState
{
  private readonly int?[] _items;

  public WorldChestState(int slot, int capacity)
  {
    if (slot < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }

    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    Slot = slot;
    _items = new int?[capacity];
  }

  public int Slot { get; }

  public int Capacity => _items.Length;

  public bool IsEmpty => _items.All(item => item is null);

  public bool TrySetItem(int index, int contentType)
  {
    if (index < 0 || index >= _items.Length || contentType < 0)
    {
      return false;
    }

    _items[index] = contentType;
    return true;
  }

  public void Clear()
  {
    Array.Clear(_items);
  }
}
