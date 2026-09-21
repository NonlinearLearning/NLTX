using System;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerSentryEquipmentSystem
{
  public int CalculateCapacityBonus(
    EquipmentStateCollectionComponent equipmentStates,
    InventoryComponent inventory,
    ItemDefinitionRegistry itemDefinitions)
  {
    ArgumentNullException.ThrowIfNull(equipmentStates);
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(itemDefinitions);

    int totalCapacityBonus = 0;
    foreach (ItemEquipmentStateComponent state in equipmentStates.States.Values)
    {
      if (state.IsVanity || state.SourceSlot < 0 ||
          state.SourceSlot >= InventoryComponent.SlotCount)
      {
        continue;
      }

      ItemStack stack = inventory.GetSlot(state.SourceSlot);
      if (stack.IsEmpty || !itemDefinitions.TryGet(stack.ItemType, out ItemDefinition definition) ||
          stack.Quantity > definition.StackLimit ||
          definition.EquipmentSlot != state.Slot)
      {
        continue;
      }

      totalCapacityBonus = checked(totalCapacityBonus + definition.SentryCapacityBonus);
    }

    return totalCapacityBonus;
  }
}
