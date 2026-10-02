namespace Terraria.Items.InventoryContainers;

public sealed class ChestSlotStorageComponent
{
  private readonly ItemStackSnapshot?[] _slots;

  public ChestSlotStorageComponent(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _slots = new ItemStackSnapshot[capacity];
  }

  public int Capacity => _slots.Length;

  public IReadOnlyList<ItemStackSnapshot?> Slots => Array.AsReadOnly(_slots.ToArray());

  public bool TrySetSlot(int slot, ItemIdentityAndStackComponent item)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!IsValidSlot(slot))
    {
      return false;
    }

    _slots[slot] = item.CreateSnapshot();
    return true;
  }

  public bool TrySetSlot(int slot, ItemStackSnapshot? item)
  {
    if (!IsValidSlot(slot))
    {
      return false;
    }

    _slots[slot] = item;
    return true;
  }

  public bool TryGetSlot(int slot, out ItemStackSnapshot? item)
  {
    if (!IsValidSlot(slot))
    {
      item = null;
      return false;
    }

    item = _slots[slot];
    return true;
  }

  public bool TryClearSlot(int slot)
  {
    if (!IsValidSlot(slot))
    {
      return false;
    }

    _slots[slot] = null;
    return true;
  }

  private bool IsValidSlot(int slot)
  {
    return (uint)slot < (uint)_slots.Length;
  }
}
