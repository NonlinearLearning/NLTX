using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Definitions;

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
    if (state.CooldownTicks > 0 || state.UseRevision < 0 || state.UseRevision == int.MaxValue)
    {
      return false;
    }

    ItemStack stack = inventory.GetSlot(slot);
    if (stack.IsEmpty || !definitions.TryGet(stack.ItemType, out ItemDefinition definition) ||
        stack.Quantity > definition.StackLimit || health.Maximum < 0 ||
        health.Current < 0 || health.Current > health.Maximum ||
        health.Current >= health.Maximum)
    {
      return false;
    }

    int healthRestore = definition.Use is ItemUseDefinition useDefinition &&
      useDefinition.HealthRestore > 0
      ? useDefinition.HealthRestore
      : definition.HealthRestore;
    int cooldown = definition.Use is ItemUseDefinition use
      ? Math.Max(
        definition.UseCooldownTicks,
        Math.Max(use.CooldownTicks, use.ReuseDelayTicks))
      : definition.UseCooldownTicks;
    if (healthRestore <= 0 || health.Current >= health.Maximum)
    {
      return false;
    }

    health.Current = (int)Math.Min(
      (long)health.Maximum,
      (long)health.Current + healthRestore);
    if (definition.IsConsumable)
    {
      inventory.SetSlot(slot, stack.WithQuantity(stack.Quantity - 1));
    }

    state.CooldownTicks = cooldown;
    state.IsUsing = true;
    state.JustStarted = true;
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
    state.JustStarted = false;
  }
}
