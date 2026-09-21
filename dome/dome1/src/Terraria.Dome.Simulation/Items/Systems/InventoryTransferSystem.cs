using System;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class InventoryTransferSystem
{
  public int TransferIntoSlot(
    InventoryComponent inventory,
    int slot,
    ItemStack incoming,
    ItemDefinitionRegistry definitions)
  {
    return TransferIntoSlot(inventory, slot, incoming, default, definitions);
  }

  public int TransferIntoSlot(
    InventoryComponent inventory,
    int slot,
    ItemStack incoming,
    ItemInstanceStateComponent incomingState,
    ItemDefinitionRegistry definitions)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(definitions);
    if (incoming.IsAir)
    {
      return 0;
    }

    incomingState.Validate();

    ItemDefinition definition = definitions.Get(incoming.ItemType);
    if (incoming.Quantity > definition.StackLimit)
    {
      throw new ArgumentOutOfRangeException(nameof(incoming));
    }

    if (!definition.CanStack && incoming.Quantity > 1)
    {
      return incoming.Quantity;
    }

    ItemStack current = inventory.GetSlot(slot);
    if (!current.IsAir && current.Stack > definition.MaxStack)
    {
      return incoming.Quantity;
    }

    if (!current.IsEmpty && current.ItemType != incoming.ItemType)
    {
      return incoming.Quantity;
    }

    if (!current.IsEmpty && !definition.CanStack)
    {
      return incoming.Quantity;
    }

    bool uniqueStack = definition.IsUniqueStack;
    if (!current.IsEmpty && !InventoryComponent.CanMerge(
        current,
        inventory.GetInstanceState(slot),
        incoming,
        incomingState,
        uniqueStack))
    {
      return incoming.Quantity;
    }

    int available = definition.MaxStack - current.Stack;
    int accepted = Math.Min(available, incoming.Stack);
    if (accepted == 0)
    {
      return incoming.Quantity;
    }

    inventory.SetSlot(
      slot,
      new ItemStack(incoming.ItemType, current.Quantity + accepted, incoming.Prefix));
    inventory.SetInstanceState(slot, incomingState);
    return incoming.Quantity - accepted;
  }
}
