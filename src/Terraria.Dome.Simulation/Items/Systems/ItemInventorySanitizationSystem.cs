using System;
using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ItemInventorySanitizationSystem
{
  public void Sanitize(InventoryComponent inventory, ItemDefinitionRegistry definitions)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(definitions);

    for (int slotId = 0; slotId < InventoryComponent.SlotCount; slotId++)
    {
      SanitizeSlot(inventory, slotId, definitions);
    }
  }

  private static void SanitizeSlot(
    InventoryComponent inventory,
    int slotId,
    ItemDefinitionRegistry definitions)
  {
    ItemStack stack = inventory.GetSlot(slotId);
    if (stack.IsEmpty)
    {
      inventory.SetSlot(slotId, ItemStack.Empty);
      return;
    }

    if (!definitions.TryGet(stack.ItemType, out ItemDefinition definition))
    {
      inventory.SetSlot(slotId, ItemStack.Empty);
      return;
    }

    int quantity = Math.Min(stack.Quantity, definition.StackLimit);
    inventory.SetSlot(slotId, stack.WithQuantity(quantity));

    ItemInstanceStateComponent state = inventory.GetInstanceState(slotId);
    // Keep unrepresentable prefixes visible so V1456 persistence rejects them instead of truncating.
    if (state.PrefixId != 0 && state.PrefixId <= byte.MaxValue &&
        (definition.Prefixes is null || !definition.Prefixes.CanApply(state.PrefixId)))
    {
      inventory.SetInstanceState(slotId, state with { PrefixId = 0 });
    }
  }
}
