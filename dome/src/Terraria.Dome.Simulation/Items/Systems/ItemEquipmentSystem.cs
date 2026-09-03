using System;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Events;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ItemEquipmentSystem
{
  public bool TryEquip(
    PlayerHandle player,
    ItemStack stack,
    int sourceSlot,
    ItemDefinition itemDefinition,
    ItemEquipmentStateComponent? existing,
    bool vanity,
    out ItemEquipmentStateComponent state,
    out ItemEquippedEvent equippedEvent,
    out ItemCommandRejection rejection)
  {
    if (!player.IsValid || stack.IsEmpty || stack.ItemType != itemDefinition.ItemType ||
        stack.Quantity > itemDefinition.StackLimit || sourceSlot < 0 ||
        sourceSlot >= InventoryComponent.SlotCount ||
        itemDefinition.Equipment is not ItemEquipmentDefinition definition ||
        !Enum.IsDefined(definition.Slot) ||
        definition.Slot == ItemEquipmentSlot.None ||
        existing.HasValue || vanity && !definition.Vanity)
    {
      state = default;
      equippedEvent = default;
      rejection = ItemCommandRejection.Invalid("The equipment slot is invalid or occupied.");
      return false;
    }

    state = new ItemEquipmentStateComponent(definition.Slot, sourceSlot, vanity);
    equippedEvent = new ItemEquippedEvent(
      player,
      stack.ItemType,
      definition.Slot,
      sourceSlot,
      vanity);
    rejection = default;
    return true;
  }
}
