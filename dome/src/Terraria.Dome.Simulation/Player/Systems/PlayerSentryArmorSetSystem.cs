using System;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Player.Definitions;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerSentryArmorSetSystem
{
  public int CalculateCapacityBonus(
    EquipmentStateCollectionComponent equipmentStates,
    InventoryComponent inventory,
    ItemDefinitionRegistry itemDefinitions)
  {
    ArgumentNullException.ThrowIfNull(equipmentStates);
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(itemDefinitions);

    if (!TryGetEquippedItemType(
          equipmentStates,
          inventory,
          itemDefinitions,
          ItemEquipmentSlot.Head,
          out ushort headItemType) ||
        !TryGetEquippedItemType(
          equipmentStates,
          inventory,
          itemDefinitions,
          ItemEquipmentSlot.Body,
          out ushort bodyItemType) ||
        !TryGetEquippedItemType(
          equipmentStates,
          inventory,
          itemDefinitions,
          ItemEquipmentSlot.Legs,
          out ushort legItemType) ||
        !LegacySentryArmorSetRegistry.TryGet(
          headItemType,
          bodyItemType,
          legItemType,
          out LegacySentryArmorSetDefinition definition)
      )
    {
      return 0;
    }

    return definition.CapacityBonus;
  }

  private static bool TryGetEquippedItemType(
    EquipmentStateCollectionComponent equipmentStates,
    InventoryComponent inventory,
    ItemDefinitionRegistry itemDefinitions,
    ItemEquipmentSlot slot,
    out ushort itemType)
  {
    itemType = 0;
    if (!equipmentStates.States.TryGetValue(
          slot,
          out ItemEquipmentStateComponent state) ||
        state.IsVanity ||
        state.SourceSlot < 0 ||
        state.SourceSlot >= InventoryComponent.SlotCount)
    {
      return false;
    }

    ItemStack stack = inventory.GetSlot(state.SourceSlot);
    if (stack.IsEmpty ||
        !itemDefinitions.TryGet(stack.ItemType, out ItemDefinition definition) ||
        stack.Quantity > definition.StackLimit ||
        definition.Equipment is not ItemEquipmentDefinition equipment ||
        definition.EquipmentSlot != slot)
    {
      return false;
    }

    itemType = stack.ItemType;
    return true;
  }
}
