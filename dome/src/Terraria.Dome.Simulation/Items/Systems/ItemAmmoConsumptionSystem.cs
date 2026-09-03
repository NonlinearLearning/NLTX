using System;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ItemAmmoConsumptionSystem
{
  public bool HasAmmo(
    InventoryComponent inventory,
    ushort ammoType,
    ItemDefinitionRegistry definitions)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(definitions);
    if (ammoType == 0 || !definitions.TryGet(ammoType, out ItemDefinition definition) ||
        definition.NotAmmo)
    {
      return false;
    }

    for (int index = 0; index < InventoryComponent.SlotCount; index++)
    {
      ItemStack stack = inventory.GetSlot(index);
      if (!stack.IsEmpty && stack.ItemType == ammoType &&
          stack.Quantity <= definition.StackLimit)
      {
        return true;
      }
    }

    return false;
  }

  public bool TryConsume(
    InventoryComponent inventory,
    ushort ammoType,
    ItemDefinitionRegistry definitions,
    out int slot)
  {
    slot = -1;
    if (!HasAmmo(inventory, ammoType, definitions))
    {
      return false;
    }

    for (int index = 0; index < InventoryComponent.SlotCount; index++)
    {
      ItemStack stack = inventory.GetSlot(index);
      if (stack.ItemType != ammoType || stack.IsEmpty ||
          stack.Quantity > definitions.Get(ammoType).StackLimit)
      {
        continue;
      }

      inventory.SetSlot(index, stack.WithQuantity(stack.Quantity - 1));
      slot = index;
      return true;
    }

    return false;
  }
}
