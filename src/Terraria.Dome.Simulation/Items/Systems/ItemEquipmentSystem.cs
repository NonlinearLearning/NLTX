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
    ItemEquipmentDefinition definition,
    ItemEquipmentStateComponent? existing,
    bool vanity,
    out ItemEquipmentStateComponent state,
    out ItemEquippedEvent equippedEvent,
    out ItemCommandRejection rejection)
  {
    if (stack.IsEmpty || sourceSlot < 0 || definition.Slot == ItemEquipmentSlot.None ||
        existing.HasValue)
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
