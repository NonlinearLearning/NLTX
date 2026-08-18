using System;
using Terraria.Dome.Simulation.Inventory.Components;

namespace Terraria.Dome.Simulation.Inventory.Systems;

public sealed class EquipmentStatSystem
{
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

    loadout.SelectedLoadout = selectedLoadout;
    loadout.AccessoryVisibility = accessoryVisibility;
    loadout.Revision++;
  }
}
