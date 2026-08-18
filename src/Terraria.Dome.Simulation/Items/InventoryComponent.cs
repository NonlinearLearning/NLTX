using System;
using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items;

public sealed class InventoryComponent
{
  public const int HotbarSlotCount = 10;
  public const int SlotCount = 40;

  private readonly ItemStack[] _slots = new ItemStack[SlotCount];
  private readonly ItemInstanceStateComponent[] _instanceStates =
    new ItemInstanceStateComponent[SlotCount];

  public long Revision { get; private set; }

  public int SelectedSlot { get; private set; }
  public ReadOnlySpan<ItemStack> Slots => _slots;
  public ReadOnlySpan<ItemInstanceStateComponent> InstanceStates => _instanceStates;

  public ItemStack GetSlot(int slot)
  {
    ValidateSlot(slot);
    return _slots[slot];
  }

  public void SetSelectedSlot(int slot)
  {
    ValidateHotbarSlot(slot);
    if (SelectedSlot == slot)
    {
      return;
    }

    SelectedSlot = slot;
    Revision = checked(Revision + 1);
  }

  public void SetSlot(int slot, ItemStack stack)
  {
    ValidateSlot(slot);
    ItemStack normalized = stack.IsEmpty ? ItemStack.Empty : stack;
    bool changed = _slots[slot] != normalized;
    _slots[slot] = normalized;
    if (_slots[slot].IsEmpty && _instanceStates[slot] != default)
    {
      _instanceStates[slot] = default;
      changed = true;
    }

    if (changed)
    {
      Revision = checked(Revision + 1);
    }
  }

  public ItemInstanceStateComponent GetInstanceState(int slot)
  {
    ValidateSlot(slot);
    return _instanceStates[slot];
  }

  public void SetInstanceState(int slot, ItemInstanceStateComponent state)
  {
    ValidateSlot(slot);
    if (_slots[slot].IsEmpty && state != default)
    {
      throw new InvalidOperationException("An empty slot cannot carry item instance state.");
    }

    if (_instanceStates[slot] == state)
    {
      return;
    }

    _instanceStates[slot] = state;
    Revision = checked(Revision + 1);
  }

  public static bool CanMerge(
    ItemStack first,
    ItemInstanceStateComponent firstState,
    ItemStack second,
    ItemInstanceStateComponent secondState,
    bool uniqueStack)
  {
    return !first.IsEmpty && !second.IsEmpty && first.ItemType == second.ItemType &&
      !uniqueStack && firstState == secondState;
  }

  private static void ValidateSlot(int slot)
  {
    if (slot < 0 || slot >= SlotCount)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }

  private static void ValidateHotbarSlot(int slot)
  {
    if (slot < 0 || slot >= HotbarSlotCount)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }
}
