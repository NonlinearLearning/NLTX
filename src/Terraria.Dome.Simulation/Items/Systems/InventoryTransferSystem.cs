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
    if (incoming.IsEmpty)
    {
      return 0;
    }

    ItemDefinition definition = definitions.Get(incoming.ItemType);
    if (incoming.Quantity > definition.StackLimit)
    {
      throw new ArgumentOutOfRangeException(nameof(incoming));
    }

    ItemStack current = inventory.GetSlot(slot);
    if (!current.IsEmpty && current.ItemType != incoming.ItemType)
    {
      return incoming.Quantity;
    }

    bool uniqueStack = definition.Identity?.UniqueStack ?? false;
    if (!current.IsEmpty && !InventoryComponent.CanMerge(
        current,
        inventory.GetInstanceState(slot),
        incoming,
        incomingState,
        uniqueStack))
    {
      return incoming.Quantity;
    }

    int available = definition.StackLimit - current.Quantity;
    int accepted = Math.Min(available, incoming.Quantity);
    if (accepted == 0)
    {
      return incoming.Quantity;
    }

    inventory.SetSlot(slot, new ItemStack(incoming.ItemType, current.Quantity + accepted));
    inventory.SetInstanceState(slot, incomingState);
    return incoming.Quantity - accepted;
  }
}
