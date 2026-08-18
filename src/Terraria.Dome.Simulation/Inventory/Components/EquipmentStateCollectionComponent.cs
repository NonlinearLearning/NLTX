using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Inventory.Components;

public sealed class EquipmentStateCollectionComponent
{
  private readonly Dictionary<ItemEquipmentSlot, ItemEquipmentStateComponent> _states = new();

  public IReadOnlyDictionary<ItemEquipmentSlot, ItemEquipmentStateComponent> States => _states;

  public long Revision { get; private set; }

  public bool Contains(ItemEquipmentSlot slot)
  {
    return _states.ContainsKey(slot);
  }

  public void Add(ItemEquipmentStateComponent state)
  {
    if (state.Slot == ItemEquipmentSlot.None)
    {
      throw new ArgumentException("Equipment state must target a concrete slot.", nameof(state));
    }

    _states.Add(state.Slot, state);
    Revision = checked(Revision + 1);
  }

  public bool Remove(ItemEquipmentSlot slot)
  {
    if (!_states.Remove(slot))
    {
      return false;
    }

    Revision = checked(Revision + 1);
    return true;
  }
}
