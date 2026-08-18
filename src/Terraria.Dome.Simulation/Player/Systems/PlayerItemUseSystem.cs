using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerItemUseSystem
{
  public bool TryUse(
    InventoryComponent inventory,
    ref ItemUseStateComponent state,
    ref HealthComponent health,
    int slot,
    ItemDefinitionRegistry definitions)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(definitions);
    if (state.CooldownTicks > 0)
    {
      return false;
    }

    ItemStack stack = inventory.GetSlot(slot);
    if (stack.IsEmpty || !definitions.TryGet(stack.ItemType, out ItemDefinition definition) ||
        definition.HealthRestore <= 0 || health.Current >= health.Maximum)
    {
      return false;
    }

    health.Current = Math.Min(health.Maximum, health.Current + definition.HealthRestore);
    inventory.SetSlot(slot, new ItemStack(stack.ItemType, stack.Quantity - 1));
    state.CooldownTicks = definition.UseCooldownTicks;
    state.IsUsing = true;
    state.UseRevision++;
    return true;
  }

  public void Tick(ref ItemUseStateComponent state)
  {
    if (state.CooldownTicks > 0)
    {
      state.CooldownTicks--;
    }

    state.IsUsing = state.CooldownTicks > 0;
  }
}
