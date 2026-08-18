using System;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.WorldObjects;

public sealed class ChestInventoryComponent
{
  public const int SlotCount = 40;

  private readonly ItemStack[] _slots;

  public ChestInventoryComponent(int capacity = SlotCount)
  {
    if (capacity != SlotCount)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity), "World chests require 40 slots.");
    }

    _slots = new ItemStack[capacity];
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

  private static void ValidateSlot(int slot)
  {
    if (slot < 0 || slot >= SlotCount)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }
}
