using System;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.WorldObjects;

public sealed class ChestInventoryComponent
{
  public const int SlotCount = 40;

  private readonly ItemStack[] _slots;

  public ChestInventoryComponent(int capacity = SlotCount)
  {
    if (capacity < SlotCount || capacity > ChestCapacityDefinition.AbsoluteMaximumItems)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _slots = new ItemStack[capacity];
  }

  public int Capacity => _slots.Length;

  public void NormalizeEmptySlots()
  {
    for (int index = 0; index < _slots.Length; index++)
    {
      if (_slots[index].IsEmpty)
      {
        _slots[index] = ItemStack.Empty;
      }
    }
  }

  public ItemStack GetSlot(int slot)
  {
    ValidateSlot(slot);
    return _slots[slot];
  }

  public void SetSlot(int slot, ItemStack stack)
  {
    ValidateSlot(slot);
    _slots[slot] = stack.IsEmpty ? ItemStack.Empty : stack;
  }

  private void ValidateSlot(int slot)
  {
    if (slot < 0 || slot >= _slots.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }
}
