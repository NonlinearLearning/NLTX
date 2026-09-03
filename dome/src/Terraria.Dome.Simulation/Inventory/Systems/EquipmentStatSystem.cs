using System;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Inventory.Systems;

public sealed class EquipmentStatSystem
{
  public void Apply(
    ref DefenseComponent defense,
    ref HealthRegenerationComponent healthRegeneration,
    ref ManaComponent mana,
    EquipmentStateCollectionComponent equipmentStates,
    InventoryComponent inventory,
    ItemDefinitionRegistry itemDefinitions)
  {
    ArgumentNullException.ThrowIfNull(equipmentStates);
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(itemDefinitions);

    int totalDefense = 0;
    int totalLifeRegen = 0;
    int totalManaIncrease = 0;
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
          definition.Equipment is not ItemEquipmentDefinition equipment ||
          definition.EquipmentSlot != state.Slot)
      {
        continue;
      }

      totalDefense = checked(totalDefense + definition.OriginalDefense);
      totalLifeRegen = checked(totalLifeRegen + definition.LifeRegen);
      totalManaIncrease = checked(totalManaIncrease + definition.ManaIncrease);
    }

    defense.Value = totalDefense;
    healthRegeneration.SetEquipmentRegenUnitsPerTick(totalLifeRegen);
    int baseMaximumMana = mana.Maximum - mana.EquipmentIncrease;
    if (baseMaximumMana < 0)
    {
      baseMaximumMana = 0;
    }

    mana.Maximum = checked(baseMaximumMana + totalManaIncrease);
    mana.EquipmentIncrease = totalManaIncrease;
    if (mana.Current > mana.Maximum)
    {
      mana.Current = mana.Maximum;
    }
  }

  public void Apply(
    ref EquipmentLoadoutComponent loadout,
    byte selectedLoadout,
    ushort accessoryVisibility)
  {
    if (selectedLoadout > 2)
    {
      throw new ArgumentOutOfRangeException(nameof(selectedLoadout));
    }

    if (loadout.SelectedLoadout == selectedLoadout &&
        loadout.AccessoryVisibility == accessoryVisibility)
    {
      return;
    }

    if (loadout.Revision < 0 || loadout.Revision == int.MaxValue)
    {
      return;
    }

    loadout.SelectedLoadout = selectedLoadout;
    loadout.AccessoryVisibility = accessoryVisibility;
    loadout.Revision++;
  }
}
